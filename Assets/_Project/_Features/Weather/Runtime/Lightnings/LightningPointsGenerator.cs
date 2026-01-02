using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

[ExecuteAlways]
public class LightningPointsGenerator : MonoBehaviour
{
    [Header("Vertical (top -> bottom)")]
    public float BasePosY = 30f;     // entrée
    public float EndPosY = 0f;      // sortie
    [Min(2)] public int levels = 12; // nombre d'étapes (niveaux)

    [Tooltip("Jitter Y ajouté au pas de base entre niveaux (>=0). Ex: [0, 0.75]")]
    public Vector2 extraYStepRange = new Vector2(0f, 0.75f);

    [Header("Déplacement entre niveaux (step aléatoire)")]
    [Tooltip("Delta X aléatoire ajouté à CHAQUE niveau (peut être négatif)")]
    public Vector2 stepXRange = new Vector2(-0.6f, 0.6f);
    [Tooltip("Delta Z aléatoire ajouté à CHAQUE niveau (peut être négatif)")]
    public Vector2 stepZRange = new Vector2(-0.6f, 0.6f);

    [Header("Connecteurs horizontaux (points supplémentaires par niveau)")]
    [Min(1)] public int pointsPerLevel = 1;
    [Tooltip("Offset X aléatoire pour les points latéraux (autour du point central du niveau)")]
    public Vector2 lateralXRange = new Vector2(-0.8f, 0.8f);
    [Tooltip("Offset Z aléatoire pour les points latéraux (autour du point central du niveau)")]
    public Vector2 lateralZRange = new Vector2(-0.8f, 0.8f);

    [Header("Aléatoire")]
    public int seed = 0;
    public bool useSeed = false;

    // ----------------- Binder (identique, via reflection pour remplir Targets) ----------
    [Header("Binder Auto-Fill")]
    public VisualEffect targetVFX;
    public VFXPropertyBinder propertyBinder;
    public bool setEveryFrame = false;
    public string childNamePrefix = "Lightning_P";

    [ContextMenu("Assign Children To VFX Binder (Reflection)")]
    public void AssignChildrenToVFXBinder_Reflection()
    {
        if (!targetVFX) { Debug.LogWarning("targetVFX manquant"); return; }
        var binder = propertyBinder ? propertyBinder : targetVFX.GetComponent<VFXPropertyBinder>();
        if (!binder) { Debug.LogWarning("Aucun VFXPropertyBinder trouvé"); return; }

        var type = typeof(VFXPropertyBinder).Assembly.GetType("UnityEngine.VFX.Utility.VFXMultiplePositionBinder");
        if (type == null) { Debug.LogError("Type interne VFXMultiplePositionBinder introuvable"); return; }

        object multipleBinder = null;
        foreach (var c in binder.GetComponents(typeof(VFXBinderBase)))
            if (c.GetType() == type) { multipleBinder = c; break; }

        if (multipleBinder == null) { Debug.LogWarning("Aucun MultiplePositionBinder sur ce VFX"); return; }

        var targetsField = type.GetField("Targets", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var everyFrameField = type.GetField("EveryFrame", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (targetsField == null) { Debug.LogError("Champ Targets non trouvé"); return; }

        var children = new List<GameObject>();
        for (int i = 0; i < transform.childCount; i++)
            children.Add(transform.GetChild(i).gameObject);

        targetsField.SetValue(multipleBinder, children.ToArray());
        if (everyFrameField != null) everyFrameField.SetValue(multipleBinder, setEveryFrame);

#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(binder);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        Debug.Log($"Assigné {children.Count} points au MultiplePositionBinder (reflection)");
    }
    // ------------------------------------------------------------------------------------

    [ContextMenu("Regenerate Points")]
    public void Regenerate()
    {
        ClearChildren();

        var rnd = useSeed ? new System.Random(seed) : new System.Random();

        levels = Mathf.Max(2, levels);
        pointsPerLevel = Mathf.Max(1, pointsPerLevel);

        // Centre (x/z) de référence : on part de la position de ce GO
        Vector3 current = new Vector3(transform.position.x, BasePosY, transform.position.z);

        // pas vertical "linéaire" (toujours dans le bon sens) + jitter par niveau
        float sign = Mathf.Sign(EndPosY - BasePosY); // -1 si on descend, +1 si on monte
        float baseStepY = Mathf.Abs(EndPosY - BasePosY) / (levels - 1);

        int idx = 0;
        for (int L = 0; L < levels; L++)
        {
            if (L > 0)
            {
                // Step aléatoire par niveau
                float extraY = RandRange(rnd, Mathf.Min(extraYStepRange.x, extraYStepRange.y),
                                              Mathf.Max(extraYStepRange.x, extraYStepRange.y));
                float dx = RandRange(rnd, Mathf.Min(stepXRange.x, stepXRange.y),
                                          Mathf.Max(stepXRange.x, stepXRange.y));
                float dz = RandRange(rnd, Mathf.Min(stepZRange.x, stepZRange.y),
                                          Mathf.Max(stepZRange.x, stepZRange.y));

                current += new Vector3(dx, sign * (baseStepY + Mathf.Abs(extraY)), dz);
            }

            // Force le dernier point exactement à EndPosY
            if (L == levels - 1) current.y = EndPosY;

            // Point central du niveau
            CreateChildPoint($"{childNamePrefix}{idx++}", current);

            // Points latéraux optionnels (autour du point central)
            for (int k = 1; k < pointsPerLevel; k++)
            {
                float lx = RandRange(rnd, Mathf.Min(lateralXRange.x, lateralXRange.y),
                                          Mathf.Max(lateralXRange.x, lateralXRange.y));
                float lz = RandRange(rnd, Mathf.Min(lateralZRange.x, lateralZRange.y),
                                          Mathf.Max(lateralZRange.x, lateralZRange.y));

                Vector3 p = new Vector3(current.x + lx, current.y, current.z + lz);
                CreateChildPoint($"{childNamePrefix}{idx++}", p);
            }
        }
    }

    // --- Utils ----------------------------------------------------------------

    void CreateChildPoint(string name, Vector3 worldPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.position = worldPos; // en monde, le binder lit des positions monde
    }

    void ClearChildren()
    {
#if UNITY_EDITOR
        for (int i = transform.childCount - 1; i >= 0; --i)
            UnityEditor.Undo.DestroyObjectImmediate(transform.GetChild(i).gameObject);
#else
        for (int i = transform.childCount - 1; i >= 0; --i)
            Destroy(transform.GetChild(i).gameObject);
#endif
    }

    static float RandRange(System.Random r, float min, float max)
    {
        if (min > max) { var t = min; min = max; max = t; }
        return (float)(min + (max - min) * r.NextDouble());
    }
}

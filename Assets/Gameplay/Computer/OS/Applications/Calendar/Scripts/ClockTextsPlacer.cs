using UnityEngine;
using TMPro;

public class ClockTextPlacer : MonoBehaviour
{
    [Header("Prefab (doit être un asset)")]
    public TMP_Text textPrefab;

    [Header("Paramètres de cercle")]
    public int numberCount = 12;
    public float radius = 200f;
    public bool clockwise = true;

    public void PlaceTextsInCircle()
    {
#if UNITY_EDITOR
        // 🧹 Supprimer les anciens éléments
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        if (textPrefab == null)
        {
            Debug.LogError("Aucun prefab assigné !");
            return;
        }

        float angleStep = 360f / numberCount;

        for (int i = 0; i < numberCount; i++)
        {
            float angle = 90f - (clockwise ? angleStep * i : -angleStep * i);
            float rad = angle * Mathf.Deg2Rad;

            Vector3 pos = new Vector3(
                Mathf.Cos(rad) * radius,
                Mathf.Sin(rad) * radius,
                0f
            );

            // ✅ Crée une instance du prefab (asset)
            TMP_Text newText = (TMP_Text)UnityEditor.PrefabUtility.InstantiatePrefab(textPrefab, transform);

            newText.transform.localPosition = pos;

            int displayNumber = i == 0 ? 12 : i;
            newText.text = displayNumber.ToString();
            newText.alignment = TextAlignmentOptions.Center;
            newText.gameObject.name = "Clock_" + displayNumber;
        }
#endif
    }
}

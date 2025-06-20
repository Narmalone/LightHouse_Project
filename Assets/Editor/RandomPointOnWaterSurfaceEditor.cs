using UnityEngine;
using UnityEditor;
using LightHouse.Game.WaterExtension;

[CustomEditor(typeof(RandomPointOnWaterSurface))]
public class RandomPointOnWaterSurfaceEditor : Editor
{
    private void OnSceneGUI()
    {
        RandomPointOnWaterSurface r = (RandomPointOnWaterSurface)target;
        if (r == null || !r.enabled || r.CenterPoint == null) return;

        Vector3 center = r.CenterPoint;
        float radius = r.radius;
        float arc = r.arcAngle;
        float startAngle = r.startingAngle;

        // Dessiner le disque circulaire complet en transparent
        Handles.color = new Color(0f, 1f, 1f, 0.1f);
        Handles.DrawSolidDisc(center, Vector3.up, radius);

        // Arc d'orientation
        float start = startAngle - arc * 0.5f;
        Handles.color = Color.white;
        Handles.DrawWireArc(center, Vector3.up, Quaternion.Euler(0, start, 0) * Vector3.forward, arc, radius);

        // Directions
        Vector3 dirEntry = (r.EntryPoint - center).normalized;
        Vector3 dirExit = (r.ExitPoint - center).normalized;

        Handles.color = Color.green;
        Handles.DrawLine(center, r.EntryPoint);
        Handles.SphereHandleCap(0, r.EntryPoint, Quaternion.identity, 1.5f, EventType.Repaint);

        Handles.color = Color.red;
        Handles.DrawLine(center, r.ExitPoint);
        Handles.SphereHandleCap(0, r.ExitPoint, Quaternion.identity, 1.5f, EventType.Repaint);

        // Direction centrale
        Handles.color = Color.yellow;
        Vector3 central = Quaternion.Euler(0, startAngle, 0) * Vector3.forward;
        Handles.DrawLine(center, center + central * radius);
    }
}

using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SceneRectangle))]
public class SceneRectangleEditor : Editor
{
    void OnSceneGUI()
    {
        SceneRectangle t = (SceneRectangle)target;
        Vector3 pos = t.transform.position;

        Vector3[] verts = new Vector3[]
        {
            t.unscaledMin,
            t.unscaledMin + new Vector3(t.unscaledMax.x - t.unscaledMin.x, 0, 0),
            t.unscaledMax,
            t.unscaledMin + new Vector3(0, 0, t.unscaledMax.z - t.unscaledMin.z),
        };

        Handles.DrawSolidRectangleWithOutline(verts, new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0, 0, 0, 1));

        EditorGUI.BeginChangeCheck();
        Vector3 newMin = Handles.PositionHandle(t.unscaledMin, Quaternion.identity);
        Vector3 newMax = Handles.PositionHandle(t.unscaledMax, Quaternion.identity);
        newMin.y = newMax.y = 0;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(t, "Change scene rectangle");
            t.unscaledMin = Vector3.Min(newMin, newMax);
            t.unscaledMax = Vector3.Max(newMin, newMax);
        }
    }
}
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
            t.min,
            t.min + new Vector3(t.max.x - t.min.x, 0, 0),
            t.max,
            t.min + new Vector3(0, 0, t.max.z - t.min.z),
        };

        Handles.DrawSolidRectangleWithOutline(verts, new Color(0.5f, 0.5f, 0.5f, 0.1f), new Color(0, 0, 0, 1));

        EditorGUI.BeginChangeCheck();
        Vector3 newMin = Handles.PositionHandle(t.min, Quaternion.identity);
        Vector3 newMax = Handles.PositionHandle(t.max, Quaternion.identity);
        newMin.y = newMax.y = 0;
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(t, "Change scene rectangle");
            t.min = Vector3.Min(newMin, newMax);
            t.max = Vector3.Max(newMin, newMax);
        }
    }
}
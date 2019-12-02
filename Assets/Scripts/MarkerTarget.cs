using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class MarkerTarget : MonoBehaviour
{
    [Tooltip("Vuforia instance ID of the marker that should cause this object to be enabled.")]
    public int markerId = 0;
    public bool spin = false;

    void OnValidate()
    {
        foreach(Text t in GetComponentsInChildren<Text>())
        {
            t.text = markerId.ToString();
        }
    }

    void Update()
    {
        bool playMode = true;
#if UNITY_EDITOR
        playMode = UnityEditor.EditorApplication.isPlaying;
#endif

        if (playMode && spin)
        {
            transform.Rotate(0, 90 * Time.deltaTime, 0, Space.Self);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarkerTarget : MonoBehaviour
{
    [Tooltip("Vuforia instance ID of the marker that should cause this object to be enabled.")]
    public int markerId = 0;
}

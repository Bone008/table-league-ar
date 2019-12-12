using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneRectangle : MonoBehaviour
{
    public Vector3 min;
    public Vector3 max;
    public Vector3 center => (max + min) / 2;

    /// <summary>Performs a 2D test if a point is contained in this rectangle.</summary>
    public bool Contains(Vector3 point)
    {
        return point.x >= min.x && point.x <= max.x
            && point.z >= min.z && point.z <= max.z;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SceneRectangle : MonoBehaviour
{
    [FormerlySerializedAs("min")]
    public Vector3 unscaledMin;
    [FormerlySerializedAs("max")]
    public Vector3 unscaledMax;

    public Vector3 min => unscaledMin * Scale.gameScale;
    public Vector3 max => unscaledMax * Scale.gameScale;
    public Vector3 center => (unscaledMin + unscaledMax) / 2 * Scale.gameScale;

    /// <summary>List of locations where resources can spawn within this rectangle.</summary>
    public Transform[] resourceSpawnPoints;

    /// <summary>Performs a 2D test if a point is contained in this rectangle.</summary>
    public bool Contains(Vector3 point)
    {
        return point.x >= min.x && point.x <= max.x
            && point.z >= min.z && point.z <= max.z;
    }

    /// <summary>Projects the X and Z coordinates of a point to lie within this rectangle.</summary>
    /// <param name="padding">minimum necessary distance from the border</param>
    /// <returns>the projected vector</returns>
    public Vector3 ProjectPoint(Vector3 point, float padding = 0)
    {
        Rect rect2d = new Rect(min.x + padding, min.z + padding, max.x - min.x - 2*padding, max.z - min.z - 2*padding);
        var projected2d = Util.ProjectPointOntoRect(rect2d, new Vector2(point.x, point.z));
        return new Vector3(projected2d.x, point.y, projected2d.y);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffScreenIndicator : MonoBehaviour
{
    public Behaviour activeTarget1;
    public Behaviour activeTarget2;
    public float screenMarginPx;

    // Needs to be set by other script upon instantiating.
    public Transform targetTransform;
    
    void LateUpdate()
    {
        if (targetTransform == null)
            return;

        Vector2 screenSize = Camera.main.ViewportToScreenPoint(Vector2.one);
        Vector3 screenPos3D = Camera.main.WorldToScreenPoint(targetTransform.position);
        Vector2 screenPos = screenPos3D;

        if(screenPos3D.z < 0 || screenPos.x < 0 || screenPos.y < 0 || screenPos.x >= screenSize.x || screenPos.y >= screenSize.y)
        {
            // When ball is behind the camera, its projection is on the opposite side of the screen by default.
            screenPos *= Mathf.Sign(screenPos3D.z);
            // Find position along the screen's edges, respecting a margin.
            Rect safeScreenRect = new Rect(screenMarginPx * Vector2.one, screenSize - 2 * screenMarginPx * Vector2.one);
            screenPos = FindPointOnRect(safeScreenRect, screenPos);

            // Seting RectTransform's position will ignore the Canvas scaling, which is what we want.
            transform.position = screenPos;
            activeTarget1.enabled = true;
            activeTarget2.enabled = true;
        }
        else
        {
            activeTarget1.enabled = false;
            activeTarget2.enabled = false;
        }
    }

    /// <summary>
    /// Projects point onto the bounds of a rectangle, preserving directions.
    /// </summary>
    private static Vector2 FindPointOnRect(Rect rect, Vector2 point)
    {
        Vector2 rectHalfSize = rect.size / 2;
        Vector2 dir = (point - rect.center).normalized;
        if (dir.x == 0)
            dir.x = 1e-4f;

        float dirRatio = dir.y / dir.x;
        Vector2 p;
        // Expand direction from center to right edge of rect.
        // Then check if its y coordinate is within the vertical bounds.
        // If yes --> left or right edge. If no --> top or bottom edge.
        if(Mathf.Abs(dirRatio) * rectHalfSize.x < rectHalfSize.y)
        {
            // Point is somewhere on left or right edge.
            p.x = Mathf.Sign(dir.x) * rectHalfSize.x;
            p.y = Mathf.Sign(dir.x) * dirRatio * rectHalfSize.x;
        }
        else
        {
            // Point is somewhere on top or bottom edge.
            p.y = Mathf.Sign(dir.y) * rectHalfSize.y;
            p.x = Mathf.Sign(dir.y) * rectHalfSize.y / dirRatio;
        }

        return rect.center + p;
    }
}

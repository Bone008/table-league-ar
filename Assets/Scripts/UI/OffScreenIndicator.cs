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
            screenPos = Util.ProjectPointOntoRect(safeScreenRect, screenPos);

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
}

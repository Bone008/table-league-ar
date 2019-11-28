using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInteractionPreview : MonoBehaviour
{
    public PlayerInputController controller;
    public Renderer visual;

    void LateUpdate()
    {
        bool inRange = (controller.transform.position - transform.position).sqrMagnitude < controller.maxInteractionRange * controller.maxInteractionRange;

        visual.enabled = inRange;
        if (inRange)
        {
            Vector3 rot = controller.transform.eulerAngles;
            // Only allow angling upwards, not downwards.
            if (rot.x < 180)
                rot.x = 0;
            transform.eulerAngles = rot;
        }
    }
}

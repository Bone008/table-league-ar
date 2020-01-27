using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInteractionPreview : MonoBehaviour
{
    public Renderer visual;
    public Material validMaterial;
    public Material invalidMaterial;

    void LateUpdate()
    {
        var controller = PlayerNetController.LocalInstance;
        if (controller == null || controller.player == null)
            return;

        float maxRange = PlayerInputController.s_unscaledMaxInteractionRange * Scale.gameScale;
        bool inRange = (controller.transform.position - transform.position).sqrMagnitude < maxRange * maxRange;
        bool inControl = controller.player.ownedRectangle.Contains(transform.position);

        visual.enabled = inRange;
        visual.sharedMaterial = inControl ? validMaterial : invalidMaterial;
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

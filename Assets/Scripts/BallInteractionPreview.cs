using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInteractionPreview : MonoBehaviour
{
    // TODO: This somehow needs to be kept in sync with the value in PlayerInputController :(
    // Not sure how to read it directly from there though ...
    public float maxInteractionRange = 1.5f;

    public Renderer visual;
    public Material validMaterial;
    public Material invalidMaterial;

    void LateUpdate()
    {
        var controller = PlayerNetController.LocalInstance;
        if (controller == null || controller.player == null)
            return;

        bool inRange = (controller.transform.position - transform.position).sqrMagnitude < maxInteractionRange * maxInteractionRange;
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

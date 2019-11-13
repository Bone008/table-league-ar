using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInteractionPreview : MonoBehaviour
{
    public PlayerInputController player;
    public Renderer visual;
    
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        bool inRange = (player.transform.position - transform.position).sqrMagnitude < player.maxInteractionRange * player.maxInteractionRange;

        visual.enabled = inRange;
        if(inRange)
        {
            Vector3 rot = player.transform.eulerAngles;
            rot.x = 0; // TODO allow pointing upwards
            transform.eulerAngles = rot;
        }
    }
}

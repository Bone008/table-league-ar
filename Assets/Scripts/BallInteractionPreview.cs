using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInteractionPreview : MonoBehaviour
{
    public PlayerInputController controller;
    public Renderer visual;
    
    void LateUpdate()
    {
        Vector3 pos = transform.position;
        bool inRange = (controller.transform.position - transform.position).sqrMagnitude < controller.maxInteractionRange * controller.maxInteractionRange;

        visual.enabled = inRange;
        if(inRange)
        {
            Vector3 rot = controller.transform.eulerAngles;
            rot.x = 0; // TODO allow pointing upwards
            transform.eulerAngles = rot;
        }
    }
}

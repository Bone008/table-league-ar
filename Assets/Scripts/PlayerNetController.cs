using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetController : NetworkBehaviour
{
    public Player gamePlayer;

    public override void OnStartLocalPlayer()
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }
        Camera.main.GetComponent<PlayerInputController>().netPlayer = this;
    }

    void OnDestroy()
    {
        Camera.main.GetComponent<PlayerInputController>().netPlayer = null;
    }

    void LateUpdate()
    {
        if (isLocalPlayer)
        {
            transform.position = Camera.main.transform.position;
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    [Command]
    public void CmdHitBall(GameObject ball, Vector3 force)
    {
        var rigidbody = ball.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(force, ForceMode.Impulse);
    }
    
}

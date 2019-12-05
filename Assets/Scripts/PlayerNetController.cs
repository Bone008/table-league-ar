using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Client-authorative script for networked players that processes commands/RPCs,
/// performs validation and forwards the commands to the Player script.
/// This separation has the following advantages:
///   * It allows performing player actions, even when there is no actual client connected
///     (such as from the BotInput script in singleplayer).
///   * It allows clients to reconnect to the game without losing any state stored in Player.
/// </summary>
public class PlayerNetController : NetworkBehaviour
{
    /// <summary>Assigned on the server when this controller is instantiated and assigned to a player.</summary>
    public Player player;

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
            // Follow the camera. We cannot just attach to it, since NetworkTransform
            // synchronizes localPosition, which would otherwise not change.
            transform.position = Camera.main.transform.position;
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    [Command]
    public void CmdHitBall(GameObject ball, Vector3 force)
    {
        if(!ball.CompareTag(Constants.BALL_TAG))
        {
            Debug.LogWarning("Tried to hit something that was not a ball: " + ball, ball);
            return;
        }
        player.HitBall(ball, force);
    }

    [Command]
    public void CmdStartBuildTower(TowerType type, Vector3 position, float rotationAngle)
    {
        player.StartBuildTower(type, position, rotationAngle);
    }
    
}

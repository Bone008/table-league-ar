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
    /// <summary>Convenience accessor for the client</summary>
    public static PlayerNetController LocalInstance { get; private set; }

    /// <summary>Assigned on the server when this controller is instantiated and assigned to a player.</summary>
    public Player player;

    /// <summary>This property allows the client to determine which player they are.</summary>
    [SyncVar]
    public int playerId;

    public override void OnStartLocalPlayer()
    {
        LocalInstance = this;

        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = false;
        }

        Debug.Log("This client is controlling player " + playerId);
        player = (playerId == 1 ? GameManager.Instance.player1 : GameManager.Instance.player2);
    }

    void OnDestroy()
    {
        LocalInstance = null;
    }

    void LateUpdate()
    {
        if (!isLocalPlayer)
            return;

        // Follow the camera. We cannot just attach to it, since NetworkTransform
        // synchronizes localPosition, which would otherwise not change.
        transform.position = Camera.main.transform.position;
        transform.rotation = Camera.main.transform.rotation;
    }

    // Called by the client when clicking on a ball.
    [Command]
    public void CmdHitBall(GameObject ball, Vector3 force)
    {
        if (!ball.CompareTag(Constants.BALL_TAG))
        {
            Debug.LogWarning("Tried to hit something that was not a ball: " + ball, ball);
            return;
        }
        player.HitBall(ball, force);
    }

    // Called by the client when clicking on a collectable.
    [Command]
    public void CmdStartCollect(GameObject collectable)
    {
        player.StartCollect(collectable);
    }

    [Command]
    public void CmdStartBuildTower(TowerType type, Vector3 position, float rotationAngle)
    {
        player.StartBuildTower(type, position, rotationAngle);
    }

    // Called by the client when the click/touch is released while collecting or building something.
    [Command]
    public void CmdCancelInteraction()
    {
        player.CancelInteraction();
    }

}

using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public int playerId;
    public Transform homeAreaAnchor;

    public string playerName => "Player " + playerId;

    [SyncVar]
    public int score = 0;
    [SyncVar]
    public int resources = 0;

    [Server]
    public void HitBall(GameObject ball, Vector3 force)
    {
        var rigidbody = ball.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(force, ForceMode.Impulse);
    }

    [Server]
    public void StartBuildTower(TowerType type, Vector3 position, float rotationAngle)
    {
        Debug.Log("SERVER starting to build tower " + type);
    }

    [Server]
    public void CancelBuild()
    {
        Debug.Log("SERVER cancelling build");
    }

}

using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main script for handling player actions on the server and remembering state about players.
/// There always exist 2 game objects with a Player, no matter the network connection state
/// and if the player is controlled by a real person or a bot.
/// </summary>
public class Player : NetworkBehaviour
{
    public int playerId;
    public Transform homeAreaAnchor;

    public string playerName => "Player " + playerId;

    [SyncVar]
    public int score = 0;
    [SyncVar]
    public int resources = 0;

    private float timerStart = 0;
    private Collectable activeCollectable = null;


    [ServerCallback]
    void Update()
    {
        if (activeCollectable && Time.time > timerStart + activeCollectable.collectDuration)
        {
            Debug.Log("finished collecting");
            switch(activeCollectable.type)
            {
                case CollectableType.TowerResource:
                    resources += 10;
                    SpawnManager.Instance.NotifyResourceCollected();
                    break;
            }
            NetworkServer.Destroy(activeCollectable.gameObject);
            activeCollectable = null;
        }
    }

    [Server]
    public void HitBall(GameObject ball, Vector3 force)
    {
        var rigidbody = ball.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(force, ForceMode.Impulse);
    }

    [Server]
    public void StartCollect(GameObject target)
    {
        var collectable = target.GetComponent<Collectable>();
        if (collectable == null)
        {
            Debug.LogWarning("Cannot collect this game object!", target);
            return;
        }
        Debug.Log("SERVER starting to collect", collectable);

        collectable.StartCollecting(this);
        activeCollectable = collectable;
        timerStart = Time.time;
    }

    [Server]
    public void StartBuildTower(TowerType type, Vector3 position, float rotationAngle)
    {
        Debug.Log("NOT IMPLEMENTED: SERVER starting to build tower " + type);
    }

    [Server]
    public void CancelInteraction()
    {
        if(activeCollectable)
        {
            activeCollectable.StopCollecting();
            activeCollectable = null;
        }
    }
}

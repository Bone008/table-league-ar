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
    /// <summary>Location where balls in control of that player should spawn.</summary>
    public Transform homeAreaAnchor;
    /// <summary>Area of the playing field that the player is in control of.</summary>
    public SceneRectangle ownedRectangle;

    public string playerName => "Player " + playerId;

    /// <summary>Indicates if the player has tracking and has indicated that they are (still) ready to play.</summary>
    [SyncVar]
    public bool isUserReady = false;
    [SyncVar]
    public int score = 0;
    [SyncVar]
    public int resources = 0;

    private float timerStart = 0;
    private Collectable activeCollectable = null;

    private GameObject activeBuildTower;
    private TowerType activeType = TowerType.None;

    [ServerCallback]
    void Update()
    {
        if (activeCollectable && Time.time > timerStart + activeCollectable.collectDuration)
        {
            EffectsManager.Instance.RpcHideInteraction();

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

        if(activeType != TowerType.None && Time.time > timerStart + TowerManager.Instance.buildTime)
        {
            EffectsManager.Instance.RpcHideInteraction();

            resources -= Constants.towerCost;
            var newTower = Instantiate(TowerManager.Instance.getTower(activeType), activeBuildTower.transform.position, activeBuildTower.transform.rotation);
            NetworkServer.Spawn(newTower);
            NetworkServer.Destroy(activeBuildTower);
            activeBuildTower = null;
            activeType = TowerType.None;
        }
    }

    [Server]
    public void HitBall(GameObject ball, Vector3 force)
    {
        if(!ownedRectangle.Contains(ball.transform.position))
        {
            Debug.Log("Player " + playerId + ": Cannot hit ball outside of their owned rectangle.");
            return;
        }

        var rigidbody = ball.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(force, ForceMode.Impulse);
    }

    [Server]
    public void StartCollect(GameObject target)
    {
        CancelInteraction();
        var collectable = target.GetComponent<Collectable>();
        if (collectable == null)
        {
            Debug.LogWarning("Cannot collect this game object!", target);
            return;
        }
        if (!ownedRectangle.Contains(collectable.transform.position))
        {
            Debug.LogWarning("Player tried to collect outside of owned rectangle!");
            return;
        }
        Debug.Log("SERVER starting to collect", collectable);

        collectable.StartCollecting(this);
        activeCollectable = collectable;
        timerStart = Time.time;
        EffectsManager.Instance.RpcShowInteraction(gameObject, collectable.transform.position);
    }

    [Server]
    public void StartBuildTower(TowerType type, Vector3 position, Quaternion rotationAngle)
    {
        if(resources < Constants.towerCost)
        {
            Debug.LogWarning("Not enough resources to build a tower!", this);
            return;
        }
        if(!ownedRectangle.Contains(position))
        {
            Debug.LogWarning("Player tried to build outside of owned rectangle!");
            return;
        }
        CancelInteraction();

        activeBuildTower = Instantiate(TowerManager.Instance.getTowerPreview(type), position, rotationAngle);
        NetworkServer.Spawn(activeBuildTower);
        activeType = type;
        timerStart = Time.time;

        RpcPlayBuildEffect(activeBuildTower);
        EffectsManager.Instance.RpcShowInteraction(gameObject, position);
    }

    [ClientRpc]
    private void RpcPlayBuildEffect(GameObject towerPreview)
    {
        var particles = towerPreview.GetComponentInChildren<ParticleSystem>();
        if(particles == null)
        {
            Debug.LogWarning("Could not find particles to play build effect on preview!", towerPreview);
            return;
        }
        particles.Play();
    }

    [Server]
    public void CancelInteraction()
    {
        EffectsManager.Instance.RpcHideInteraction();
        if(activeCollectable)
        {
            activeCollectable.StopCollecting();
            activeCollectable = null;
        }

        if(activeType != TowerType.None)
        {
            NetworkServer.Destroy(activeBuildTower);
            activeBuildTower = null;
            activeType = TowerType.None;
        }
    }
}

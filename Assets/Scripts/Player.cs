using Mirror;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main script for handling player actions on the server and remembering state about players.
/// There always exist 2 game objects with a Player, no matter the network connection state
/// and if the player is controlled by a real person or a bot.
/// </summary>
public class Player : NetworkBehaviour
{
    public class SyncDictionaryCollectableInt : SyncDictionary<CollectableType, int> { }

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
    private SyncDictionaryCollectableInt inventory = new SyncDictionaryCollectableInt();

    private float timerStart = 0;
    private Collectable activeCollectable = null;

    private GameObject activeBuildTower = null;
    private GameObject activeDestroyingTower = null;
    private TowerType activeType = TowerType.None;

    /// <summary>Returns how many items of the given type the player has in their inventory.</summary>
    public int GetInventoryCount(CollectableType type)
    {
        if (type == CollectableType.None) throw new ArgumentException("cannot lookup invalid type");
        inventory.TryGetValue(type, out int value);
        return value;
    }

    /// <summary>Server-side method to add a collectable to the player's inventory.</summary>
    [Server]
    public void AddToInventory(CollectableType type, int amount)
    {
        if (type == CollectableType.None) throw new ArgumentException("cannot add invalid type");
        inventory.TryGetValue(type, out int value);
        inventory[type] = value + amount;
    }

    /// <summary>Server-side method to consume a collectable from the player's inventory.</summary>
    /// <returns>False if not enough available, true if consumption was successful.</returns>
    [Server]
    public bool ConsumeFromInventory(CollectableType type, int amount)
    {
        if (type == CollectableType.None) throw new ArgumentException("cannot consume invalid type");
        inventory.TryGetValue(type, out int value);

        if (value < amount) return false;
        inventory[type] = value - amount;
        return true;
    }

    [ServerCallback]
    void Update()
    {
        // Finish collecting.
        if (activeCollectable && Time.time > timerStart + activeCollectable.collectDuration)
        {
            EffectsManager.Instance.RpcHideInteraction();
            AddToInventory(activeCollectable.type, activeCollectable.amount);

            NetworkServer.Destroy(activeCollectable.gameObject);
            activeCollectable = null;
        }

        // Finish building tower.
        if(activeType != TowerType.None && Time.time > timerStart + TowerManager.Instance.buildTime)
        {
            EffectsManager.Instance.RpcHideInteraction();

            if (ConsumeFromInventory(CollectableType.TowerResource, Constants.towerCost))
            {
                var newTower = Instantiate(TowerManager.Instance.getTower(activeType), activeBuildTower.transform.position, activeBuildTower.transform.rotation);
                NetworkServer.Spawn(newTower);
            }
            else { Debug.LogWarning("Not enough resources to finish building tower!"); }

            NetworkServer.Destroy(activeBuildTower);
            activeBuildTower = null;
            activeType = TowerType.None;
        }

        // Finish destroying tower.
        float destroyGraceTime = TowerManager.Instance.destroyEffectOnlyTime;
        if(activeDestroyingTower != null && Time.time > timerStart + TowerManager.Instance.destroyTime - destroyGraceTime)
        {
            GameObject tower = activeDestroyingTower;
            activeDestroyingTower = null;
            // Wait a bit longer for the clients to finish playing the scale effect before actually destroying the tower.
            // During this time, the destruction should no longer be cancelable. Since network delay can only 
            this.Delayed(destroyGraceTime, () => NetworkServer.Destroy(tower));
        }
    }

    [Server]
    public void HitBall(GameObject ball, Vector3 force)
    {
        if (!GameManager.Instance.isRunning) return;
        if (!ownedRectangle.Contains(ball.transform.position))
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
        if (!GameManager.Instance.isRunning) return;

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

        collectable.StartCollecting(this);
        activeCollectable = collectable;
        timerStart = Time.time;
        EffectsManager.Instance.RpcShowInteraction(gameObject, collectable.transform.position);
    }

    [Server]
    public void StartBuildTower(TowerType type, Vector3 position, Quaternion rotationAngle)
    {
        if (!GameManager.Instance.isRunning) return;

        if (GetInventoryCount(CollectableType.TowerResource) < Constants.towerCost)
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

    [Server]
    public void StartDestroyTower(GameObject tower)
    {
        if (!ownedRectangle.Contains(tower.transform.position))
        {
            Debug.LogWarning("Player tried to destroy tower outside of owned rectangle!");
            return;
        }
        Debug.Log("[SERVER] starting to destroy tower", tower);

        CancelInteraction();
        activeDestroyingTower = tower;
        timerStart = Time.time;

        EffectsManager.Instance.RpcPlayTowerDestroyEffect(tower, TowerManager.Instance.destroyTime);
        EffectsManager.Instance.RpcShowInteraction(gameObject, tower.transform.position);
    }

    [ClientRpc]
    private void RpcPlayBuildEffect(GameObject towerPreview)
    {
        var particles = towerPreview.GetComponentsInChildren<ParticleSystem>();
        if(particles == null)
        {
            Debug.LogWarning("Could not find particles to play build effect on preview!", towerPreview);
            return;
        }
        foreach(var p in particles)
        {
            p.Play();
        }
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

        if(activeDestroyingTower != null)
        {
            EffectsManager.Instance.RpcStopTowerDestroyEffect(activeDestroyingTower);
            activeDestroyingTower = null;
        }
    }

    [Server]
    public void UsePowerupFreeze()
    {
        if (!ConsumeFromInventory(CollectableType.PowerupFreeze, 1))
            return;

        foreach(Ball ball in GameManager.Instance.balls)
        {
            ball.Freeze(Constants.freezeBallDuration);
        }
    }
}

﻿using Mirror;
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
    /// <summary>Transform of the GameObject that controls this player (the player's camera or the bot). Can be null!</summary>
    public Transform controllerTransform { get; set; }

    /// <summary>Only available on server.</summary>
    public GameStatistics statistics { get; } = new GameStatistics();

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

    /// <summary>C# event to listen for inventory updates.</summary>
    public event SyncDictionaryCollectableInt.SyncDictionaryChanged InventoryChange
    {
        add { inventory.Callback += value; }
        remove { inventory.Callback -= value; }
    }

    [ServerCallback]
    void Start()
    {
        statistics.playerId = playerId;
        StartCoroutine(MeasureDistanceLoop());
    }

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

    [Server]
    private IEnumerator MeasureDistanceLoop()
    {
        Vector3 lastPos = new Vector3(0, 0, 0);

        while (true)
        {
            if (controllerTransform == null)
            {
                //Debug.LogWarning("ControllerTransform was not assigned!", this);
            }
            else
            {
                statistics.distanceTravelled += Vector3.Distance(controllerTransform.position, lastPos);
                lastPos = controllerTransform.position;
                //Debug.Log(statistics.distanceTravelled);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    [ServerCallback]
    void Update()
    {
        // Finish collecting.
        if (activeCollectable && Time.time > timerStart + activeCollectable.collectDuration)
        {
            EffectsManager.Instance.RpcHideInteraction();
            SoundManager.Instance.RpcStopSoundPlayer(SoundEffect.CollectableCollecting, playerId);
            SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.CollectableCollected, playerId);

            AddToInventory(activeCollectable.type, activeCollectable.amount);

            NetworkServer.Destroy(activeCollectable.gameObject);
            activeCollectable = null;
        }

        // Finish building tower.
        if (activeType != TowerType.None && Time.time > timerStart + TowerManager.Instance.buildTime)
        {
            EffectsManager.Instance.RpcHideInteraction();
            SoundManager.Instance.RpcStopSoundPlayer(SoundEffect.TowerBuilding, playerId);

            int cost = TowerManager.Instance.getTowerCost(activeType);
            if (ConsumeFromInventory(CollectableType.TowerResource, cost))
            {
                var newTower = Instantiate(TowerManager.Instance.getTower(activeType), activeBuildTower.transform.position, activeBuildTower.transform.rotation);
                newTower.GetComponent<TowerBase>().owner = this;
                NetworkServer.Spawn(newTower);
            }
            else { Debug.LogWarning("Not enough resources to finish building tower!"); }

            NetworkServer.Destroy(activeBuildTower);
            activeBuildTower = null;
            activeType = TowerType.None;
        }

        // Finish destroying tower.
        float destroyGraceTime = TowerManager.Instance.destroyEffectOnlyTime;
        if (activeDestroyingTower != null && Time.time > timerStart + TowerManager.Instance.destroyTime - destroyGraceTime)
        {
            GameObject tower = activeDestroyingTower;
            activeDestroyingTower = null;
            // Wait a bit longer for the clients to finish playing the scale effect before actually destroying the tower.
            // During this time, the destruction should no longer be cancelable. Since network delay can only 
            this.Delayed(destroyGraceTime, () =>
            {
                NetworkServer.Destroy(tower);
                SoundManager.Instance.RpcStopSoundPlayer(SoundEffect.TowerDestroying, playerId);
            });
        }
    }

    [Server]
    public void HitBall(GameObject ball, Vector3 force)
    {
        if (!GameManager.Instance.isRunning) return;
        if (!ownedRectangle.Contains(ball.transform.position) && !GameManager.Instance.allowCheats)
        {
            Debug.Log("Player " + playerId + ": Cannot hit ball outside of their owned rectangle.");
            return;
        }

        ball.GetComponent<Ball>().Hit();

        var rigidbody = ball.GetComponent<Rigidbody>();

        Vector3 pos = rigidbody.position;
        GameObject[] goals = GameObject.FindGameObjectsWithTag(Constants.GOAL_TAG);

        foreach (GameObject g in goals)
        {
            if (g.GetComponent<Goal>().owner.playerId == playerId)
            {
                if (Math.Abs(pos.x - g.transform.position.x) < Constants.scaledDistanceFromGoal && Math.Abs(rigidbody.velocity.x) > Constants.scaledDallVelocity)
                {
                    Debug.Log("Player: " + playerId + " " + "Distance: " + Math.Abs(pos.x - g.transform.position.x) + " Velocity: " + Math.Abs(rigidbody.velocity.x));
                    SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.NiceSave, playerId);
                    statistics.saves += 1;
                }
            }
        }

        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(force, ForceMode.Impulse);
        SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.BallHit, playerId);
        statistics.numberOfBallHits += 1;
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
        if (!ownedRectangle.Contains(collectable.transform.position) && !GameManager.Instance.allowCheats)
        {
            Debug.LogWarning("Player tried to collect outside of owned rectangle!");
            return;
        }

        collectable.StartCollecting(this);
        activeCollectable = collectable;
        timerStart = Time.time;
        EffectsManager.Instance.RpcShowInteraction(gameObject, collectable.transform.position);
        SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.CollectableCollecting, playerId);
    }

    [Server]
    public void StartBuildTower(TowerType type, Vector3 position, Quaternion rotationAngle)
    {
        if (!GameManager.Instance.isRunning) return;

        int cost = TowerManager.Instance.getTowerCost(activeType);
        if (GetInventoryCount(CollectableType.TowerResource) < cost)
        {
            Debug.LogWarning("Not enough resources to build a tower!", this);
            return;
        }
        if (!ownedRectangle.Contains(position) && !GameManager.Instance.allowCheats)
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
        SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.TowerBuilding, playerId);
        statistics.towersBuilt += 1;
    }

    [Server]
    public void StartDestroyTower(GameObject tower)
    {
        if (!GameManager.Instance.isRunning) return;
        if (!ownedRectangle.Contains(tower.transform.position) && !GameManager.Instance.allowCheats)
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
        SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.TowerDestroying, playerId);
    }

    [ClientRpc]
    private void RpcPlayBuildEffect(GameObject towerPreview)
    {
        var particles = towerPreview.GetComponentsInChildren<ParticleSystem>();
        if (particles == null)
        {
            Debug.LogWarning("Could not find particles to play build effect on preview!", towerPreview);
            return;
        }
        foreach (var p in particles)
        {
            p.Play();
        }
    }

    [Server]
    public void CancelInteraction()
    {
        EffectsManager.Instance.RpcHideInteraction();
        if (activeCollectable)
        {
            activeCollectable.StopCollecting();
            activeCollectable = null;
            SoundManager.Instance.RpcStopSoundPlayer(SoundEffect.CollectableCollecting, playerId);
        }

        if (activeType != TowerType.None)
        {
            NetworkServer.Destroy(activeBuildTower);
            activeBuildTower = null;
            activeType = TowerType.None;
            SoundManager.Instance.RpcStopSoundPlayer(SoundEffect.TowerBuilding, playerId);
            statistics.towersBuilt -= 1;
        }

        if (activeDestroyingTower != null)
        {
            EffectsManager.Instance.RpcStopTowerDestroyEffect(activeDestroyingTower);
            activeDestroyingTower = null;
            SoundManager.Instance.RpcStopSoundPlayer(SoundEffect.TowerDestroying, playerId);
        }
    }

    [Server]
    public void UsePowerupFreeze()
    {
        if (!GameManager.Instance.isRunning) return;
        if (!ConsumeFromInventory(CollectableType.PowerupFreeze, 1))
            return;

        foreach (Ball ball in GameManager.Instance.balls)
        {
            ball.Freeze(Constants.freezeBallDuration);

        }
        SoundManager.Instance.RpcPlaySoundAll(SoundEffect.BallFreeze);
        this.Delayed(Constants.freezeBallDuration, () =>
        {
            SoundManager.Instance.RpcPlaySoundAll(SoundEffect.BallUnfreeze);
        });
        statistics.powerupsUsed += 1;
    }

    [Server]
    public void UsePowerupJamTowers()
    {
        if (!GameManager.Instance.isRunning) return;
        if (!ConsumeFromInventory(CollectableType.PowerupJamTowers, 1))
            return;

        foreach (var towerGo in GameObject.FindGameObjectsWithTag(Constants.TOWER_TAG))
        {
            var tower = towerGo.GetComponent<TowerBase>();
            if (tower.owner != this || GameManager.Instance.allowCheats) // jam all towers with cheats for testing vs bot
            {
                tower.JamForDuration(Constants.towerJamDuration);
                EffectsManager.Instance.RpcPlayInterferenceEffect(towerGo, Constants.towerJamDuration);
            }
        }
        SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.TowerJamming, playerId);
        statistics.powerupsUsed += 1;
    }

    [Server]
    public void UsePowerupGrapple()
    {
        if (!GameManager.Instance.isRunning) return;
        if (controllerTransform == null)
        {
            Debug.LogWarning("Cannot use grapple: controllerTransform was not assigned!", this);
            return;
        }

        var balls = new List<Ball>(GameManager.Instance.balls);
        if (balls.Count == 0)
            return;
        if (balls.Any(ball => !ball.CanGrapple()))
        {
            Debug.Log("Cannot use grapple: a ball is already blocked");
            SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.Invalid, playerId);
            return;
        }

        if (!ConsumeFromInventory(CollectableType.PowerupGrapplingHook, 1))
            return;

        float ballDiameter = balls[0].transform.localScale.y;
        float spacing = 1.7f * ballDiameter;
        float maxX = spacing * (balls.Count - 1) / 2f;
        // Sort balls by their current x position local to the player, to avoid crossing them over in weird ways.
        balls.Sort((a, b) => controllerTransform.InverseTransformPoint(a.transform.position).x.CompareTo(
            controllerTransform.InverseTransformPoint(b.transform.position).x));
        for (int i = 0; i < balls.Count; i++)
        {
            Ball ball = balls[i];
            float x = Mathf.Lerp(-maxX, maxX, (float)i / (balls.Count - 1));
            Vector3 targetPos = new Vector3(x, 0, Constants.scaledGrappleTargetDistance);
            ball.Grapple(controllerTransform, targetPos, ownedRectangle);
        }
        statistics.powerupsUsed += 1;
        SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.GrapplingHook, playerId);
    }

    [ClientRpc]
    public void RpcPlayerGamePaused()
    {
        StatusUIMananger.LocalInstance?.GamesPaused();
    }

    [ClientRpc]
    public void RpcPlayerHidePanel()
    {
        StatusUIMananger.LocalInstance?.HidePanel(true);
    }
}

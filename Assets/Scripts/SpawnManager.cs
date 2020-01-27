using Mirror;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum CollectableType
{
    None,
    TowerResource,
    PowerupFreeze,
    PowerupJamTowers,
    PowerupGrapplingHook
}

/// <summary>
/// Server-side script to spawn collectables.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct CollectableConfig
    {
        public CollectableType type;
        public GameObject prefab;
        public int probabilityWeight;
    }

    public static SpawnManager Instance { get; private set; }
    
    public float spawnIntervalMin;
    public float spawnIntervalMax;
    public CollectableConfig[] collectables;
    
    private Player lastSpawnedPlayer = null;
    private Queue<CollectableConfig> scheduledSpawns1 = new Queue<CollectableConfig>();
    private Queue<CollectableConfig> scheduledSpawns2 = new Queue<CollectableConfig>();

    void Awake()
    {
        Instance = this;

        if ((collectables?.Length).GetValueOrDefault() == 0)
        {
            Debug.LogError("There are no collectables configured in SpawnManager!", this);
            enabled = false;
        }
    }

    public void OnGameStart()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while(GameManager.Instance.isRunning)
        {
            float delay = UnityEngine.Random.Range(spawnIntervalMin, spawnIntervalMax);
            yield return new WaitForSeconds(delay);
            AttemptSpawn();
        }
    }

    private void AttemptSpawn()
    {
        // Alternate spawn attempts on either player's side.
        if (lastSpawnedPlayer) lastSpawnedPlayer = GameManager.Instance.GetOpponentOf(lastSpawnedPlayer);
        else lastSpawnedPlayer = GameManager.Instance.player1;

        // Pick a random spawn point on the player's side that is not occupied.
        Transform[] spawns = lastSpawnedPlayer.ownedRectangle.resourceSpawnPoints;
        List<Vector3> freeSpawns = spawns.Select(spawn => spawn.position).Where(pos => !IsSpawnOccupied(pos)).ToList();
        if (freeSpawns.Count == 0)
        {
            // All spawn points occupied
            return;
        }
        Vector3 spawnPos = Util.PickRandomElement(freeSpawns) + 0.02f * Vector3.up;
        
        // Pick which collectable type to spawn.
        CollectableConfig config = PickSpawnedType(lastSpawnedPlayer);
        var newCollectable = Instantiate(config.prefab, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(newCollectable);
        SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.CollectableSpawn, lastSpawnedPlayer.playerId);
    }

    private bool IsSpawnOccupied(Vector3 pos)
    {
        Collider[] hits = Physics.OverlapSphere(pos, 0.1f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        return hits.Any(hit => hit.CompareTag(Constants.COLLECTABLE_TAG));
    }

    private CollectableConfig PickSpawnedType(Player forPlayer)
    {
        if (forPlayer == GameManager.Instance.player1)
            return PickSpawnedTypeFromQueue(scheduledSpawns1);
        else
            return PickSpawnedTypeFromQueue(scheduledSpawns2);
    }

    private CollectableConfig PickSpawnedTypeFromQueue(Queue<CollectableConfig> queue)
    {
        if(queue.Count == 0)
        {
            var shuffledSpawns = collectables
                .SelectMany(config => Enumerable.Repeat(config, config.probabilityWeight))
                .OrderBy(_ => UnityEngine.Random.Range(0f, 1f));
            foreach(var spawn in shuffledSpawns)
            {
                queue.Enqueue(spawn);
            }
        }
        return queue.Dequeue();
    }
}

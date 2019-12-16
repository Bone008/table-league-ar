using Mirror;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectableType
{
    None,
    TowerResource,
    PowerupFreeze
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
    }

    public static SpawnManager Instance { get; private set; }

    public float spawnProbability;
    public CollectableConfig[] collectables;

    private Player lastSpawnedSide = null;

    void Awake()
    {
        Instance = this;

        if ((collectables?.Length).GetValueOrDefault() == 0)
        {
            Debug.LogError("There are no collectables configured in SpawnManager!", this);
            enabled = false;
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.isRunning)
            return;

        float prob = Random.Range(0, 1.0f);
        if (prob < spawnProbability)
        {
            AttemptSpawn();
        }
    }

    private void AttemptSpawn()
    {
        // Pick which collectable type to spawn.
        CollectableConfig config = Util.PickRandomElement(collectables);
        GameObject prefab = config.prefab;

        // Alternate spawning on either player's side.
        if (lastSpawnedSide) lastSpawnedSide = GameManager.Instance.GetOpponentOf(lastSpawnedSide);
        else lastSpawnedSide = GameManager.Instance.player1;

        // Pick a random spawn point on the player's side that is not occupied.
        Transform[] spawns = lastSpawnedSide.ownedRectangle.resourceSpawnPoints;
        List<Vector3> freeSpawns = spawns.Select(spawn => spawn.position).Where(pos => !IsSpawnOccupied(pos)).ToList();
        if (freeSpawns.Count == 0)
        {
            // All spawn points occupied
            return;
        }
        Vector3 spawnPos = Util.PickRandomElement(freeSpawns) + 0.04f * Vector3.up;

        var newCollectable = Instantiate(prefab, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(newCollectable);
    }

    private bool IsSpawnOccupied(Vector3 pos)
    {
        Collider[] hits = Physics.OverlapSphere(pos, 0.01f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        return hits.Any(hit => hit.CompareTag(Constants.RESOURCE_TAG));
    }
}

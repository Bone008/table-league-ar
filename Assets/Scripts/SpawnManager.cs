using Mirror;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectableType
{
    None,
    TowerResource,
    // Add powerup types here.
}

/// <summary>
/// Server-side script to spawn collectables.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }
    
    public GameObject resourcePrefab;
    public float resourceCreationProbabilty;

    private int resourcesCreated = 0;
    private Player lastSpawnedSide = null;

    void Awake() { Instance = this; }

    void Update()
    {
        if (!GameManager.Instance.isRunning)
            return;

        float prob = Random.Range(0, 1.0f);
        if (prob < resourceCreationProbabilty)
        {
            AttemptSpawn();
        }
    }

    private void AttemptSpawn()
    {
        // Alternate spawning on either player's side.
        if (lastSpawnedSide) lastSpawnedSide = GameManager.Instance.GetOpponentOf(lastSpawnedSide);
        else lastSpawnedSide = GameManager.Instance.player1;

        // Pick a random spawn point on the player's side that is not occupied.
        Transform[] spawns = lastSpawnedSide.ownedRectangle.resourceSpawnPoints;
        List<Vector3> freeSpawns = spawns.Select(spawn => spawn.position).Where(pos => !IsSpawnOccupied(pos)).ToList();
        if(freeSpawns.Count == 0)
        {
            // All spawn points occupied
            return;
        }
        Vector3 spawnPos = freeSpawns[Random.Range(0, freeSpawns.Count)] + 0.04f * Vector3.up;

        var newResource = Instantiate(resourcePrefab, spawnPos, Quaternion.identity);
        NetworkServer.Spawn(newResource);
        resourcesCreated++;
    }

    private bool IsSpawnOccupied(Vector3 pos)
    {
        Collider[] hits = Physics.OverlapSphere(pos, 0.01f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);
        return hits.Any(hit => hit.CompareTag(Constants.RESOURCE_TAG));
    }

    public void NotifyResourceCollected()
    {
        resourcesCreated--;
    }
}

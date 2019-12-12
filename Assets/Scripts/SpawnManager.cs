using Mirror;
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

    public int resourceLimit;
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
        if (prob < resourceCreationProbabilty && resourcesCreated < resourceLimit)
        {
            // Alternate spawning on either player's side.
            if (lastSpawnedSide) lastSpawnedSide = GameManager.Instance.GetOpponentOf(lastSpawnedSide);
            else lastSpawnedSide = GameManager.Instance.player1;

            // Pick a random point within their rectangle.
            var rect = lastSpawnedSide.ownedRectangle;
            float spawnX = Random.Range(rect.min.x, rect.max.x);
            float spawnZ = Random.Range(rect.min.z, rect.max.z);
            Vector3 spawnPos = new Vector3(spawnX, 0.015f, spawnZ);

            var newResource = Instantiate(resourcePrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(newResource);
            resourcesCreated++;
        }
    }
    
    public void NotifyResourceCollected()
    {
        resourcesCreated--;
    }
}

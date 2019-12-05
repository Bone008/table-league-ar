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
    
    void Awake() { Instance = this; }

    void Update()
    {
        if (!GameManager.Instance.isRunning)
            return;

        float prob = Random.Range(0, 1.0f);
        if (prob < resourceCreationProbabilty && resourcesCreated < resourceLimit)
        {
            var newResource = Instantiate(resourcePrefab, new Vector3(Random.Range(-1.0f, 1.0f), 0.015f, Random.Range(-1.4f, 1.4f)), Quaternion.identity);
            NetworkServer.Spawn(newResource);
            resourcesCreated++;
        }
    }
    
    public void NotifyResourceCollected()
    {
        resourcesCreated--;
    }
}

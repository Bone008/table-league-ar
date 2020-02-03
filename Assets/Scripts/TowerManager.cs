using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TowerType
{
    None,
    Magnetic,
    MagneticPush,
    Barrier,
    Capture
}

public class TowerManager : MonoBehaviour
{
    public static TowerManager Instance { get; private set; }

    public GameObject[] previewTowers;
    public GameObject[] towers;
    public int[] towerCosts;
    public float buildTime;
    public float destroyTime;
    public float destroyEffectOnlyTime;

    void Awake()
    {
        Instance = this;
        if (previewTowers.Length != towers.Length || towers.Length != towerCosts.Length)
            Debug.LogError("Invalid tower configuration: TowerManager arrays should have same length.", this);
    }

    public GameObject getTowerPreview(TowerType type)
    {
        return previewTowers[(int)type];
    }

    public GameObject getTower(TowerType type)
    {
        return towers[(int)type];
    }

    public int getTowerCost(TowerType type)
    {
        return towerCosts[(int)type];
    }
}

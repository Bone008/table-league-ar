using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TowerType
{
    None,
    Magnetic,
    MagneticPush,
    Barrier
}

public class TowerManager : MonoBehaviour
{
    public static TowerManager Instance { get; private set; }

    public GameObject[] previewTowers;
    public GameObject[] towers;
    public float buildTime;

    void Awake() { Instance = this; }

    public GameObject getTowerPreview(TowerType type)
    {
        return previewTowers[(int)type];
    }

    public GameObject getTower(TowerType type)
    {
        return towers[(int)type];
    }
}

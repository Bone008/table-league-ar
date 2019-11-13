using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerManager : MonoBehaviour
{
    public static int towerChoice;

    public void MagneticTower()
    {
        towerChoice = 0;
    }

    public void MagneticPushTower()
    {
        towerChoice = 1;
        Debug.Log(towerChoice);
    }

    public void BarrierTower()
    {
        towerChoice = 2;
        Debug.Log(towerChoice);
    }

    public static int GetTowerChoice()
    {
        return towerChoice;
    }
         
}

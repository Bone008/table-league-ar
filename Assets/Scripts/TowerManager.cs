using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerManager : MonoBehaviour
{
    public static int towerChoice = - 1;
    public Button mTowerButton;
    public Button mPushButton;
    public Button bTowerButton;

    public void MagneticTower()
    {
        if(towerChoice  == 0)
        {
            towerChoice = -1;
            mTowerButton.image.color = Color.white;
        }
        else
        {
            towerChoice = 0;
            mTowerButton.image.color = Color.red;
        }
    }

    public void MagneticPushTower()
    {
        if (towerChoice == 1)
        {
            towerChoice = -1;
            mPushButton.image.color = Color.white;
        }
        else
        {
            towerChoice = 1;
            mPushButton.image.color = Color.red;
        }
    }

    public void BarrierTower()
    {
        if (towerChoice == 2)
        {
            towerChoice = -1;
            bTowerButton.image.color = Color.white;
        }
        else
        {
            towerChoice = 2;
            bTowerButton.image.color = Color.red;
        }
    }

    public static int GetTowerChoice()
    {
        return towerChoice;
    }
         
}

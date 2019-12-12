using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerUIManager : MonoBehaviour
{
    public static int towerChoice = 0;
    public Button mTowerButton;
    public Button mPushButton;
    public Button bTowerButton;

    public void MagneticTower()
    {
        if (towerChoice == 1)
        {
            towerChoice = 0;
            mTowerButton.image.color = Color.white;
        }
        else
        {
            towerChoice = 1;
            mTowerButton.image.color = Color.red;
            mPushButton.image.color = Color.white;
            bTowerButton.image.color = Color.white;
        }
    }

    public void MagneticPushTower()
    {
        if (towerChoice == 2)
        {
            towerChoice = 0;
            mPushButton.image.color = Color.white;
        }
        else
        {
            towerChoice = 2;
            mPushButton.image.color = Color.red;
            mTowerButton.image.color = Color.white;
            bTowerButton.image.color = Color.white;
        }
    }

    public void BarrierTower()
    {
        if (towerChoice == 3)
        {
            towerChoice = 0;
            bTowerButton.image.color = Color.white;
        }
        else
        {
            towerChoice = 3;
            bTowerButton.image.color = Color.red;
            mTowerButton.image.color = Color.white;
            mPushButton.image.color = Color.white;
        }
    }

    public static int GetTowerChoice()
    {
        return towerChoice;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerUIManager : MonoBehaviour
{
    public static bool destroyMode = false;
    public static TowerType towerChoice = TowerType.None;
    
    [Tooltip("the order should follow the TowerType enum, but first entry (None) represents the destroy button")]
    public Button[] towerButtons;
    
    public void DestroyTower()
    {
        destroyMode = !destroyMode;
        towerChoice = TowerType.None;
        UpdateButtons();
    }

    public void MagneticTower()
    {
        ToggleTowerChoice(TowerType.Magnetic);
    }

    public void MagneticPushTower()
    {
        ToggleTowerChoice(TowerType.MagneticPush);
    }

    public void BarrierTower()
    {
        ToggleTowerChoice(TowerType.Barrier);
    }

    private void ToggleTowerChoice(TowerType newType)
    {
        if (towerChoice == newType)
        {
            towerChoice = TowerType.None;
        }
        else
        {
            towerChoice = newType;
            destroyMode = false;
        }
        UpdateButtons();
    }

    private void UpdateButtons()
    {
        towerButtons[0].image.color = (destroyMode ? Color.red : Color.white);
        for (int i = 1; i < towerButtons.Length; i++)
        {
            towerButtons[i].image.color = (i == (int)towerChoice ? Color.red : Color.white);
        }
    }
    
}

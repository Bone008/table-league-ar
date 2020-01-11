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
    public Sprite selectedSpriteBuild;
    public Sprite selectedSpriteDestroy;

    private Sprite defaultSpriteBuild;
    private Sprite defaultSpriteDestroy;

    private bool hasRegisteredPlayer;

    void Start()
    {
        defaultSpriteBuild = towerButtons[1].image.sprite; // first tower button's default sprite
        defaultSpriteDestroy = towerButtons[0].image.sprite; // destroy tower button's default sprite
        UpdateButtons();
    }

    void Update()
    {
        // Listen to inventory changes as soon as we have a player.
        if(!hasRegisteredPlayer && PlayerNetController.LocalInstance?.player != null)
        {
            hasRegisteredPlayer = true;
            UpdateButtons();
            PlayerNetController.LocalInstance.player.InventoryChange += (_, __, ___) =>
            {
                //Debug.Log("towerui change: " + _ + ";" + __ + ";" + ___);
                // Due to a bug in SyncDictionary, during the callback the change may not be applied yet.
                Invoke(nameof(UpdateButtons), 0);
            };
        }
    }

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

    public void CaptureTower()
    {
        ToggleTowerChoice(TowerType.Capture);
    }

    public void DeselectAll()
    {
        towerChoice = TowerType.None;
        destroyMode = false;
        UpdateButtons();
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
        int resources = PlayerNetController.LocalInstance?.player?.GetInventoryCount(CollectableType.TowerResource) ?? 0;
        bool canBuild = resources >= Constants.towerCost;

        if (!canBuild) towerChoice = TowerType.None;

        towerButtons[0].image.sprite = (destroyMode ? selectedSpriteDestroy : defaultSpriteDestroy);
        for (int i = 1; i < towerButtons.Length; i++)
        {
            towerButtons[i].interactable = canBuild;
            towerButtons[i].image.sprite = (i == (int)towerChoice ? selectedSpriteBuild : defaultSpriteBuild);
        }
    }
    
}

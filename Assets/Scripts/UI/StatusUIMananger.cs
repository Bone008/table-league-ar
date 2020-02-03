using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusUIMananger : MonoBehaviour
{
    public static StatusUIMananger LocalInstance { get; private set; }

    public GameObject statusMessagePanel;
    public TMPro.TextMeshProUGUI statusMessageText;

    public bool destroyMode = false;
    private bool pauseActive = false;

    void Awake()
    {
        LocalInstance = this;
    }

    public void TowerDestroyToggle(bool mode)
    {
        if (mode && !pauseActive)
        {
            statusMessageText.text = "Hold a tower to destroy it.";
            statusMessagePanel.SetActive(true);
        }

        destroyMode = mode;
    }

    public void HoldToBuild()
    {
        if (pauseActive) return;
        statusMessageText.text = "Hold on the floor to build tower.";
        statusMessagePanel.SetActive(true);
    }

    public void TowerTooClose()
    {
        if (pauseActive) return;
        statusMessageText.text = "Too close to other towers!";
        statusMessagePanel.SetActive(true);
    }

    public void BuildOwnSide()
    {
        if (pauseActive) return;
        statusMessageText.text = "Can only build on your own side!";
        statusMessagePanel.SetActive(true);
    }

    public void GamesPaused()
    {
        statusMessageText.text = "Game is paused.";
        statusMessagePanel.SetActive(true);
        pauseActive = true;
    }

    public void HidePanel(bool unpause = false)
    {
        if (!destroyMode && !pauseActive)
            statusMessagePanel.SetActive(false);

        if (unpause)
        {
            statusMessagePanel.SetActive(false);
            pauseActive = false;
        }
    }


}

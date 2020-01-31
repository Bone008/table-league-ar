using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusUIMananger : MonoBehaviour
{
    public static StatusUIMananger LocalInstance { get; private set; }

    public GameObject statusMessagePanel;
    public TMPro.TextMeshProUGUI statusMessageText;

    public bool destroyMode = false;

    void Awake()
    {
        if (LocalInstance == null)
        {
            LocalInstance = this;
        }
    }

    public void TowerDestroyToggle(bool mode)
    {
        if (mode)
        {
            statusMessageText.text = "Hold a tower to destroy it";
            statusMessagePanel.SetActive(true);
        }

        destroyMode = mode;
    }

    public void HoldToBuild()
    {
        statusMessageText.text = "Hold on the floor to build tower";
        statusMessagePanel.SetActive(true);
    }

    public void TowerTooClose()
    {
        statusMessageText.text = "Too close to other towers";
        statusMessagePanel.SetActive(true);
    }

    public void BuildOwnSide()
    {
        statusMessageText.text = "Build on your own side";
        statusMessagePanel.SetActive(true);
    }

    public void HidePanel()
    {
        if (!destroyMode)
            statusMessagePanel.SetActive(false);
    }


}

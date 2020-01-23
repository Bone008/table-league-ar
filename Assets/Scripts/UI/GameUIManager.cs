using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Mirror.SyncIDictionary<CollectableType, int>;

/// <summary>Singleton responsible for showing information about the game on the UI.</summary>
public class GameUIManager : MonoBehaviour
{
    public Player player1;
    public Player player2;
    public Player localPlayer => PlayerNetController.LocalInstance?.player;

    public Renderer floorRenderer;
    public Material floorEnabledMaterial;
    public Material floorDisabledMaterial;

    public TimeController timeController;
    public GameObject getReadyPanel;
    public Text gameTimeText;
    public Text score1Text;
    public Text score2Text;
    public TMPro.TextMeshProUGUI resourceText;
    public Button freezeButton;
    public TMPro.TextMeshProUGUI freezeAmountText;
    public Button jamButton;
    public TMPro.TextMeshProUGUI jamAmountText;
    public Button grappleButton;
    public TMPro.TextMeshProUGUI grappleAmountText;

    public AnimationCurve valueUpdatePopCurve;
    public float valueUpdatePopSize;
    public GameObject[] serverOnlyObjects = new GameObject[0];

    private bool hasRegisteredInventoryCallback = false;

#if UNITY_EDITOR
    void Awake()
    {
        if (!NetworkManager.singleton && SceneManager.GetActiveScene().name == "GameScene")
        {
            Debug.LogWarning("[Workaround] Pulling in the NetworkManager from the Menu scene and starting in Singleplayer mode. "
                + "There may be side effects to this, it is recommended to start the game from the Menu scene!");

            // After loading the menu, its NetworkManager will be initialized and put into DontDestroyOnUnload,
            // so it will persist after unloading it again. The game will use the default values of ServerSettings.
            // This may have some side effects for initialization logic ...
            SceneManager.LoadScene("Menu", LoadSceneMode.Additive);
            this.Delayed(0, () =>
            {
                SceneManager.UnloadSceneAsync("Menu");
                NetworkManager.singleton.StartHost();
            });
        }
    }
#endif

    void Start()
    {
        if (!NetworkServer.active)
        {
            foreach (var go in serverOnlyObjects)
                go.SetActive(false);
        }
    }
    void Update()
    {
        gameTimeText.text = timeController.GetFormattedTimeRemaining();
        if (localPlayer != null)
        {
            score1Text.text = localPlayer.score.ToString();
            score2Text.text = (localPlayer == player1 ? player2 : player1).score.ToString();

            if (!hasRegisteredInventoryCallback)
            {
                localPlayer.InventoryChange += LocalPlayer_InventoryChange;
                hasRegisteredInventoryCallback = true;
                // Call once to show initial inventory state.
                LocalPlayer_InventoryChange(Operation.OP_ADD, CollectableType.None, 0);
            }
        }
    }

    private void LocalPlayer_InventoryChange(Operation op, CollectableType key, int newAmount)
    {
        this.Delayed(0, () => // Need to delay to make sure the changes have been applied.
        {
            resourceText.text = localPlayer.GetInventoryCount(CollectableType.TowerResource).ToString();
            UpdatePowerupButton(CollectableType.PowerupFreeze, freezeButton, freezeAmountText);
            UpdatePowerupButton(CollectableType.PowerupJamTowers, jamButton, jamAmountText);
            UpdatePowerupButton(CollectableType.PowerupGrapplingHook, grappleButton, grappleAmountText);
        });

        if (op == Operation.OP_SET)
        {
            int oldAmount = localPlayer.GetInventoryCount(key);
            Debug.Log(key + " from " + oldAmount + " to " + newAmount);
            Transform target;
            switch(key)
            {
                case CollectableType.PowerupFreeze: target = freezeAmountText.transform; break;
                case CollectableType.PowerupJamTowers: target = jamAmountText.transform; break;
                case CollectableType.TowerResource: target = resourceText.transform; break;
                default: return;
            }
            this.AnimateVector(0.3f, Vector3.one, valueUpdatePopSize * Vector3.one, valueUpdatePopCurve, v => target.localScale = v);
        }
    }

    private void UpdatePowerupButton(CollectableType type, Button button, TMPro.TextMeshProUGUI amountText)
    {
        int amount = localPlayer.GetInventoryCount(type);
        amountText.text = amount.ToString();
        button.interactable = amount > 0;
    }

    public void SetReady()
    {
        if (PlayerNetController.LocalInstance)
        {
            PlayerNetController.LocalInstance.CmdSetReady(true);
            getReadyPanel.SetActive(false);
        }
        else
            Debug.LogWarning("Cannot send ready command without a network player!");
    }

    public void ExitGame()
    {
        if (NetworkServer.active)
        {
            NetworkManager.singleton.StopHost();
        }
        else if (NetworkClient.active)
        {
            NetworkManager.singleton.StopClient();
        }
        else
        {
            Debug.LogWarning("Neither server nor client active while trying to exit game!");
            SceneManager.LoadSceneAsync(NetworkManager.singleton.offlineScene);
        }
    }

    public void ToggleFloorVisibility()
    {
        Material newMat = (floorRenderer.sharedMaterial == floorEnabledMaterial ? floorDisabledMaterial : floorEnabledMaterial);
        floorRenderer.sharedMaterial = newMat;
    }

    public void UsePowerupFreeze()
    {
        PlayerNetController.LocalInstance?.CmdUsePowerupFreeze();
    }

    public void UsePowerupJamTowers()
    {
        PlayerNetController.LocalInstance?.CmdUsePowerupJamTowers();
    }

    public void UsePowerupGrapple()
    {
        PlayerNetController.LocalInstance?.CmdUsePowerupGrapple();
    }
}

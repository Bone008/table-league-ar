using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public Text resourceText;
    public Button freezeButton;
    public TMPro.TextMeshProUGUI freezeAmountText;
    public Button jamButton;
    public TMPro.TextMeshProUGUI jamAmountText;

    public GameObject[] serverOnlyObjects = new GameObject[0];


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
            resourceText.text = localPlayer.GetInventoryCount(CollectableType.TowerResource).ToString();

            int freezeNum = localPlayer.GetInventoryCount(CollectableType.PowerupFreeze);
            freezeAmountText.text = freezeNum.ToString();
            freezeButton.interactable = freezeNum > 0;
            int jamNum = localPlayer.GetInventoryCount(CollectableType.PowerupJamTowers);
            jamAmountText.text = jamNum.ToString();
            jamButton.interactable = jamNum > 0;
        }
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
            NetworkManager.singleton.ServerChangeScene("EndGame");
        }
        else if (NetworkClient.active)
        {
            NetworkManager.singleton.StopClient();
            NetworkManager.singleton.ServerChangeScene("EndGame");
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
}

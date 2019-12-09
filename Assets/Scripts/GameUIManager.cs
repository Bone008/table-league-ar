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

    public Text score1Text;
    public Text score2Text;
    public Text resourceText;

    public GameObject[] serverOnlyObjects = new GameObject[0];


#if UNITY_EDITOR
    void Awake()
    {
        if (!NetworkManager.singleton)
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
        score1Text.text = player1.score.ToString();
        score2Text.text = player2.score.ToString();
        if (localPlayer != null)
        {
            resourceText.text = localPlayer.resources.ToString();
        }
    }

    public void ExitGame()
    {
        NetworkManager.singleton.StopHost();
    }
}

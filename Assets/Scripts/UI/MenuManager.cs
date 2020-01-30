using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private const string PREFS_KEY_ADDRESS = "client.remote_address";
    private static string remoteAddress = "";

    public GameObject playMenu;
    public GameObject multiPlayer;
    public Button defaultBalls;
    public Button defaultDuration;
    public TMPro.TextMeshProUGUI statusText;
    public TMPro.TMP_InputField remoteAddressInput;
    public Toggle enableCheatsToggle;
    public AudioSource altClickyAudio;

    private bool attemptingConnect = false;

    void Start()
    {
        remoteAddress = PlayerPrefs.GetString(PREFS_KEY_ADDRESS, "localhost");
        remoteAddressInput.text = remoteAddress;
        remoteAddressInput.onValueChanged.AddListener(value =>
        {
            remoteAddress = value;
            PlayerPrefs.SetString(PREFS_KEY_ADDRESS, value);
            PlayerPrefs.Save();
        });

        enableCheatsToggle.isOn = ServerSettings.allowCheats;
        enableCheatsToggle.onValueChanged.AddListener(value =>
        {
            ServerSettings.allowCheats = value;
        });

        // Highlights the default buttons and calls SetBalls and SetDuration,
        // to reset ServerSettings properly after quitting a previous game.
        altClickyAudio.mute = true;
        this.Delayed(0.4f, () => altClickyAudio.mute = false);
        defaultBalls.onClick.Invoke();
        defaultDuration.onClick.Invoke();
    }

    void Update()
    {
        if (attemptingConnect && !NetworkClient.active)
        {
            statusText.text = "Connection failed! Please try again.";
            attemptingConnect = false;
        }
        else if (NetworkClient.isConnected && !ClientScene.ready)
        {
            statusText.text = "Connected! Loading scene ...";
        }
    }

    public void ConfigBack()
    {
        if (!ServerSettings.isMultiplayer)
        {
            playMenu.SetActive(true);
        }
        else
        {
            multiPlayer.SetActive(true);
        }
    }

    public void SetSingleplayer()
    {
        ServerSettings.isMultiplayer = false;
    }

    public void SetMultiplayer()
    {
        ServerSettings.isMultiplayer = true;
    }

    public void SetBalls(int balls)
    {
        ServerSettings.numberOfBalls = balls;
    }

    public void SetDuration(float durationMinutes)
    {
        ServerSettings.gameDurationSeconds = durationMinutes * 60;
    }

    public void StartGame()
    {
        Debug.Log(string.Format("Starting game with settings: multiplayer={0}, #balls={1}, duration={2:0.#}",
            ServerSettings.isMultiplayer, ServerSettings.numberOfBalls, ServerSettings.gameDurationSeconds / 60));

        // For singleplayer, we still host a pseudo "server", but we don't open any ports.
        NetworkServer.dontListen = !ServerSettings.isMultiplayer;
        NetworkManager.singleton.networkAddress = "localhost";
        NetworkManager.singleton.StartHost();
        // NetworkManager will switch to its "onlineScene" automatically.
    }

    public void JoinGame()
    {
        statusText.text = "Trying to connect ...";
        attemptingConnect = true;
        NetworkManager.singleton.networkAddress = remoteAddress;
        NetworkManager.singleton.StartClient();
    }

    public void CancelLoading()
    {
        attemptingConnect = false;
        NetworkManager.singleton.StopClient();
    }

}

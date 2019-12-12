using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    private static string remoteAddress = "localhost";

    public GameObject playMenu;
    public GameObject multiPlayer;
    public Button defaultBalls;
    public Button defaultPoints;
    public TMPro.TextMeshProUGUI statusText;

    private bool attemptingConnect = false;
    
    void Start()
    {
        defaultBalls.interactable = false;
        defaultPoints.interactable = false;
    }

    void Update()
    {
        if(attemptingConnect && !NetworkClient.active)
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

    public void SetPoints(int points)
    {
        ServerSettings.winningPoints = points;
    }

    public void SetRemoteAddress(string value)
    {
        remoteAddress = value;
    }

    public void StartGame()
    {
        Debug.Log(string.Format("Starting game with settings: multiplayer={0}, #balls={1}, #points={2}",
            ServerSettings.isMultiplayer, ServerSettings.numberOfBalls, ServerSettings.winningPoints));

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

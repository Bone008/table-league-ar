using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject playMenu;
    public GameObject multiPlayer;
    public Button defaultBalls;
    public Button defaultPoints;
    
    void Start()
    {
        defaultBalls.interactable = false;
        defaultPoints.interactable = false;
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

    public void StartGame()
    {
        Debug.Log(string.Format("Starting game with settings: multiplayer={0}, #balls={1}, #points={2}",
            ServerSettings.isMultiplayer, ServerSettings.numberOfBalls, ServerSettings.winningPoints));

        // For singleplayer, we still host a pseudo "server", but we don't open any ports.
        NetworkServer.dontListen = !ServerSettings.isMultiplayer;
        NetworkManager.singleton.StartHost();
        // NetworkManager will switch to its "onlineScene" automatically.
    }

    public void JoinGame()
    {
        NetworkManager.singleton.StartClient();
    }
    
}

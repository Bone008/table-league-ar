using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeController : NetworkBehaviour
{
    [Scene]
    public string postGameScene;

    [SyncVar]
    private double gameStartTime = double.NaN;
    [SyncVar]
    private float gameDuration = 0;
    [SyncVar]
    private double gamePausedTime = double.NaN;

    public bool IsPaused => !double.IsNaN(gamePausedTime);
    
    [Server]
    public void OnGameStart()
    {
        gameStartTime = NetworkTime.time;
        gameDuration = ServerSettings.gameDurationSeconds;
    }

    [Server]
    public void OnGamePause(bool newPaused)
    {
        if (newPaused && !IsPaused)
        {
            gamePausedTime = NetworkTime.time;
        }
        else if (!newPaused && IsPaused)
        {
            double pauseDuration = NetworkTime.time - gamePausedTime;
            // Shift game start timestamp by the pause duration to keep counting from here.
            gameStartTime += pauseDuration;
            gamePausedTime = double.NaN;
        }
    }

    /// <summary>Returns the time in seconds that the game has been running for.</summary>
    public float GetTimeElapsed()
    {
        if (double.IsNaN(gameStartTime)) return 0;
        double now = IsPaused ? gamePausedTime : NetworkTime.time;
        double elapsed = now - gameStartTime;
        return (float)elapsed;
    }

    /// <summary>Returns the remaining game time in seconds.</summary>
    public float GetTimeRemaining()
    {
        return gameDuration - GetTimeElapsed();
    }

    /// <summary>Returns the remaining game time formatted as M:SS.</summary>
    public string GetFormattedTimeRemaining()
    {
        var ts = TimeSpan.FromSeconds(Mathf.Max(0, GetTimeRemaining()));
        return string.Format("{0}:{1:00}", (int)ts.TotalMinutes, ts.Seconds);
    }


    [ClientRpc]
    public void RpcFinishGame(GameStatistics stats1, GameStatistics stats2)
    {
        // Find our statistic and remember them.
        GameStatistics localStats;
        if (stats1.playerId == PlayerNetController.LocalInstance?.playerId)
            localStats = stats1;
        else if (stats2.playerId == PlayerNetController.LocalInstance?.playerId)
            localStats = stats2;
        else
        {
            // This is probably a way to detect spectator mode later.
            Debug.LogWarning("Could not locate own stats.", this);
            localStats = new GameStatistics();
        }

        PostGameManager.statistics = localStats;

        // Change to postGameScene.
        SceneManager.LoadSceneAsync(postGameScene);

        // Disconnect from server / Stop server.
        NetworkManager.singleton.Delayed(2, () =>
        {
            string initialOfflineScene = NetworkManager.singleton.offlineScene;
            NetworkManager.singleton.offlineScene = null;

            if (NetworkServer.active)
                NetworkManager.singleton.StopHost();
            else if (NetworkClient.active)
                NetworkManager.singleton.StopClient();
            else
                Debug.LogWarning("Neither server nor client active while trying to finish game!");

            NetworkManager.singleton.offlineScene = initialOfflineScene;
        });
    }

}

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
    [SyncVar]
    private int ticksRemaining = Constants.totalTicks;

    public bool IsPaused => !double.IsNaN(gamePausedTime);

    private bool timeWarningPlayed = false;
    
    [Server]
    public void OnGameStart()
    {
        gameStartTime = NetworkTime.time;
        gameDuration = ServerSettings.gameDurationSeconds;
    }

    [ServerCallback]
    void Update()
    {
        if (!Double.IsNaN(gameStartTime))
        {
            float time = GetTimeRemaining();
            
            if(Mathf.Ceil(time) <= ticksRemaining && ticksRemaining > 0)
            {
                Debug.Log("Time: " + time);
                SoundManager.Instance.RpcPlaySoundAll(SoundEffect.ClockTick);
                ticksRemaining--;
            }

            if (!timeWarningPlayed && Mathf.Ceil(time) <= Constants.timeWarning)
            {
                SoundManager.Instance.RpcPlaySoundAll(SoundEffect.TimeWarning);
                timeWarningPlayed = true;
            }
        }
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
        GameStatistics opStats;
        if (stats1.playerId == PlayerNetController.LocalInstance?.playerId)
        {
            localStats = stats1;
            opStats = stats2;
        }
        else if (stats2.playerId == PlayerNetController.LocalInstance?.playerId)
        {
            localStats = stats2;
            opStats = stats1;
        }
        else
        {
            // Spectator.
            Debug.Log("Could not locate own stats. Pretending to be player 1.", this);
            localStats = stats1;
            opStats = stats2;
        }

        PostGameManager.statistics = localStats;
        PostGameManager.opStatistics = opStats;

        // Change to postGameScene.
        SceneManager.LoadSceneAsync(postGameScene);

        // Disconnect from server / Stop server.
        float delay = NetworkServer.active ? 8 : 2;
        NetworkManager.singleton.Delayed(new WaitForSecondsRealtime(delay), () =>
        {
            Debug.Log("Disconnecting now.");
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

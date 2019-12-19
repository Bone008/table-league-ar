using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : NetworkBehaviour
{
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
}

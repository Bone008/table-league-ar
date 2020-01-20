using System;
using System.Collections;
using System.Collections.Generic;

public enum WinResult
{
    Winner,
    Loser,
    Draw
}
/// <summary>Gathered statistics about a player for one game.</summary>
public class GameStatistics
{
    /// <summary>Id of the player who this statistic is for.</summary>
    public int playerId;
    public WinResult result;
    public int finalScoreSelf;
    public int finalScoreOpponent;
    public int numberOfBallHits;
}

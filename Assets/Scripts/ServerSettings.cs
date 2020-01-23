using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Static class that holds mutable game settings for the server that can be set before entering GameScene.</summary>
public static class ServerSettings
{
    /// <summary>If false, the game will be started with a Bot opponent after the first player connects.</summary>
    public static bool isMultiplayer = false;
    public static int numberOfBalls = 4;
    public static float gameDurationSeconds = 300f;
}

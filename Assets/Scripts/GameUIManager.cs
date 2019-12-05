﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    
    void Update()
    {
        score1Text.text = player1.score.ToString();
        score2Text.text = player2.score.ToString();
        if (localPlayer != null)
        {
            resourceText.text = localPlayer.resources.ToString();
        }
    }

    public void UpdateScores(int score1, int score2)
    {
        score1Text.text = score1.ToString();
        score2Text.text = score2.ToString();
    }

    public void UpdateResource(int resource)
    {
        resourceText.text = resource.ToString();
    }
}

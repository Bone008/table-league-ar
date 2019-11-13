using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Singleton that keeps track of balls, players, scores and winning.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameUIManager ui;
    public Player player1;
    public Player player2;

    private int score1 = 0;
    private int score2 = 0;

    void Awake()
    {
        Instance = this;
    }

    public void ScoreGoal(Ball ball, Player defendingPlayer)
    {
        if (defendingPlayer == player1)
        {
            score2++;
            Debug.Log("Player " + player2.playerName + " scored!\nScores: " + score1 + " : " + score2);
        }
        else if (defendingPlayer == player2)
        {
            score1++;
            Debug.Log("Player " + player1.playerName + " scored!\nScores: " + score1 + " : " + score2);
        }
        else
            throw new ArgumentException("unknown player!");
        
        ui.UpdateScores(score1, score2);
        ResetBall(ball, defendingPlayer);
        // TODO Winning condition.
    }

    /// <summary>Spawns a ball back in the home area of the given player.</summary>
    private void ResetBall(Ball ball, Player forPlayer)
    {
        // TODO reset in home area instead of center
        ball.Reset(new Vector3(0, 0.4f, 0));
    }
}

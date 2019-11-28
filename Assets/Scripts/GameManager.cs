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

    private int resourceCollected = 0;

    void Awake() { Instance = this; }

    void Start()
    {
        ResetAllBalls();
    }

    /// <summary>Distributes all balls across both players' home areas.</summary>
    public void ResetAllBalls()
    {
        Player p = player1;
        foreach (GameObject ballObject in GameObject.FindGameObjectsWithTag(Constants.BALL_TAG))
        {
            ResetBall(ballObject.GetComponent<Ball>(), p);
            p = GetOpponentOf(p);
        }
    }

    public void ScoreGoal(Ball ball, Player defendingPlayer)
    {
        Player attacker = GetOpponentOf(defendingPlayer);
        attacker.score++;

        ui.UpdateScores(player1.score, player2.score);
        ResetBall(ball, defendingPlayer);

        // TODO Check winning condition.
    }

    /// <summary>Spawns a ball back in the home area of the given player.</summary>
    private void ResetBall(Ball ball, Player forPlayer)
    {
        ball.Reset(forPlayer.homeAreaAnchor.position);
    }

    public void CollectResource()
    {
        resourceCollected += 10;
        ui.UpdateResource(resourceCollected);
    }

    public void UseResource(int towerCost)
    {
        resourceCollected -= towerCost;
        ui.UpdateResource(resourceCollected);
    }

    public int GetResource()
    {
        return resourceCollected;
    }


    public Player GetOpponentOf(Player player)
    {
        if (player == player1) return player2;
        else if (player == player2) return player1;
        else throw new ArgumentException("Unknown player!");
    }
}

using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Singleton that keeps track of balls, players, scores and winning.</summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject ballPrefab;
    public GameUIManager ui;
    public Player player1;
    public Player player2;

    private bool assignedPlayer1 = false;
    private bool assignedPlayer2 = false;
    private bool hasStarted = false;
    public bool isReadyToStart => !hasStarted && assignedPlayer1 && assignedPlayer2;

    // TODO: set value from menu config
    private readonly int numBallsToSpawn = 2;
    private List<Ball> balls = new List<Ball>();


    // TODO put into Player script
    private int resourceCollected = 100;

    void Awake() { Instance = this; }

    public Player AssignPlayer()
    {
        if (!assignedPlayer1)
        {
            assignedPlayer1 = true;
            return player1;
        }
        if (!assignedPlayer2)
        {
            assignedPlayer2 = true;
            return player2;
        }
        return null;
    }

    public void UnassignPlayer(Player player)
    {
        if (player == player1) assignedPlayer1 = false;
        else if (player == player2) assignedPlayer2 = false;
        else throw new ArgumentException("Cannot unassign unknown player!");
    }

    public void StartGame()
    {
        if(hasStarted)
        {
            Debug.LogWarning("Tried to start game multiple times.");
            return;
        }

        for(int i=0; i<numBallsToSpawn; i++)
        {
            var ballObject = Instantiate(ballPrefab);
            NetworkServer.Spawn(ballObject);
            balls.Add(ballObject.GetComponent<Ball>());
        }
        ResetAllBalls();
    }

    /// <summary>Distributes all balls across both players' home areas.</summary>
    public void ResetAllBalls()
    {
        Player p = player1;
        foreach (var ball in balls)
        {
            ResetBall(ball, p);
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

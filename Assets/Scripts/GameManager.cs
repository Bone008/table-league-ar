using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Server-only singleton that keeps track of balls, players, scores and winning.</summary>
public class GameManager : MonoBehaviour
{
    /// <summary>WARNING: Can NOT be accessed by client!</summary>
    public static GameManager Instance { get; private set; }

    public GameObject ballPrefab;
    public GameObject botPlayerPrefab;
    public Player player1;
    public Player player2;

    private bool assignedPlayer1 = false;
    private bool assignedPlayer2 = false;
    private bool hasStarted = false;
    private bool isPaused = false;
    /// <summary>True if the game has started and is NOT paused.</summary>
    public bool isRunning => hasStarted && !isPaused;

    private List<Ball> balls = new List<Ball>();
    private GameObject botPlayer = null;

    void Awake() { Instance = this; }

    void Update()
    {
        if (!hasStarted && player1.isUserReady && player2.isUserReady)
            StartGame();
        else if (isPaused && player1.isUserReady && player2.isUserReady)
        {
            Time.timeScale = 1;
            isPaused = false;
        }
        else if (isRunning && (!player1.isUserReady || !player2.isUserReady))
        {
            Time.timeScale = 0;
            isPaused = true;
        }
    }

    public Player AssignPlayer(Vector3 clientPosition)
    {
        float distTo1 = (clientPosition - player1.ownedRectangle.center).sqrMagnitude;
        float distTo2 = (clientPosition - player2.ownedRectangle.center).sqrMagnitude;
        // Assign player 1 by default, unless player 2 is is also free and we have the choice.
        bool preferPlayer2 = !assignedPlayer2 && distTo2 < distTo1;
        
        if (!assignedPlayer1 && !preferPlayer2)
        {
            assignedPlayer1 = true;
            player1.resources = 123;

            if (!ServerSettings.isMultiplayer && !assignedPlayer2) SpawnAndAssignBot();
            return player1;
        }
        if (!assignedPlayer2)
        {
            assignedPlayer2 = true;
            player2.resources = 321;

            if (!ServerSettings.isMultiplayer && !assignedPlayer1) SpawnAndAssignBot();
            return player2;
        }
        return null;
    }

    public void UnassignPlayer(Player player)
    {
        player.isUserReady = false;
        if (player == player1) assignedPlayer1 = false;
        else if (player == player2) assignedPlayer2 = false;
        else throw new ArgumentException("Cannot unassign unknown player!");
    }

    private void SpawnAndAssignBot()
    {
        Player player = AssignPlayer(Vector3.zero);
        if (player == null)
            throw new InvalidOperationException("No free player to assign bot!");

        botPlayer = Instantiate(botPlayerPrefab, player.transform);
        botPlayer.GetComponent<BotInputController>().player = player;
        NetworkServer.Spawn(botPlayer);
    }

    public void StartGame()
    {
        if (hasStarted)
        {
            Debug.LogWarning("Tried to start game multiple times.");
            return;
        }
        hasStarted = true;
        Debug.Log("Starting game!");

        for (int i = 0; i < ServerSettings.numberOfBalls; i++)
        {
            var ballObject = Instantiate(ballPrefab);
            NetworkServer.Spawn(ballObject);
            balls.Add(ballObject.GetComponent<Ball>());
        }
        ResetAllBalls();
    }

    // Note: Probably redundant as the scene is destroyed anyway when the game stops.
    public void StopGame()
    {
        if (!hasStarted) return;
        hasStarted = false;
        Debug.Log("Stopping game!");

        assignedPlayer1 = false;
        assignedPlayer2 = false;
        if (botPlayer)
        {
            NetworkServer.Destroy(botPlayer);
            botPlayer = null;
        }
        balls.Clear();
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
        ResetBall(ball, defendingPlayer);

        if (attacker.score >= ServerSettings.winningPoints)
        {
            Debug.Log("[SERVER] !!!!!!!!!!");
            Debug.Log("[SERVER] " + attacker.playerName + " has won! In the future, this will redirect to the winning screen ...");
            Debug.Log("[SERVER] !!!!!!!!!!");
        }
    }

    /// <summary>Spawns a ball back in the home area of the given player.</summary>
    private void ResetBall(Ball ball, Player forPlayer)
    {
        ball.Reset(forPlayer.homeAreaAnchor.position);
    }

    public Player GetOpponentOf(Player player)
    {
        if (player == player1) return player2;
        else if (player == player2) return player1;
        else throw new ArgumentException("Unknown player!");
    }
}

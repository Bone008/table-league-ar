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

    [Scene]
    public string postGameScene;
    public TimeController timeController;
    public GameObject ballPrefab;
    public GameObject botPlayerPrefab;
    public Player player1;
    public Player player2;
    public bool _allowCheats;
    public bool allowCheats => _allowCheats || ServerSettings.allowCheats;

    private bool assignedPlayer1 = false;
    private bool assignedPlayer2 = false;
    private bool hasStarted = false;
    private bool isPaused = false;

    public List<Ball> balls { get; } = new List<Ball>();
    private GameObject botPlayer = null;

    /// <summary>True if the game has started and is NOT paused.</summary>
    public bool isRunning => hasStarted && !isPaused;

    void Awake() { Instance = this; }

    void Update()
    {
        if (!hasStarted && player1.isUserReady && player2.isUserReady)
            StartGame();
        else if (isPaused && player1.isUserReady && player2.isUserReady)
        {
            Time.timeScale = 1;
            isPaused = false;
            timeController.OnGamePause(false);
            player1.PlayerHidePanel();
            player2.PlayerHidePanel();
        }
        else if (isRunning && (!player1.isUserReady || !player2.isUserReady))
        {
            Time.timeScale = 0;
            isPaused = true;
            timeController.OnGamePause(true);
            player1.PlayerGamePaused();
            player2.PlayerGamePaused();
        }

        if(isRunning && timeController.GetTimeRemaining() <= 0 && !allowCheats)
        {
            FinishGame();
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
            if (allowCheats)
            {
                player1.AddToInventory(CollectableType.TowerResource, 101);
                player1.AddToInventory(CollectableType.PowerupFreeze, 3);
                player1.AddToInventory(CollectableType.PowerupJamTowers, 3);
                player1.AddToInventory(CollectableType.PowerupGrapplingHook, 30);
            }

            if (!ServerSettings.isMultiplayer && !assignedPlayer2) SpawnAndAssignBot();
            return player1;
        }
        if (!assignedPlayer2)
        {
            assignedPlayer2 = true;
            if (allowCheats)
            {
                player2.AddToInventory(CollectableType.TowerResource, 102);
                player2.AddToInventory(CollectableType.PowerupFreeze, 3);
                player2.AddToInventory(CollectableType.PowerupJamTowers, 3);
                player2.AddToInventory(CollectableType.PowerupGrapplingHook, 30);
            }

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
        player.controllerTransform = botPlayer.transform;
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

        SpawnManager.Instance.OnGameStart();
        timeController.OnGameStart();
        SoundManager.Instance.RpcPlaySoundAll(SoundEffect.GameStart);
    }

    private void FinishGame()
    {
        Debug.Log("FINISHING GAME!");
        hasStarted = false;
        UnassignPlayer(player1);
        UnassignPlayer(player2);

        foreach (Ball ball in balls)
            ball.gameObject.SetActive(false);

        // Gather statistics.
        player1.statistics.finalScoreSelf = player1.score;
        player1.statistics.finalScoreOpponent = player2.score;
        player2.statistics.finalScoreSelf = player2.score;
        player2.statistics.finalScoreOpponent = player1.score;
        if (player1.score > player2.score)
        {
            player1.statistics.result = WinResult.Winner;
            player2.statistics.result = WinResult.Loser;
        }
        else if (player1.score < player2.score)
        {
            player1.statistics.result = WinResult.Loser;
            player2.statistics.result = WinResult.Winner;
        }
        else
        {
            player1.statistics.result = WinResult.Draw;
            player2.statistics.result = WinResult.Draw;
        }

        SoundManager.Instance.RpcPlaySoundAll(SoundEffect.GameStart);
        this.Delayed(3, () =>
        {
            timeController.RpcFinishGame(player1.statistics, player2.statistics);
        });
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

    public void ScoreGoal(Ball ball, Player defendingPlayer, GameObject goal)
    {
        Player attacker = GetOpponentOf(defendingPlayer);
        attacker.score++;
        ResetBall(ball, defendingPlayer);

        EffectsManager.Instance.RpcPlayGoalEffect(goal);
        SoundManager.Instance.RpcPlaySoundPlayer(SoundEffect.GoalScore, defendingPlayer.playerId);
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

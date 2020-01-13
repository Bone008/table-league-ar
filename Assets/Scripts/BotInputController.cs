using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BotInputController : NetworkBehaviour
{
    /// <summary>Assigned on the server when this controller is instantiated and assigned to a player.</summary>
    public Player player;
    
    public float moveSpeed;
    public float moveSpeedAngle;
    public float ballMaxVelocity;
    public float ballDesiredHitDistance;
    public float ballHitStrength;
    public float ballHitCooldown;
    public float ballHitScatterAngle;
    public float collectInterval;

    private Goal[] opponentGoals = null;
    private Ball targetBall = null;
    private float lastHitTime = 0;

    private float lastCollectTime = 0;
    
    [ServerCallback]
    void OnEnable()
    {
        StartCoroutine(HighLevelLoop());
    }

    public override void OnStartServer()
    {
        // Bots were born ready.
        player.isUserReady = true;
    }

    [ServerCallback]
    void Update()
    {
        if(targetBall != null)
        {
            if(opponentGoals == null ||opponentGoals.Length == 0)
            {
                opponentGoals = GameObject.FindGameObjectsWithTag(Constants.GOAL_TAG)
                    .Select(obj => obj.GetComponent<Goal>())
                    .Where(goal => goal.owner != player)
                    .ToArray();
                if(opponentGoals.Length == 0)
                {
                    Debug.LogError("BotInput: Could not find any opponent goals!");
                    return;
                }
            }

            // Pick goal, switches every 10 seconds.
            int goalIndex = Mathf.FloorToInt(Time.time / 10) % opponentGoals.Length;
            Vector3 goalPos = opponentGoals[goalIndex].transform.position;

            // Move towards ball.
            Vector3 movePos = targetBall.transform.position;
            Quaternion moveRot = Quaternion.LookRotation(goalPos - movePos, Vector3.up);
            movePos += ballDesiredHitDistance * (moveRot * Vector3.back);
            movePos.y = Mathf.Max(0.1f, movePos.y);

            transform.position = Vector3.MoveTowards(transform.position, movePos, Time.deltaTime * moveSpeed);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, moveRot, Time.deltaTime * moveSpeedAngle);

            // If in position and able to shoot, hit the ball.
            if ((transform.position - movePos).sqrMagnitude < 0.01f * 0.01f
                && Quaternion.Angle(transform.rotation, moveRot) < 5
                && Time.time - lastHitTime >= ballHitCooldown)
            {
                lastHitTime = Time.time;
                HitBall(targetBall);
            }
        }
    }

    private IEnumerator HighLevelLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            FindTargetBall();
            FindResource();
        }
    }

    private void FindTargetBall()
    {
        targetBall = null;

        GameObject[] balls = GameObject.FindGameObjectsWithTag(Constants.BALL_TAG);
        if (balls.Length == 0)
            return;

        GameObject target = balls.MinBy(ball => (ball.transform.position - transform.position).sqrMagnitude);
        if (player.ownedRectangle.Contains(target.transform.position) &&
            target.GetComponent<Rigidbody>().velocity.sqrMagnitude < ballMaxVelocity * ballMaxVelocity)
        {
            targetBall = target.GetComponent<Ball>();
        }
    }

    private void FindResource()
    {
        // Be a bit less OP.
        if (Time.time - lastCollectTime < collectInterval)
            return;

        GameObject[] resources = GameObject.FindGameObjectsWithTag(Constants.COLLECTABLE_TAG);
        if (resources.Length == 0)
            return;

        GameObject target = resources.MinBy(go => (go.transform.position - transform.position).sqrMagnitude);
        if (player.ownedRectangle.Contains(target.transform.position))
        {
            lastCollectTime = Time.time;
            player.StartCollect(target);
        }
    }

    private void HitBall(Ball ball)
    {
        Quaternion scatter = Quaternion.Euler(
            UnityEngine.Random.Range(-ballHitScatterAngle, ballHitScatterAngle),
            UnityEngine.Random.Range(-ballHitScatterAngle, ballHitScatterAngle),
            UnityEngine.Random.Range(-ballHitScatterAngle, ballHitScatterAngle));
        Vector3 direction = scatter * transform.rotation * Vector3.forward;
        if (direction.y < 0) direction.y = 0;
        direction.Normalize();

        Vector3 force = ballHitStrength * direction;
        player.HitBall(ball.gameObject, force.normalized, Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f));
    }
}

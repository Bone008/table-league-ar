﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class BotInput : MonoBehaviour
{
    public SceneRectangle ownedArea;
    public float moveSpeed;
    public float moveSpeedAngle;
    public float ballMaxVelocity;
    public float ballDesiredHitDistance;
    public float ballHitStrength;
    public float ballHitCooldown;
    public float ballHitScatterAngle;

    private Player player;
    private Goal[] opponentGoals = null;
    private Ball targetBall = null;
    private float lastHitTime = 0;

    void Start()
    {
        player = GetComponent<Player>();
    }

    void OnEnable()
    {
        StartCoroutine(HighLevelLoop());

    }

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
            FindTargetBall();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void FindTargetBall()
    {
        targetBall = null;

        GameObject[] balls = GameObject.FindGameObjectsWithTag(Constants.BALL_TAG);
        if (balls.Length == 0)
            return;

        GameObject target = balls.MinBy(ball => (ball.transform.position - transform.position).sqrMagnitude);
        if (ownedArea.Contains(target.transform.position) &&
            target.GetComponent<Rigidbody>().velocity.sqrMagnitude < ballMaxVelocity * ballMaxVelocity)
        {
            targetBall = target.GetComponent<Ball>();
        }
    }

    private void HitBall(Ball ball)
    {
        var rbody = ball.GetComponent<Rigidbody>();
        
        Quaternion scatter = Quaternion.Euler(
            UnityEngine.Random.Range(-ballHitScatterAngle, ballHitScatterAngle),
            UnityEngine.Random.Range(-ballHitScatterAngle, ballHitScatterAngle),
            UnityEngine.Random.Range(-ballHitScatterAngle, ballHitScatterAngle));
        Vector3 direction = scatter * transform.rotation * Vector3.forward;
        if (direction.y < 0) direction.y = 0;
        direction.Normalize();

        Vector3 force = ballHitStrength * direction;
        rbody.velocity = Vector3.zero;
        rbody.AddForce(force, ForceMode.Impulse);
    }
}
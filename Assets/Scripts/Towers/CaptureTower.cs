﻿using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptureTower : TowerBase
{
    public Transform head;
    public Transform verticalAim;
    public LineRenderer aimLine1;
    public LineRenderer aimLine2;
    public float targetLockDuration;
    public float captureEffectDuration;
    [Tooltip("degrees/second to spin head while idling")]
    public float idleSpinSpeed;
    [Tooltip("max degrees/second to spin head while actively targeting some direction")]
    public float activeSpinSpeed;
    
    private GameObject targetBall = null;
    private Rigidbody targetBallRb = null;
    private bool isHoldingBall = false;
    private bool isCapturingBall = false;
    private float currentLockTime = 0;

    [ServerCallback]
    void Start()
    {
        aimLine1.enabled = false;
        aimLine2.enabled = false;
    }
    
    void LateUpdate()
    {
        if (NetworkServer.active) UpdateServer();
        if (NetworkClient.active) UpdateClient();
    }

    [Client]
    private void UpdateClient()
    {

    }

    [Server]
    private void UpdateServer()
    {
        if (targetBall == null)
        {
            // Idle spin.
            head.Rotate(0, idleSpinSpeed * Time.deltaTime, 0, Space.World);
            verticalAim.localRotation = Quaternion.RotateTowards(verticalAim.localRotation, Quaternion.identity, 15 * Time.deltaTime);
        }
        else if (isHoldingBall)
        {
            if (targetBallRb.velocity.sqrMagnitude > 0.01f * 0.01f)
            {
                Debug.Log("CT Detected hit, releasing!");
                ReleaseBall();
            }
            else if(!isCapturingBall && owner.controllerTransform != null)
            {
                // Face same way as owner is aiming.
                head.RotateTowards(Quaternion.Euler(0, owner.controllerTransform.eulerAngles.y, 0), activeSpinSpeed * Time.deltaTime);
            }
        }
        else
        {
            Debug.Assert(!isHoldingBall);

            // Check if target left valid area.
            if (!IsWithinTargetArea(targetBall.transform.position))
            {
                ReleaseBall();
                return;
            }

            // Check if ready to capture locked on target.
            currentLockTime += Time.deltaTime;
            if (currentLockTime >= targetLockDuration)
            {
                StartCoroutine(CaptureBall());
                return;
            }

            // Visually look at ball.
            Vector3 lookDirection = targetBall.transform.position - head.position;
            Vector3 lookAngles = Quaternion.LookRotation(lookDirection).eulerAngles;
            head.RotateTowards(Quaternion.Euler(0, lookAngles.y, 0), activeSpinSpeed * Time.deltaTime);
            verticalAim.RotateTowardsLocal(Quaternion.Euler(lookAngles.x, 0, 0), activeSpinSpeed * Time.deltaTime);
            aimLine1.SetPosition(1, aimLine1.transform.InverseTransformPoint(targetBall.transform.position));
            aimLine2.SetPosition(1, aimLine2.transform.InverseTransformPoint(targetBall.transform.position));
            aimLine1.enabled = true;
            aimLine2.enabled = true;
        }
    }

    // Also call OnTriggerStay to prevent the detection delay of 1 frame otherwise.
    [ServerCallback]
    void OnTriggerEnter(Collider other) { OnTriggerStay(other); }

    // Used to detect new balls entering. Cannot rely on OnTriggerEnter cuz we also want to
    // test if the ball is in the true target area.
    [ServerCallback]
    void OnTriggerStay(Collider other)
    {
        if (isHoldingBall || targetBall == other.gameObject)
            return;
        if (other.isTrigger || !other.gameObject.CompareTag(Constants.BALL_TAG))
            return;
        if (!IsWithinTargetArea(other.transform.position))
            return;

        Debug.Log("CT Changing target to new ball", other.gameObject);
        targetBall = other.gameObject;
        targetBallRb = other.attachedRigidbody;
        currentLockTime = 0f;
    }

    [ServerCallback]
    void OnTriggerExit(Collider other)
    {
        if (!isHoldingBall && other.gameObject == targetBall)
        {
            ReleaseBall();
        }
    }

    [Server]
    private IEnumerator CaptureBall()
    {
        Debug.Log("CT Starting to capture ball!");
        isHoldingBall = true;
        isCapturingBall = true;
        targetBallRb.isKinematic = true;

        Vector3 startPos = targetBall.transform.position;
        Quaternion aimStartRot = verticalAim.localRotation;
        yield return this.Animate(captureEffectDuration, Util.EaseOut01, t =>
        {
            targetBall.transform.position = Vector3.Lerp(startPos, head.position, t);
            verticalAim.localRotation = Quaternion.Slerp(aimStartRot, Quaternion.identity, t);

            aimLine1.SetPosition(1, aimLine1.transform.InverseTransformPoint(targetBall.transform.position));
            aimLine2.SetPosition(1, aimLine2.transform.InverseTransformPoint(targetBall.transform.position));
        });

        isCapturingBall = false;
        targetBallRb.isKinematic = false;
        targetBallRb.velocity = Vector3.zero;
        targetBallRb.angularVelocity = Vector3.zero;
        targetBallRb.useGravity = false;
    }

    [Server]
    private void ReleaseBall()
    {
        Debug.Log("CT Target is releasing");
        targetBallRb.useGravity = true;
        aimLine1.enabled = false;
        aimLine2.enabled = false;
        targetBall = null;
        targetBallRb = null;
        isHoldingBall = false;
    }

    private bool IsWithinTargetArea(Vector3 pos)
    {
        return transform.InverseTransformPoint(pos).z >= 0;
    }
}
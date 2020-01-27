using Mirror;
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
    
    [SyncVar]
    private GameObject targetBall = null;

    private Rigidbody targetBallRb = null;
    private bool isHoldingBall = false;
    private bool isCapturingBall = false;
    private float currentLockTime = 0;

    [ServerCallback]
    void OnDestroy()
    {
        if(targetBallRb != null)
        {
            // Make sure to reset permanent effects on the captured ball.
            targetBallRb.isKinematic = false;
            targetBallRb.useGravity = true;
        }
    }
    
    void LateUpdate()
    {
        if (NetworkServer.active) UpdateServer();
        if (NetworkClient.active) UpdateClient();
    }

    [Client]
    private void UpdateClient()
    {
        if(targetBall == null && aimLine1.enabled)
        {
            aimLine1.enabled = false;
            aimLine2.enabled = false;
        }
        else if(targetBall != null)
        {
            aimLine1.enabled = true;
            aimLine2.enabled = true;
            aimLine1.SetPosition(1, aimLine1.transform.InverseTransformPoint(targetBall.transform.position));
            aimLine2.SetPosition(1, aimLine2.transform.InverseTransformPoint(targetBall.transform.position));
        }
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
            if (!isCapturingBall && targetBallRb.velocity.sqrMagnitude > 0.01f * 0.01f)
            {
                ReleaseBall();
            }
            else if(!isCapturingBall && isJammed)
            {
                Vector3 bounce = 0.05f * Scale.gameScale * Random.onUnitSphere;
                bounce.y = 6 * Mathf.Abs(bounce.y);
                targetBallRb.AddForce(bounce, ForceMode.Impulse);
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
            // Ignore if some other higher force is already controlling the ball (isKinematic == true).
            currentLockTime += Time.deltaTime;
            if (currentLockTime >= targetLockDuration && !targetBallRb.isKinematic)
            {
                StartCoroutine(CaptureBall());
                return;
            }

            // Visually look at ball.
            Vector3 lookDirection = targetBall.transform.position - head.position;
            Vector3 lookAngles = Quaternion.LookRotation(lookDirection).eulerAngles;
            head.RotateTowards(Quaternion.Euler(0, lookAngles.y, 0), activeSpinSpeed * Time.deltaTime);
            verticalAim.RotateTowardsLocal(Quaternion.Euler(lookAngles.x, 0, 0), activeSpinSpeed * Time.deltaTime);
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
        if (isJammed)
            return;
        if (targetBall != null)
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
    protected override void OnJammedStart()
    {
        // Reset ball target if we are currently targeting a ball but
        // NOT holding it yet. Releasing a held ball is handled in UpdateServer().
        // If we are currently capturing a ball, we are also holding it and will release it
        // once capturing is complete.
        if(!isHoldingBall)
        {
            targetBall = null;
            targetBallRb = null;
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
        yield return Util.DoAnimate(captureEffectDuration, Util.EaseOut01, t =>
        {
            targetBall.transform.position = Vector3.Lerp(startPos, head.position, t);
            verticalAim.localRotation = Quaternion.Slerp(aimStartRot, Quaternion.identity, t);
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
        targetBall = null;
        targetBallRb = null;
        isHoldingBall = false;
    }

    private bool IsWithinTargetArea(Vector3 pos)
    {
        return transform.InverseTransformPoint(pos).z >= 0;
    }
}

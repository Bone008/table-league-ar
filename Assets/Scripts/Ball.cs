using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    private Rigidbody rbody;
    
    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        // Make sure the ball can spin fast enough for high speeds.
        rbody.maxAngularVelocity = 100;
    }

    public override void OnStartClient()
    {
        if (!isClientOnly)
            return;
        rbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rbody.isKinematic = true;
    }

    [Server]
    public void Reset(Vector3 position)
    {
        Vector3 targetPosition = position;

        // Move the target position upwards until it is no longer occupied by other balls.
        GameObject[] otherBalls = GameObject.FindGameObjectsWithTag(Constants.BALL_TAG);
        float diameter = transform.localScale.y;
        float minDistance;
        while ((minDistance = otherBalls
            .Where(other => other != gameObject)
            .Select(other => (other.transform.position - targetPosition).sqrMagnitude)
            .DefaultIfEmpty(float.PositiveInfinity)
            .Min()) < diameter * diameter)
        {
            targetPosition += (diameter*1.05f) * Vector3.up;
        }

        rbody.velocity = Vector3.zero;
        rbody.angularVelocity = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.position = targetPosition;
    }

}

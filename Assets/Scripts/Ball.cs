using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody rbody;

    // Start is called before the first frame update
    void Start()
    {
        rbody = GetComponent<Rigidbody>();
        // Make sure the ball can spin fast enough for high speeds.
        rbody.maxAngularVelocity = 100;
    }

    // Update is called once per frame
    void Update()
    {
        // Debug
        if (Input.GetKeyDown(KeyCode.F))
        {
            rbody.velocity = Vector3.zero;
            rbody.angularVelocity = Vector3.zero;
        }
    }

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

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.tag == "Walls")
        {
            Vector3 velocity;
            //Vector3 dir = c.contacts[0].point - transform.position;
            //dir = -dir.normalized;
            velocity = GetComponent<Rigidbody>().velocity;
            //GetComponent<Rigidbody>().velocity = velocity - 2 * (Vector3.Dot(velocity, dir)) * dir;


            velocity = 2 * (Vector3.Dot(velocity, Vector3.Normalize(c.contacts[0].normal))) * Vector3.Normalize(c.contacts[0].normal) - velocity; //Following formula  v' = 2 * (v . n) * n - v

            velocity *= -1;
            // No longer needed with proper physics settings
            //GetComponent<Rigidbody>().velocity = velocity;
        }
    }

}

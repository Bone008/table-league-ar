using System.Collections;
using System.Collections.Generic;
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
        if(Input.GetKeyDown(KeyCode.F))
        {
            rbody.velocity = Vector3.zero;
            rbody.angularVelocity = Vector3.zero;
        }

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

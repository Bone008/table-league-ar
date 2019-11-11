using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierHitRegion : MonoBehaviour
{
    public BarrierTower tower;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag(Constants.BALL_TAG))
        {
            Vector3 localBallPosition = transform.InverseTransformPoint(collision.gameObject.transform.position);
            if (localBallPosition.z > 0)
            {
                tower.OnBallCollide(collision);
            }
            else Debug.Log("ignored collision from behind");
        }
    }
}

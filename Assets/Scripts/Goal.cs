using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Player owner;

    void OnTriggerEnter(Collider other) {
        if (!NetworkServer.active)
            return;

        if(other.gameObject.CompareTag(Constants.BALL_TAG) && !other.isTrigger)
        {
            if(owner == null)
            {
                Debug.LogWarning("Goal does not have an owner!", this);
                return;
            }
            GameManager.Instance.ScoreGoal(other.gameObject.GetComponent<Ball>(), owner);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Player owner;

    void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag(Constants.BALL_TAG))
        {
            GameManager.Instance.ScoreGoal(other.gameObject.GetComponent<Ball>(), owner);
        }
    }
}

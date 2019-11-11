using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierTower : MonoBehaviour
{
    public Transform anchor;
    public float ballImpulse = 2f;
    public float knockOverDuration = 0.2f;
    public AnimationCurve knockOverCurve;
    public float recoverDuration = 5f;

    private bool isReady = true;

    public void OnBallCollided(Collision collision)
    {
        if (isReady)
        {
            collision.rigidbody.AddForce(ballImpulse * anchor.forward, ForceMode.Impulse);
            StartCoroutine(KnockOver());
        }
    }

    private IEnumerator KnockOver()
    {
        isReady = false;
        yield return this.AnimateVector(knockOverDuration, Vector3.zero, new Vector3(-85, 0, 0), knockOverCurve, v => anchor.localEulerAngles = v);
        yield return new WaitForSeconds(recoverDuration);
        this.AnimateVector(3*knockOverDuration, new Vector3(-85, 0, 0), Vector3.zero, knockOverCurve, v => anchor.localEulerAngles = v);
        isReady = true;
    }
}

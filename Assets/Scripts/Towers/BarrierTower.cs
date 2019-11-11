using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierTower : MonoBehaviour
{
    public Transform anchor;
    public float ballMinImpactVelocity = 0.15f;
    public float ballImpulse = 2f;
    public float knockOverDuration = 0.2f;
    public AnimationCurve knockOverCurve;
    public float recoverDuration = 5f;

    private bool isReady = true;

    public void OnBallCollide(Collision collision)
    {
        if (isReady)
        {
            // Note that if the ball is bouncing (which it will if it is fast),
            // its velocity is already inverted here, so it should point AWAY from the barrier anchor.
            Vector3 velocityBeforeImpact = collision.rigidbody.velocity - collision.impulse / collision.rigidbody.mass;
            float impactVelocity = Vector3.Dot(-anchor.forward, velocityBeforeImpact);
            Debug.Log("barrier impact velocity: " + impactVelocity);
            if (impactVelocity >= ballMinImpactVelocity)
            {
                collision.rigidbody.AddForce(ballImpulse * anchor.forward, ForceMode.Impulse);
                StartCoroutine(KnockOver());
            }
        }
    }

    void OnDisable()
    {
        // Re-enable tower when it is disabled, since coroutines are canceled.
        anchor.localEulerAngles = Vector3.zero;
        isReady = true;
    }

    private IEnumerator KnockOver()
    {
        isReady = false;
        yield return this.AnimateVector(knockOverDuration, Vector3.zero, new Vector3(-85, 0, 0), knockOverCurve, v => anchor.localEulerAngles = v);
        yield return new WaitForSeconds(recoverDuration);
        yield return this.AnimateVector(2 * knockOverDuration, new Vector3(-85, 0, 0), Vector3.zero, knockOverCurve, v => anchor.localEulerAngles = v);
        isReady = true;
    }
}

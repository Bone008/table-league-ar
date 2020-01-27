using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarrierTower : TowerBase
{
    public Transform anchor;
    public Collider hitRegionCollider;
    public float ballMinImpactVelocity = 0.15f;
    public float ballImpulse = 2f;
    public float knockOverDuration = 0.2f;
    public AnimationCurve knockOverCurve;
    public float recoverDuration = 5f;

    private bool isReady = true;

    [Server]
    public void OnBallCollide(Collision collision)
    {
        if (isReady)
        {
            // Note that if the ball is bouncing (which it will if it is fast),
            // its velocity is already inverted here, so it should point AWAY from the barrier anchor.
            Vector3 velocityBeforeImpact = collision.rigidbody.velocity - collision.impulse / collision.rigidbody.mass;
            float impactVelocity = Vector3.Dot(-anchor.forward, velocityBeforeImpact);
            Debug.Log("barrier impact velocity: " + impactVelocity);
            if (impactVelocity >= ballMinImpactVelocity * Scale.gameScale)
            {
                collision.rigidbody.AddForce(ballImpulse * Scale.gameScale * anchor.forward, ForceMode.Impulse);
                StartCoroutine(KnockOver());
                RpcAlsoKnockOverOnClient();

                SoundManager.Instance.RpcPlaySoundAll(SoundEffect.BarrierKnockOver);
            }
        }
    }

    [Server]
    protected override void OnJammedStart()
    {
        if (isReady || hitRegionCollider.enabled)
        {
            StartCoroutine(KnockOver());
            RpcAlsoKnockOverOnClient();
        }
    }

    [ClientRpc]
    private void RpcAlsoKnockOverOnClient()
    {
        if(isClientOnly)
            StartCoroutine(KnockOver());
    }

    [ServerCallback]
    void OnDisable()
    {
        // Re-enable tower when it is disabled, since coroutines are canceled.
        anchor.localEulerAngles = Vector3.zero;
        hitRegionCollider.enabled = true;
        isReady = true;
    }

    private IEnumerator KnockOver()
    {
        isReady = false;
        yield return Util.DoAnimateVector(knockOverDuration, Vector3.zero, new Vector3(-85, 0, 0), knockOverCurve, v => anchor.localEulerAngles = v);
        hitRegionCollider.enabled = false;
        yield return new WaitForSeconds(recoverDuration);
        
        // Potentially wait for unjam before standing back up.
        while(isJammed)
        {
            yield return null;
        }

        hitRegionCollider.enabled = true;
        yield return Util.DoAnimateVector(2 * knockOverDuration, new Vector3(-85, 0, 0), Vector3.zero, knockOverCurve, v => anchor.localEulerAngles = v);
        isReady = true;
    }

}

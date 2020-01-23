using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ball : NetworkBehaviour
{
    public GameObject offScreenIndicatorPrefab;
    public GameObject freezeEffect;
    public GameObject clickEffect;
    public AnimationCurve grappleYCurve;
    private Rigidbody rbody;

    private Coroutine activeUnfreezeCoroutine = null;
    private Coroutine activeGrappleCoroutine = null;
    private GameObject activeOffScreenIndicator = null;

    void Awake()
    {
        rbody = GetComponent<Rigidbody>();
        // Make sure the ball can spin fast enough for high speeds.
        rbody.maxAngularVelocity = 100;
    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (transform.position.y < -100)
        {
            Debug.LogWarning("A ball left the play area and had to be reset.");
            Reset(new Vector3(0, 0.2f, 0.02f));
        }
    }

    public override void OnStartClient()
    {
        if (isClientOnly)
        {
            rbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rbody.isKinematic = true;
        }

        Canvas mainCanvas = FindObjectOfType<Canvas>();
        activeOffScreenIndicator = Instantiate(offScreenIndicatorPrefab, mainCanvas.transform);
        var script = activeOffScreenIndicator.GetComponent<OffScreenIndicator>();
        script.targetTransform = transform;
    }

    [ClientCallback]
    void OnDestroy()
    {
        Destroy(activeOffScreenIndicator);
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

    [Server]
    public void Freeze(float duration)
    {
        // If the ball is unready frozen, cancel the old timer to unfreeze it.
        if (activeUnfreezeCoroutine != null)
            StopCoroutine(activeUnfreezeCoroutine);
        // If the ball is getting grappled, cancel the grapple.
        if (activeGrappleCoroutine != null)
        {
            StopCoroutine(activeGrappleCoroutine);
            activeGrappleCoroutine = null;
        }

        rbody.velocity = Vector3.zero;
        rbody.angularVelocity = Vector3.zero;
        rbody.isKinematic = true;
        RpcSetFrozen(true);

        activeUnfreezeCoroutine = this.Delayed(duration, () =>
        {
            rbody.isKinematic = false;
            RpcSetFrozen(false);
            activeUnfreezeCoroutine = null;
        });
    }
    
    [Server]
    public bool CanGrapple() => activeGrappleCoroutine == null && activeUnfreezeCoroutine == null;

    [Server]
    public void Grapple(Transform grapplerTransform, Vector3 relativeTargetPos, SceneRectangle validRect)
    {
        if (activeGrappleCoroutine != null)
        {
            Debug.LogError("Grapple is still active!", this);
            return;
        }
        activeGrappleCoroutine = StartCoroutine(DoGrapple(grapplerTransform, relativeTargetPos, validRect));
    }

    private IEnumerator DoGrapple(Transform grapplerTransform, Vector3 relativeTargetPos, SceneRectangle validRect)
    {
        rbody.isKinematic = true;

        Vector3 startPos = transform.position;
        float radius = transform.localScale.y / 2;
        
        Func<Vector3> currentTargetPos = () =>
        {
            Vector3 pos = grapplerTransform.TransformPoint(relativeTargetPos);
            pos = validRect.ProjectPoint(pos, radius + 0.01f);
            // Make sure point is above ground and below ceiling
            pos.y = Mathf.Clamp(pos.y, radius, Constants.CEILING_HEIGHT - radius);
            return pos;
        };
        Func<Vector3> randomOffset = () => 0.001f * new Vector3(Mathf.Sin(50 * (100+Time.time)), Mathf.Sin(52 * (100+Time.time)), Mathf.Sin(54 * (100+Time.time)));

        yield return Util.DoAnimate(Constants.grappleTransitionDuration, Util.EaseOut01, t =>
        {
            Vector3 targetPos = currentTargetPos();
            targetPos.y += grappleYCurve.Evaluate(t);
            rbody.MovePosition(Vector3.Lerp(startPos, targetPos, t) + randomOffset());
        });
        // Hold in place.
        yield return Util.DoAnimate(Constants.grappleHoldDuration, _ =>
        {
            rbody.MovePosition(currentTargetPos() + randomOffset());
        });
        rbody.MovePosition(currentTargetPos());
        rbody.velocity = Vector3.zero;
        rbody.angularVelocity = Vector3.zero;

        // Go!
        rbody.isKinematic = false;
        activeGrappleCoroutine = null;
    }

    [Server]
    public void Hit()
    {
        if (!rbody.isKinematic)
            RpcHitEffect(true);
    }

    [ClientRpc]
    private void RpcHitEffect(bool value)
    {
        clickEffect.SetActive(false);
        clickEffect.SetActive(true);
    }

    [ClientRpc]
    private void RpcSetFrozen(bool value)
    {
        // TODO play more neat effects to activate/deactivate the freeze.
        freezeEffect.SetActive(value);
    }

}

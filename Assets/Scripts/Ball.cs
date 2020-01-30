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
    public GameObject grapplingHookPrefab;
    private Rigidbody rbody;

    // Server-side.
    private Coroutine activeUnfreezeCoroutine = null;
    private Coroutine activeGrappleCoroutine = null;
    private GameObject activeOffScreenIndicator = null;

    // Client-side.
    private GameObject currentGrapplingHook = null;

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
            Reset(Scale.gameScale * new Vector3(0, 0.2f, 0.02f));
        }
    }

    public override void OnStartServer()
    {
        // Note: Disabled since it seems to be very inaccurate.
        // Just leaving drag unchanged feels much more consistent.
        // Drag should be quadratic in the velocity.
        //rbody.drag /= Scale.gameScale * Scale.gameScale;
        //rbody.angularDrag /= Scale.gameScale * Scale.gameScale;
    }

    public override void OnStartClient()
    {
        if (isClientOnly)
        {
            rbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            rbody.isKinematic = true;
        }

        // Spawn off screen UI elements. But not for spectators.
        if (PlayerNetController.LocalInstance?.player != null)
        {
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            activeOffScreenIndicator = Instantiate(offScreenIndicatorPrefab, mainCanvas.transform);
            var script = activeOffScreenIndicator.GetComponent<OffScreenIndicator>();
            script.targetTransform = transform;
        }
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
            RpcHideGrapple();
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
            Debug.LogError("Grapple is already active!", this);
            return;
        }
        activeGrappleCoroutine = StartCoroutine(DoGrapple(grapplerTransform, relativeTargetPos, validRect));
        RpcShowGrapple(grapplerTransform.gameObject);
    }

    private IEnumerator DoGrapple(Transform grapplerTransform, Vector3 relativeTargetPos, SceneRectangle validRect)
    {
        yield return new WaitForSeconds(Constants.grappleShootDuration);
        rbody.isKinematic = true;

        Vector3 startPos = transform.position;
        float radius = transform.localScale.y / 2;
        
        Func<Vector3> currentTargetPos = () =>
        {
            Vector3 pos = grapplerTransform.TransformPoint(relativeTargetPos);
            pos = validRect.ProjectPoint(pos, radius + 0.01f);
            // Make sure point is above ground and below ceiling
            pos.y = Mathf.Clamp(pos.y, radius, Constants.scaledCeilingHeight - radius);
            return pos;
        };

        yield return Util.DoAnimate(Constants.grapplePullDuration, Util.EaseOut01, t =>
        {
            Vector3 targetPos = currentTargetPos();
            targetPos.y += grappleYCurve.Evaluate(t) * Scale.gameScale;
            rbody.MovePosition(Vector3.Lerp(startPos, targetPos, t));
        });
        // Hold in place.
        yield return Util.DoAnimate(Constants.grappleHoldDuration, _ =>
        {
            rbody.MovePosition(currentTargetPos());
        });
        rbody.velocity = Vector3.zero;
        rbody.angularVelocity = Vector3.zero;

        // Go!
        rbody.isKinematic = false;
        RpcHideGrapple();
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

    [ClientRpc]
    private void RpcShowGrapple(GameObject source)
    {
        if(currentGrapplingHook == null)
            currentGrapplingHook = Instantiate(grapplingHookPrefab);
        var effect = currentGrapplingHook.GetComponent<GrappleEffect>();
        effect.source = source.transform;
        effect.target = transform;
    }

    [ClientRpc]
    private void RpcHideGrapple()
    {
        if(currentGrapplingHook != null)
        {
            Destroy(currentGrapplingHook);
            currentGrapplingHook = null;
        }
    }

}

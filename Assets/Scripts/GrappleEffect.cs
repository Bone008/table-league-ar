using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Client-only script to animate the visible grappling hook towards the ball.</summary>
[RequireComponent(typeof(LineRenderer))]
public class GrappleEffect : MonoBehaviour
{
    public Transform source;
    public Transform target;
    public float cameraYOffset = 0.04f;
    public Transform spinningHook;

    private LineRenderer line;
    private float startTime;
    private float spinVelocity;

    private void Start()
    {
        line = GetComponent<LineRenderer>();
        startTime = Time.time;

        spinningHook.transform.localEulerAngles = new Vector3(0, Random.Range(0f, 90f), 0);
        spinVelocity = (Random.Range(0, 2) == 0 ? -1 : 1) * Random.Range(400f, 700f);
    }

    void LateUpdate()
    {
        var sourcePos = source.position + cameraYOffset * Vector3.down;
        float shootProgress = Mathf.Min(1f, (Time.time - startTime) / Constants.grappleShootDuration);
        Vector3 offset = Util.EaseIn01(1 - shootProgress) * 0.4f * Vector3.up;
        transform.position = Vector3.Lerp(sourcePos, target.position + offset, shootProgress);
        transform.rotation = Quaternion.LookRotation(target.position - sourcePos);
        line.SetPosition(1, transform.InverseTransformPoint(sourcePos));

        // Spin until we hit our target.
        if (shootProgress < 1f)
            spinningHook.transform.Rotate(0, spinVelocity * Time.deltaTime, 0, Space.Self);
    }
}

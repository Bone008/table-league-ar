using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class MagneticTower : MonoBehaviour
{
    [Tooltip("positive = attract ball, negative = repulse ball")]
    public float pullForce;
    public float maxEnergy = 0.2f;
    public float rechargeDelay = 1;

    private float maxRadius;
    private float lastActivation = float.NegativeInfinity;
    private float energy = 0;

    void Start()
    {
        maxRadius = transform.lossyScale.x * GetComponent<CapsuleCollider>().radius;
    }

    void Update()
    {
        // Recharge energy if the tower hasn't triggered for a while.
        if (Time.time - lastActivation >= rechargeDelay)
        {
            energy = Mathf.Min(maxEnergy, energy + Time.deltaTime);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag(Constants.BALL_TAG))
        {
            return;
        }

        lastActivation = Time.time;
        if (energy < Time.fixedDeltaTime)
        {
            return;
        }
        energy -= Time.fixedDeltaTime;

        Vector3 dir = transform.position - other.transform.position;
        dir.y = 0; // Ignore vertical component.
        // At the range border, pull strength should be 100%,
        // increasing linearly with proximity up to 200%.
        // In reality, it should be quadratic, but that is way too extreme.
        float distFactor = Mathf.Min(2, maxRadius / dir.magnitude);
        Vector3 force = (pullForce * distFactor) * dir.normalized;
        other.attachedRigidbody.AddForce(force, ForceMode.Force);
    }
}

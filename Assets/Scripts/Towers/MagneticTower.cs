using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class MagneticTower : MonoBehaviour
{
    [Tooltip("positive = attract ball, negative = repulse ball")]
    public float pullStrength;
    public float maxEnergy = 0.2f;
    public float rechargeDelay = 1;

    private float lastActivation = float.NegativeInfinity;
    private float energy = 0;

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
        Vector3 force = (pullStrength / dir.sqrMagnitude) * dir.normalized;
        other.attachedRigidbody.AddForce(force, ForceMode.Force);
    }
}

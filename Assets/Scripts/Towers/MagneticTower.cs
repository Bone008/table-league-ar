using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class MagneticTower : TowerBase
{
    [Tooltip("positive = attract ball, negative = repulse ball")]
    public float pullForce;
    public float maxEnergy = 0.2f;
    public float rechargeDelay = 1;
    public float effectCooldown = 0.5f;

    [SyncVar(hook = nameof(OnUpdateEnergy))]
    private float energy = 0;

    private float maxRadius;
    private float lastActivationTime = float.NegativeInfinity;
    private float lastEffectTime = float.NegativeInfinity;

    public Transform circleTransform;
    public Renderer circleRenderer;
    private Color circleInitialColor;
    private readonly int shaderColorProp = Shader.PropertyToID("_BaseColor");

    [ServerCallback]
    void Start()
    {
        maxRadius = transform.lossyScale.x * GetComponent<CapsuleCollider>().radius;
        circleInitialColor = circleRenderer.material.GetColor(shaderColorProp);
    }

    [ServerCallback]
    protected override void Update()
    {
        base.Update();

        // Recharge energy if the tower hasn't triggered for a while.
        if (Time.time - lastActivationTime >= rechargeDelay)
        {
            energy = Mathf.Min(maxEnergy, energy + Time.deltaTime);
        }

        // Keep energy at 0 if jammed.
        if (isJammed) energy = 0;
    }

    [ServerCallback]
    void OnTriggerStay(Collider other)
    {
        if (!other.gameObject.CompareTag(Constants.BALL_TAG) || other.isTrigger)
        {
            return;
        }

        if (energy < Time.fixedDeltaTime)
        {
            return;
        }
        energy -= Time.fixedDeltaTime;
        lastActivationTime = Time.time;

        Vector3 dir = transform.position - other.transform.position;
        dir.y = 0; // Ignore vertical component.
        // At the range border, pull strength should be 100%,
        // increasing linearly with proximity up to 200%.
        // In reality, it should be quadratic, but that is way too extreme.
        float distFactor = Mathf.Min(2, maxRadius / dir.magnitude);
        Vector3 force = (pullForce * distFactor * Scale.gameScale) * dir.normalized;
        other.attachedRigidbody.AddForce(force, ForceMode.Force);

        if (Time.time - lastEffectTime >= effectCooldown)
        {
            lastEffectTime = Time.time;
            if (pullForce > 0) EffectsManager.Instance.RpcPlayTowerPullEffect(gameObject);
            else EffectsManager.Instance.RpcPlayTowerPushEffect(gameObject);

            SoundManager.Instance.RpcPlaySoundAll(SoundEffect.MagnetActivate);
        }
    }
    
    private void OnUpdateEnergy(float value)
    {
        float t = value / maxEnergy;
        Debug.Log("updating energy to " + t);
        //circleRenderer.material.SetColor(shaderColorProp, Color.Lerp(Color.black, circleInitialColor, t));
        circleTransform.localScale = t * Vector3.one;
    }
}

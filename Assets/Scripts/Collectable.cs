using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : NetworkBehaviour
{
    public CollectableType type;
    public int amount;
    public float collectDuration;
    public float timeToLive;
    public ParticleSystem collectEffect;

    private float creationTime;

    [ServerCallback]
    void Start()
    {
        creationTime = Time.time;
    }

    [ServerCallback]
    void Update()
    {
        if(timeToLive > 0 && Time.time - creationTime > timeToLive)
        {
            Destroy(gameObject);
        }
    }

    [Server]
    public void StartCollecting(Player who) {
        RpcTriggerEffect(true);
        // Reset despawn timer whenever the player interacts with it.
        creationTime = Time.time;
    }

    [Server]
    public void StopCollecting() {
        RpcTriggerEffect(false);
    }

    [ClientRpc]
    private void RpcTriggerEffect(bool active)
    {
        if (!collectEffect)
            return;

        if (active)
            collectEffect.Play();
        else
            collectEffect.Stop();
    }
}

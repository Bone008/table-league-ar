using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : NetworkBehaviour
{
    public CollectableType type;
    public float collectDuration;
    public ParticleSystem collectEffect;

    [Server]
    public void StartCollecting(Player who) {
        RpcTriggerEffect(true);
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

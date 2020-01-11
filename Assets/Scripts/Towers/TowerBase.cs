using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all root-level tower scripts.
/// Make sure that all derived logic only happens on the server!
/// </summary>
public abstract class TowerBase : NetworkBehaviour
{
    public Player owner;

    /// <summary>Server- & client-side jam status of the tower.</summary>
    [SyncVar]
    protected bool isJammed = false;

    private float remainingJammedTime = 0;

    [ServerCallback]
    protected virtual void Update()
    {
        if(isJammed)
        {
            remainingJammedTime -= Time.deltaTime;
            if(remainingJammedTime <= 0)
            {
                isJammed = false;
                OnJammedEnd();
            }
        }
    }

    public void JamForDuration(float duration)
    {
        remainingJammedTime = duration;
        if (!isJammed)
        {
            isJammed = true;
            OnJammedStart();
        }
    }

    [Server]
    protected virtual void OnJammedStart() { }
    [Server]
    protected virtual void OnJammedEnd() { }
}

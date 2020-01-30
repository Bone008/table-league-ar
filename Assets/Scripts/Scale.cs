using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scale : NetworkBehaviour
{
    /// <summary>Shared factor of scaling of the scene.</summary>
    public static float gameScale = 1f;
    public static Scale Instance { get; private set; }

    public event System.Action GameScaleChanged;

    private Vector3 initialGravity = Physics.gravity;

    void Awake()
    {
        Instance = this;
        UpdatePhysics();
    }

    /// <summary>Centralized way to control the scale. Note that this shouldn't be touched after the game has started.</summary>
    [Server]
    public void SetScale(float value)
    {
        // Broadcast scale change to all connected clients.
        RpcChangeScale(value);
    }
    
    [ClientRpc]
    private void RpcChangeScale(float value) => DoSetScale(value);
    
    /// <summary>Necessary to transfer the current scale to a new connected client.</summary>
    [TargetRpc]
    public void TargetRpcChangeScale(NetworkConnection conn, float value) => DoSetScale(value);

    private void DoSetScale(float value)
    {
        // Update static accessor.
        gameScale = value;
        GameScaleChanged?.Invoke();
        UpdatePhysics();
    }

    private void UpdatePhysics()
    {
        Physics.gravity = initialGravity * gameScale;
    }
}

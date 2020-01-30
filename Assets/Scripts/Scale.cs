using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scale : NetworkBehaviour
{
    /// <summary>Shared factor of scaling of the scene.</summary>
    public static float gameScale = 1f;

    public static Scale Instance { get; private set; }

    // Lazily initialized because physics settings cannot be read from a static initializer.
    // It has to be static because after we change the initial values once, we need to remember them
    // even when reloading GameScene.
    private static ScaledPhysics _scaledPhysics = null;
    private static ScaledPhysics scaledPhysics => _scaledPhysics ?? (_scaledPhysics = new ScaledPhysics());

    public event System.Action GameScaleChanged;

    
    // Hack: We need to use the constructor here, because somehow in a standalone player,
    // Awake() is called way too late for networked GameObjects. In the Editor, it is properly
    // called before any other events, but after building, scripts like FollowScale will not find
    // this instance if Scale's initialization is done in Awake();
    public Scale()
    {
        Instance = this;
        //Debug.Log("SCALE MANAGER INSTANTIATED");
    }

    /// <summary>Centralized way to control the scale. Note that this shouldn't be touched after the game has started.
    /// IMPORTANT: Scale.gameScale is not immediately changed after calling SetScale, since it dispatches an RPC.</summary>
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
        scaledPhysics.Set(value);
    }
    
    private class ScaledPhysics
    {
        private readonly Vector3 gravity =  Physics.gravity;
        private readonly float bounceThreshold = Physics.bounceThreshold;
        private readonly float sleepThreshold = Physics.sleepThreshold;
        private readonly float defaultContactOffset = Physics.defaultContactOffset;

        public void Set(float scale)
        {
            Physics.gravity = gravity * scale;
            Physics.bounceThreshold = bounceThreshold * scale;
            Physics.sleepThreshold = sleepThreshold * scale;
            Physics.defaultContactOffset = defaultContactOffset * scale;
        }
    }
}

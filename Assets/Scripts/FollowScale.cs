using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Component to add to any GameObject that should be scaled according to Scale.gameScale.</summary>
public class FollowScale : MonoBehaviour
{
    private Vector3 initialScale;

    void Awake()
    {
        // Ignore on the client, but only if the transform is networked.
        if (!NetworkServer.active && TryGetComponent<NetworkTransform>(out _))
        {
            enabled = false;
            return;
        }

        initialScale = transform.localScale;
        OnGameScaleChanged();
        Scale.Instance.GameScaleChanged += OnGameScaleChanged;
    }

    void OnDestroy()
    {
        Scale.Instance.GameScaleChanged -= OnGameScaleChanged;
    }
    
    private void OnGameScaleChanged()
    {
        transform.localScale = initialScale * Scale.gameScale;
    }
}

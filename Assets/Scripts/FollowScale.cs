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
        initialScale = transform.localScale;
    }

    void OnEnable()
    {
        // Ignore on the client, but only if the transform is networked.
        if (!NetworkServer.active && TryGetComponent<NetworkTransform>(out _))
        {
            Debug.Log("DO NOT scale " + gameObject.name + " on the client.", this);
            return;
        }
        
        OnGameScaleChanged();

        if(Scale.Instance == null)
        {
            Debug.LogError("Scale manager not initialized while enabling " + gameObject.name + "!", this);
            return;
        }
        Scale.Instance.GameScaleChanged += OnGameScaleChanged;
    }

    void OnDisable()
    {
        if (Scale.Instance == null)
        {
            Debug.LogError("Scale manager not initialized while disabling " + gameObject.name + "!", this);
            return;
        }

        Scale.Instance.GameScaleChanged -= OnGameScaleChanged;
    }
    
    private void OnGameScaleChanged()
    {
        transform.localScale = initialScale * Scale.gameScale;
        Debug.Log($"Scaling {gameObject.name} from {initialScale} to {transform.localScale}.", this);
    }
}

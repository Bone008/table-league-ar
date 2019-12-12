using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : NetworkBehaviour
{
    public static EffectsManager Instance { get; private set; }

    public LineRenderer interactionLine;

    private Transform interactionLineSource = null;
    
    void Awake() { Instance = this; }

    [ClientCallback]
    void LateUpdate()
    {
        if (interactionLineSource)
        {
            interactionLine.SetPosition(0, interactionLineSource.position);
        }
    }

    [ClientRpc]
    public void RpcShowInteraction(GameObject playerObject, Vector3 targetPos)
    {
        if (PlayerNetController.LocalInstance?.player?.gameObject == playerObject)
            return;
        interactionLineSource = playerObject.transform.GetChild(0);
        interactionLine.SetPosition(1, targetPos);
        interactionLine.enabled = true;
    }

    [ClientRpc]
    public void RpcHideInteraction()
    {
        interactionLineSource = null;
        interactionLine.enabled = false;
    }
}

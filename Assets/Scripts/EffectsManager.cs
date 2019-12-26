using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : NetworkBehaviour
{
    public static EffectsManager Instance { get; private set; }

    public LineRenderer interactionLine;
    public GameObject towerDestroyEffectPrefab;

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

    [ClientRpc]
    public void RpcPlayTowerDestroyEffect(GameObject tower, float duration)
    {
        float finalStageTime = TowerManager.Instance.destroyEffectOnlyTime * 0.33f;

        var effect = Instantiate(towerDestroyEffectPrefab, tower.transform);
        effect.name = "__destroy_effect__";
        this.AnimateVector(duration - finalStageTime, -0.32f * Vector3.up, Vector3.zero, Util.EaseInOut01, v =>
        {
            if(effect) effect.transform.localPosition = v;
        });
        this.Delayed(duration - finalStageTime, () => this.AnimateScalar(finalStageTime, 1f, 0f, Util.EaseOut01, s =>
        {
            if (effect) tower.transform.localScale = new Vector3(s, 1, s);
        }));
    }

    [ClientRpc]
    public void RpcStopTowerDestroyEffect(GameObject tower)
    {
        var effect = tower.transform.Find("__destroy_effect__");
        if(effect)
        {
            Destroy(effect.gameObject);
        }
    }
}

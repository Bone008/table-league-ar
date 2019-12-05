using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Vuforia;

[RequireComponent(typeof(VuMarkBehaviour))]
public class VuforiaNumericDetector : MonoBehaviour
{
    public Transform forcedParent;
    public int currentMarkerId = -1;

    void Start()
    {
        UpdateTargetsActive();

        var vuBehavior = GetComponent<VuMarkBehaviour>();
        vuBehavior.RegisterVuMarkTargetAssignedCallback(() =>
        {
            currentMarkerId = (int)vuBehavior.VuMarkTarget.InstanceId.NumericValue;
            Debug.Log(string.Format("!!! Found marker #{0}", currentMarkerId));
            UpdateTargetsActive();

            if(forcedParent && transform.parent != forcedParent)
            {
                transform.SetParent(forcedParent, true);
            }
        });
        vuBehavior.RegisterVuMarkTargetLostCallback(() =>
        {
            Debug.Log(string.Format("!!! Lost marker #{0}", currentMarkerId));
            currentMarkerId = -1;
            UpdateTargetsActive();
        });
    }

    private void UpdateTargetsActive()
    {
        // Do nothing if we are already destroyed.
        // This looks really weird but because Unity overloads "==" it works ...
        if (this == null) return;
        
        foreach (var target in transform.GetComponentsInChildren<MarkerTarget>(true))
        {
            target.gameObject.SetActive(target.markerId == currentMarkerId);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using System.Linq;

public class TrackingController : MonoBehaviour
{
    private void Start()
    {
        var vuMarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        vuMarkManager.RegisterVuMarkDetectedCallback(target =>
        {
            // If necessary, here we can access the new VuMarkTarget (determine ID).
            // But the VuMarkBehavior is not available yet.
        });
        vuMarkManager.RegisterVuMarkBehaviourDetectedCallback(behavior =>
        {
            // If necessary, here we can access the assigned VuMarkBehavior.
            // But the VuMarkTarget is not available yet.
        });
    }

    void LateUpdate()
    {
        var vuMarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        //Debug.Log("active markers: " + string.Join(",", vuMarkManager.GetActiveBehaviours().Select(marker => marker.VuMarkTarget.InstanceId.NumericValue)));

        foreach (var marker in vuMarkManager.GetActiveBehaviours())
        {
            // Update our parent position so target with id #1 remains in world center.
            if(marker.VuMarkTarget.InstanceId.NumericValue == 1)
            {
                transform.localRotation = Quaternion.Inverse(marker.transform.localRotation);
                transform.localPosition = transform.localRotation * -marker.transform.localPosition;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TrackingController : MonoBehaviour
{
    /// <summary>Vuforia ID of the tracker that should be the world center.</summary>
    public int centerTrackerId = 1;
    public VuMarkBehaviour mainVuMark;
    public GameObject onlyWithTrackingTarget;
    public GameObject onlyWithoutTrackingTarget;
    public Transform scaleTarget;
    public Text debug;

    private bool hasTracking;

#if UNITY_EDITOR || UNITY_STANDALONE
    void Awake()
    {
        if (VuforiaConfiguration.Instance.WebCam.TurnOffWebCam)
        {
            GetComponent<UnityTemplateProjects.SimpleCameraController>().enabled = true;
            this.enabled = false;
        }
    }
#endif

    void Start()
    {
        UpdateTrackingStatus(false);

        // Through the editor, we can only set VuMarkWorldCenter,
        // which behaves incorrectly when a VuMark is cloned.
        // We need to keep track of which VuMarkBehavior is the main
        // tracker and should be the WorldCenter ourselves!
        VuforiaManager.Instance.WorldCenter = mainVuMark;
        VuforiaManager.Instance.VuMarkWorldCenter = null;
        
        var vuMarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        vuMarkManager.RegisterVuMarkBehaviourDetectedCallback(behavior =>
        {
            var numericScript = behavior.GetComponent<VuforiaNumericDetector>();
            if (numericScript == null)
                return;
            
            // VuMarkTarget was not assigned yet => marker ID not yet known.
            // Need to delay until next frame.
            this.Delayed(0, () =>
            {
                if(numericScript.currentMarkerId == centerTrackerId)
                {
                    UpdateTrackingStatus(true);
                    if(mainVuMark != behavior)
                    {
                        mainVuMark = behavior;
                        mainVuMark.transform.position = Vector3.zero;
                        mainVuMark.transform.rotation = Quaternion.identity;
                        VuforiaManager.Instance.WorldCenter = mainVuMark;
                        Debug.Log("#### mainVuMark has changed!", mainVuMark);
                    }
                }
            });
        });
        vuMarkManager.RegisterVuMarkLostCallback(target =>
        {
            if((int)target.InstanceId.NumericValue == centerTrackerId)
            {
                UpdateTrackingStatus(false);
            }
        });
    }

    void Update()
    {
        var tracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
        debug.text = mainVuMark.CurrentStatus + " -- " + mainVuMark.CurrentStatusInfo + " -- device tracker: " + tracker?.IsActive;
    }

    private void UpdateTrackingStatus(bool hasTracking)
    {
        this.hasTracking = hasTracking;
        if(onlyWithTrackingTarget != null)
            onlyWithTrackingTarget.SetActive(hasTracking);
        if (onlyWithoutTrackingTarget != null)
            onlyWithoutTrackingTarget.SetActive(!hasTracking);
    }

    public void SetScale(float scaleExponent)
    {
        float s = Mathf.Pow(10, scaleExponent);
        scaleTarget.localScale = s * Vector3.one;
    }

    public void ResetTrackers()
    {
        var tracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
        if (tracker == null)
        {
            Debug.LogError("Positional device tracker is not initialized!");
            return;
        }

        bool r = tracker.Reset();
        tracker.ResetAnchors();
        Debug.Log("tracker reset successful: " + r);
        debug.text = "tracker reset successful: " + r;
    }

    public void SetExtendedTracking(bool enabled)
    {
        var tracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
        if (tracker == null)
        {
            Debug.LogError("Positional device tracker is not initialized!");
            return;
        }
        if (!tracker.IsActive && enabled)
            tracker.Start();
        else if (tracker.IsActive && !enabled)
            tracker.Stop();
    }
}

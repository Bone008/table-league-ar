using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TrackingController : MonoBehaviour, ITrackableEventHandler
{
    public VuMarkBehaviour mainVuMark;
    public GameObject onlyWithTrackingTarget;
    public GameObject onlyWithoutTrackingTarget;
    public Text debug;
    public bool disableInEditor;

    private bool hasTracking;

#if UNITY_EDITOR
    void Awake()
    {
        if(disableInEditor)
        {
            GetComponent<UnityTemplateProjects.SimpleCameraController>().enabled = true;
            this.enabled = false;
        }
    }
#endif

    void Start()
    {
        UpdateTrackingStatus(false);
        mainVuMark.RegisterTrackableEventHandler(this);
    }

    void Update()
    {
        var tracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
        debug.text = mainVuMark.CurrentStatus + " -- " + mainVuMark.CurrentStatusInfo + " -- device tracker: " + tracker.IsActive;
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        UpdateTrackingStatus(newStatus != TrackableBehaviour.Status.NO_POSE);
    }

    private void UpdateTrackingStatus(bool hasTracking)
    {
        this.hasTracking = hasTracking;
        onlyWithTrackingTarget.SetActive(hasTracking);
        if (onlyWithoutTrackingTarget != null)
            onlyWithoutTrackingTarget.SetActive(!hasTracking);
    }

    public void SetScale(float scaleExponent)
    {
        float s = Mathf.Pow(10, scaleExponent);
        onlyWithTrackingTarget.transform.localScale = s * Vector3.one;
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

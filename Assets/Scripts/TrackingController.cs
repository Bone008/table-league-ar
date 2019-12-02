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
    public Transform scaleTarget;
    public Text debug;

    private bool hasTracking;

#if UNITY_EDITOR
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
        mainVuMark.RegisterTrackableEventHandler(this);

        VuforiaManager.Instance.WorldCenter = mainVuMark;
        VuforiaManager.Instance.VuMarkWorldCenter = null;


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
            //Debug.Log("----- target: " + behavior.VuMarkTarget);
            this.Delayed(0, () =>
            {
                Debug.Log("----- target delayed: " + behavior.VuMarkTarget);
                if(behavior.GetComponent<VuforiaNumericDetector>().currentMarkerId == 1)
                {
                    UpdateTrackingStatus(true);
                    if(mainVuMark != behavior)
                    {
                        mainVuMark = behavior;
                        Debug.Log("#### mainVuMark was changed!", mainVuMark);
                    }
                }
            });


        });
        vuMarkManager.RegisterVuMarkLostCallback(target =>
        {
            if(target.InstanceId.NumericValue == 1)
            {
                UpdateTrackingStatus(false);
            }
        });
    }

    void Update()
    {
        var tracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();
        debug.text = mainVuMark.CurrentStatus + " -- " + mainVuMark.CurrentStatusInfo + " -- device tracker: " + tracker.IsActive;
        if (Time.frameCount > 100 && Time.frameCount % 100 == 0)
        {
            Debug.Log(mainVuMark.Trackable.Name + " -- " + VuforiaManager.Instance.VuMarkWorldCenter, VuforiaManager.Instance.VuMarkWorldCenter as UnityEngine.Object);
            //Debug.Log(.Trackable, VuforiaManager.Instance.WorldCenter as UnityEngine.Object);
        }
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
    {
        //UpdateTrackingStatus(newStatus != TrackableBehaviour.Status.NO_POSE);
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

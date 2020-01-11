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
    public LayerMask inactiveCullingMask;
    public GameObject onlyWithTrackingTarget;
    public GameObject onlyWithoutTrackingTarget;
    public Text debug;

    private int initialCullingMask;
    private bool hasTracking;

#if UNITY_EDITOR || UNITY_STANDALONE
    void Awake()
    {
        if (VuforiaConfiguration.Instance.WebCam.TurnOffWebCam)
        {
            GetComponent<UnityTemplateProjects.SimpleCameraController>().enabled = true;
            this.enabled = false;
            GameObject.Find("Game UI").GetComponent<GameUIManager>().ToggleFloorVisibility();
        }
    }
#endif

    void Start()
    {
        initialCullingMask = Camera.main.cullingMask;
        UpdateTrackingStatus(false);

        this.Delayed(0.5f, () =>
        {
            // Vuforia automatically creates this child, we want it to be on a separate layer so we can render
            // only the background video when tracking is inactive.
            Camera.main.transform.Find("BackgroundPlane").gameObject.layer = LayerMask.NameToLayer("BackgroundPlane");
        });

        bool focusSuccess = CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        Debug.Log("Focus mode continuous auto supported: " + focusSuccess);
        if(!focusSuccess)
        {
            focusSuccess = CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_NORMAL);
            Debug.Log("Focus mode fallback default success: " + focusSuccess);
        }

        // Through the editor, we can only set VuMarkWorldCenter,
        // which behaves incorrectly when a VuMark is cloned.
        // We need to keep track of which VuMarkBehavior is the main
        // tracker and should be the WorldCenter ourselves!
        VuforiaManager.Instance.WorldCenter = mainVuMark;
        VuforiaManager.Instance.VuMarkWorldCenter = null;
        
        var vuMarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        
        vuMarkManager.RegisterVuMarkBehaviourDetectedCallback(behavior =>
        {
            if (!this)
                return;
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
            if (!this)
                return;
            if ((int)target.InstanceId.NumericValue == centerTrackerId)
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

        // Only render UI when there is no tracking.
        Camera.main.cullingMask = hasTracking ? initialCullingMask : inactiveCullingMask.value;

        if (PlayerNetController.LocalInstance)
        {
            PlayerNetController.LocalInstance.CmdSetHasTracking(hasTracking);
        }
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

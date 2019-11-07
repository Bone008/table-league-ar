using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TrackingController : MonoBehaviour
{
    private const int MAX_TRACKER_ID = 10;
    
    private class TrackerState
    {
        public readonly int id;
        public bool isRegistered = false;
        public Quaternion knownWorldRotation = Quaternion.identity;
        public Vector3 knownWorldPosition = Vector3.zero;
        public GameObject visualRepresentation = null;

        public TrackerState(int id) { this.id = id; }
    }

    /// <summary>Vuforia ID of the tracker that should be the world center.</summary>
    public int centerTrackerId = 1;
    /// <summary>Prefab of the world-space representation of a tracker.</summary>
    public GameObject trackerCenterPrefab;
    public GameObject onlyWithTrackingTarget;
    public Text debug;

    private VuMarkManager vuMarkManager;
    private TrackerState[] trackers = new TrackerState[MAX_TRACKER_ID + 1];

    void Start()
    {
        vuMarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        for (int i = 0; i <= MAX_TRACKER_ID; i++)
        {
            trackers[i] = new TrackerState(i);
        }

        vuMarkManager.RegisterVuMarkDetectedCallback(target =>
        {
            // If necessary, here we can access the new VuMarkTarget (determine ID).
            // But the VuMarkBehavior is not available yet.
            var tracker = GetTrackerFromTarget(target);
            if (tracker != null && tracker.id == centerTrackerId)
            {
                // Center tracker is automatically registered to the origin.
                tracker.isRegistered = true;
                tracker.knownWorldPosition = Vector3.zero;
            }
        });
        vuMarkManager.RegisterVuMarkBehaviourDetectedCallback(behavior =>
        {
            // If necessary, here we can access the assigned VuMarkBehavior.
            // But the VuMarkTarget is not available yet.
        });
    }

    void LateUpdate()
    {
        // Use all active registered trackers to determine world-space camera position.
        Quaternion averageCameraRotation = Quaternion.identity;
        List<Vector3> predictedCameraPositions = new List<Vector3>(2 * MAX_TRACKER_ID);
        int n = 0;
        foreach (var marker in vuMarkManager.GetActiveBehaviours())
        {
            var tracker = GetTrackerFromTarget(marker.VuMarkTarget);
            if (tracker == null) continue;
            if (!tracker.isRegistered) continue;

            Quaternion trackerRotation = tracker.knownWorldRotation * Quaternion.Inverse(marker.transform.localRotation);
            Vector3 trackerPosition = tracker.knownWorldPosition + trackerRotation * -marker.transform.localPosition;
            averageCameraRotation = AverageQuaternionStepwise(n, averageCameraRotation, trackerRotation);
            predictedCameraPositions.Add(trackerPosition);
            n++;
        }

        bool hasTracking = n > 0;
        float trackingAccuracy = float.NaN;
        if (hasTracking)
        {
            Vector3 averageCameraPosition = Vector3.zero;
            foreach (Vector3 pos in predictedCameraPositions)
                averageCameraPosition += pos / n;

            transform.localRotation = averageCameraRotation;
            transform.localPosition = averageCameraPosition;

            trackingAccuracy = Mathf.Sqrt(predictedCameraPositions.Select(pos => (pos - averageCameraPosition).sqrMagnitude).Max());
        }
        
        debug.text = "Tracking: " + n
            + ", Trackers: " + string.Join(", ", vuMarkManager.GetActiveBehaviours()
                .Select(marker => GetTrackerFromTarget(marker.VuMarkTarget))
                .Where(tracker => tracker != null)
                .Select(tracker => string.Format("#{0}::{1}", tracker.id, tracker.isRegistered ? "R" : "?")))
            + "\nAccuracy: " + (trackingAccuracy*100).ToString("0") + " cm";

        if (hasTracking) {
            ProcessUnregisteredTrackers();
        }

        if(onlyWithTrackingTarget != null && onlyWithTrackingTarget.activeSelf != hasTracking)
        {
            onlyWithTrackingTarget.SetActive(hasTracking);
        }

        // Debug: Reset with keyboard.
        if(Input.GetKeyDown(KeyCode.R))
        {
            ResetTrackers();
        }
    }

    private void ProcessUnregisteredTrackers()
    {
        // Try to find world positions of unregistered trackers.
        foreach (var marker in vuMarkManager.GetActiveBehaviours())
        {
            var tracker = GetTrackerFromTarget(marker.VuMarkTarget);
            if (tracker == null) continue;
            if (tracker.isRegistered) continue;

            // Debug: Register on mouse click.
            if (Input.GetMouseButton(0))
            {
                Debug.Log("Registering #" + tracker.id + " at " + marker.transform.position);
                tracker.knownWorldRotation = marker.transform.rotation;
                tracker.knownWorldPosition = marker.transform.position;
                tracker.isRegistered = true;
                tracker.visualRepresentation = Instantiate(trackerCenterPrefab, tracker.knownWorldPosition, tracker.knownWorldRotation);
            }
        }
    }

    public void ResetTrackers()
    {
        // Unregister all trackers except the world center.
        foreach (var tracker in trackers)
        {
            if (tracker.id != centerTrackerId)
            {
                tracker.isRegistered = false;
                if (tracker.visualRepresentation != null)
                {
                    Destroy(tracker.visualRepresentation);
                    tracker.visualRepresentation = null;
                }
            }
        }
    }

    public void SetScale(float scaleExponent)
    {
        float s = Mathf.Pow(10, scaleExponent);
        onlyWithTrackingTarget.transform.localScale = s * Vector3.one;
    }

    private Quaternion AverageQuaternionStepwise(int step, Quaternion accumulator, Quaternion newQuaternion)
    {
        if (step == 0) return newQuaternion;
        float weight = 1f / (step + 1f);
        return Quaternion.Slerp(accumulator, newQuaternion, weight);
    }

    private int GetIDFromTarget(VuMarkTarget target)
    {
        return (int)target.InstanceId.NumericValue;
    }

    private TrackerState GetTrackerFromTarget(VuMarkTarget target)
    {
        int id = GetIDFromTarget(target);
        if (id < 1 || id > MAX_TRACKER_ID)
            return null;
        return trackers[id];
    }
}

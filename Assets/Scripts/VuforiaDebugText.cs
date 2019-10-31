using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class VuforiaDebugText : MonoBehaviour, ITrackableEventHandler
{
    public VuMarkBehaviour trackingTarget;
    public Text output;

    void Start() {
        trackingTarget.RegisterTrackableEventHandler(this);
    }

    void Update() {
        output.text = "(unknown)";
    }

    public void OnTrackableStateChanged(TrackableBehaviour.Status prev, TrackableBehaviour.Status curr)
    {
        output.text = curr.ToString();
    }
}

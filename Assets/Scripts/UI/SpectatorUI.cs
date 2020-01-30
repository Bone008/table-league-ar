using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SpectatorUI : MonoBehaviour, IPointerClickHandler
{
    public GameObject[] toggleTargets;
    public TrackingController trackingController;

    private float lastClickTime = 0;
    private bool uiActive = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Note: eventData.clickCount does not work for touches.
        if (eventData.clickTime - lastClickTime < 0.5f)
            ToggleUI();
        lastClickTime = eventData.clickTime;
    }

    private void ToggleUI()
    {
        uiActive = !uiActive;
        foreach (var target in toggleTargets)
            target.SetActive(uiActive);
    }

    public void SetFreezeCamera(bool value)
    {
        trackingController.enabled = value;
    }
}

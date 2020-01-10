﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Script for a button that toggles the visibility of some panel.</summary>
[RequireComponent(typeof(Button))]
public class TogglePanelButton : MonoBehaviour
{
    public Transform toggleTarget;
    public float openDuration;

    private Button button;
    private Sprite buttonBaseSprite;
    private bool targetState;
    private Coroutine transitionCoroutine = null;

    void Start()
    {
        button = GetComponent<Button>();
        buttonBaseSprite = button.image.sprite;
        targetState = toggleTarget.gameObject.activeSelf;

        button.onClick.AddListener(() =>
        {
            targetState = !targetState;
            OnChange();
        });
    }

    public void SetPanelOpen(bool value)
    {
        if (targetState == value)
            return;
        targetState = value;
        OnChange();
    }

    private void OnChange()
    {
        // Note: Cannot use overrideSprite as that is already used by the hover/click effect.
        button.image.sprite = targetState ? button.spriteState.disabledSprite : buttonBaseSprite;

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        toggleTarget.gameObject.SetActive(true);
        transitionCoroutine = this.AnimateScalar(openDuration, toggleTarget.localScale.x, targetState ? 1 : 0, Util.EaseOut01, value =>
        {
            toggleTarget.localScale = value * Vector3.one;
            if(!targetState && value == 0.0f) toggleTarget.gameObject.SetActive(false);
        }, true);
    }
}
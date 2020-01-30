using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleSetup : MonoBehaviour
{
    public float minWidth;
    public float referenceWidth;
    public float referenceLength;
    public Transform previewLineNeg;
    public Transform previewLinePos;
    public GameObject playingAreaPreview;
    public TMPro.TextMeshProUGUI[] widthTexts;
    public TMPro.TextMeshProUGUI[] lengthTexts;

    private float currentHalfWidth;

    void LateUpdate()
    {
        currentHalfWidth = Mathf.Max(minWidth / 2, Mathf.Abs(Camera.main.transform.position.z));
        previewLineNeg.position = currentHalfWidth * Vector3.back;
        previewLinePos.position = currentHalfWidth * Vector3.forward;
    }

    public void ConfirmScale()
    {
        float newScale = currentHalfWidth * 2 / referenceWidth;
        Scale.Instance.SetScale(newScale);

        playingAreaPreview.SetActive(true);
        foreach (var text in widthTexts)
            text.text = Mathf.RoundToInt(referenceWidth * newScale * 100f) + " cm";
        foreach (var text in lengthTexts)
            text.text = Mathf.RoundToInt(referenceLength * newScale * 100f) + " cm";
    }
}

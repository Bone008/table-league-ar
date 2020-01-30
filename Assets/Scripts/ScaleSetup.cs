using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleSetup : MonoBehaviour
{
    public float minWidth;
    public float referenceWidth;
    public Transform previewLineNeg;
    public Transform previewLinePos;
    public GameObject playingAreaPreview;

    private float currentHalfWidth;

    void LateUpdate()
    {
        currentHalfWidth = Mathf.Max(minWidth / 2, Mathf.Abs(Camera.main.transform.position.z));
        previewLineNeg.position = currentHalfWidth * Vector3.back;
        previewLinePos.position = currentHalfWidth * Vector3.forward;
    }

    public void ConfirmScale()
    {
        Scale.Instance.SetScale(currentHalfWidth * 2 / referenceWidth);
        playingAreaPreview.SetActive(true);
    }
}

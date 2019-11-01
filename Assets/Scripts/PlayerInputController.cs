using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    private const string BALL_TAG = "GameBall";

    public float maxInteractionRange = 2f;
    public float maxHitStrength = 30;

    void Update()
    {
        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("touch at " + touch.position);
                OnSceneClick(touch.position);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("click at " + Input.mousePosition);
            OnSceneClick(Input.mousePosition);
        }
    }

    private void OnSceneClick(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxInteractionRange))
        {
            float distFactor = 1 - hit.distance / maxInteractionRange;

            if (hit.collider.gameObject.CompareTag(BALL_TAG))
            {
                Debug.Log("hit a ball at distance " + hit.distance, hit.collider.gameObject);
                Vector3 force = distFactor * maxHitStrength * ray.direction.normalized;
                hit.rigidbody.AddForceAtPosition(force, hit.point, ForceMode.Impulse);
            }
        }
    }
}

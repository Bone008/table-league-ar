using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputController : MonoBehaviour
{
    public float maxInteractionRange = 2f;
    public float maxHitStrength = 30;

    public void ResetBall()
    {
        var ball = GameObject.FindGameObjectWithTag(Constants.BALL_TAG);
        if(ball == null)
        {
            Debug.LogWarning("Ball not found!");
            return;
        }
        ball.transform.position = new Vector3(0, 0.1f, 0);
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    void Update()
    {
        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Debug.Log("touch at " + touch.position);
                OnSceneClick(touch.position);
            }
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
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

            if (hit.collider.gameObject.CompareTag(Constants.BALL_TAG))
            {
                Debug.Log("hit a ball at distance " + hit.distance, hit.collider.gameObject);
                Vector3 force = distFactor * maxHitStrength * (transform.rotation * Vector3.forward);
                hit.rigidbody.AddForceAtPosition(force, hit.point, ForceMode.Impulse);
            }
        }
    }
}

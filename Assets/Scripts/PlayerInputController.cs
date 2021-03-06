﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class PlayerInputController : MonoBehaviour
{
    // Hacky helper to access this value from BallInteractionPreview.
    public static float s_unscaledMaxInteractionRange;

    public PlayerNetController netController => PlayerNetController.LocalInstance;

    [FormerlySerializedAs("maxInteractionRange")]
    public float unscaledMaxInteractionRange;
    [FormerlySerializedAs("minHitStrength")]
    public float unscaledMinHitStrength;
    [FormerlySerializedAs("maxHitStrength")]
    public float unscaledMaxHitStrength;
    public AnimationCurve hitStrengthCurve;

    private float maxInteractionRange => unscaledMaxInteractionRange * Scale.gameScale;
    private float minHitStrength => unscaledMinHitStrength * Scale.gameScale;
    private float maxHitStrength => unscaledMaxHitStrength * Scale.gameScale;

    private GameObject towerPreview = null;
    private int towerChoice = -1;
    
    // Local flag to remember if we need to send CancelInteraction or not.
    private bool isInteracting = false;
    //Local flag to check if we are building anything
    private bool isBuilding = false;

    void Awake()
    {
        Input.simulateMouseWithTouches = false;
        s_unscaledMaxInteractionRange = unscaledMaxInteractionRange;
    }

    void LateUpdate()
    {
        if(netController == null || netController.player == null)
        {
            return;
        }

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit = default(RaycastHit);
        GameObject[] towers = GameObject.FindGameObjectsWithTag(Constants.TOWER_TAG);
        Quaternion previewAngle;

        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            isBuilding = false;

            if (isInteracting)
            {
                netController.CmdCancelInteraction();
                isInteracting = false;
            }
        }

        if (!isBuilding)
        {
            int newTowerChoice;

            if (TowerUIManager.towerChoice == TowerType.None || netController.player.GetInventoryCount(CollectableType.TowerResource) < TowerManager.Instance.getTowerCost(TowerUIManager.towerChoice))
            {
                newTowerChoice = -1;
                StatusUIMananger.LocalInstance.HidePanel();
            }
            else if (Physics.Raycast(ray, out hit, maxInteractionRange, LayerMask.GetMask("Floor"), QueryTriggerInteraction.Collide))
            {
                if (hit.collider.gameObject.CompareTag(Constants.FLOOR_TAG))
                {
                    bool towerFeasible = true;
                    foreach (GameObject t in towers)
                    {
                        if (Vector3.SqrMagnitude(t.transform.position - hit.point) < Constants.scaledTowerDistance * Constants.scaledTowerDistance)
                        {
                            towerFeasible = false;
                            StatusUIMananger.LocalInstance?.TowerTooClose();
                            break;
                        }
                    }
                    if(!netController.player.ownedRectangle.Contains(hit.point))
                    {
                        towerFeasible = false;
                        StatusUIMananger.LocalInstance?.BuildOwnSide();
                    }

                    if (towerFeasible)
                    {
                        newTowerChoice = (int)TowerUIManager.towerChoice;
                    }
                    else
                    {
                        newTowerChoice = (int)TowerType.None;
                    }
                }
                else
                {
                    newTowerChoice = -1;
                }
                
            }
            else
            {
                newTowerChoice = -1;
            }

            

            if(newTowerChoice == - 1)
            {
                Destroy(towerPreview);
                towerPreview = null;
            }
            else
            {
                previewAngle = Quaternion.Euler(0f, Camera.main.transform.eulerAngles.y, 0f);

                if (towerPreview == null || newTowerChoice != towerChoice)
                {
                    Destroy(towerPreview);
                    towerPreview = Instantiate(TowerManager.Instance.getTowerPreview((TowerType)newTowerChoice), hit.point, previewAngle);
                    StatusUIMananger.LocalInstance.HoldToBuild();
                }
                else
                {
                    towerPreview.transform.rotation = previewAngle;
                    towerPreview.transform.position = hit.point;
                }
            }

            towerChoice = newTowerChoice;
        }

        foreach (var touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                //Debug.Log("touch at " + touch.position);
                OnSceneClick(touch.position);
            }
        }
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            //Debug.Log("click at " + Input.mousePosition);
            OnSceneClick(Input.mousePosition);
        }
    }
    

    private void OnSceneClick(Vector2 position)
    {
        Ray ray = Camera.main.ScreenPointToRay(position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxInteractionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            Debug.Log($"CLICK raycast hit: {hit.collider.gameObject.name} tagged {hit.collider.tag}", hit.collider);

            float distanceToOrigin = (hit.transform.position - transform.position).magnitude;
            if (hit.collider.CompareTag(Constants.BALL_TAG) && distanceToOrigin < maxInteractionRange)
            {
                float distFactor = hitStrengthCurve.Evaluate(distanceToOrigin / maxInteractionRange);
                float hitStrength = Mathf.Lerp(minHitStrength, maxHitStrength, distFactor);
                Debug.Log($"hit a ball at d={distanceToOrigin}, str={hitStrength} ({Mathf.RoundToInt((hitStrength /maxHitStrength)* 100)} %)", hit.collider.gameObject);

                Vector3 direction = transform.rotation * Vector3.forward;

                if (direction.y < 0) direction.y = 0;
                direction.Normalize();

                Vector3 force = hitStrength * direction;
                netController.CmdHitBall(hit.collider.gameObject, force);
            }

            else if (hit.collider.CompareTag(Constants.COLLECTABLE_TAG) && netController.player.ownedRectangle.Contains(hit.transform.position))
            {
                isInteracting = true;
                netController.CmdStartCollect(hit.collider.gameObject);
            }

            else if (TowerUIManager.destroyMode)
            {
                GameObject tower = Util.GetGoInParentWithTag(hit.collider.gameObject, Constants.TOWER_TAG);
                if (tower != null && netController.player.ownedRectangle.Contains(tower.transform.position))
                {
                    isInteracting = true;
                    netController.CmdStartDestroyTower(tower);
                }
            }
            else if(towerChoice > 0)
            {
                Vector3 tempPos = towerPreview.transform.position;
                Quaternion tempAngle = towerPreview.transform.rotation;
                isInteracting = true;
                isBuilding = true;
                Destroy(towerPreview);
                netController.CmdStartBuildTower((TowerType)towerChoice, tempPos, tempAngle);
            }
            
        }
    }
}
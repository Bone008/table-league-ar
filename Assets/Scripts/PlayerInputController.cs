using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputController : MonoBehaviour
{
    public float maxInteractionRange = 2f;
    public float minHitStrength;
    public float maxHitStrength;
    
    private int clickedObjectType = 0;

    /// <summery>For resource.</summery>
    public float resourceCollectionTime;
    private float resourceTimer = 0f;
    private int resourcesCreated = 0;
    public int resourceLimit;
    public GameObject resourcePrefab;
    private GameObject activeResource;
    public float resourceCreationProbabilty;

    /// <summery>For tower preview.</summery>
    public GameObject[] prefabPreviewTowers;
    private bool towerFeasible;
    public int towerCost;

    /// <summery>For tower.</summery>
    public float towerDistance;
    public GameObject[] prefabTowers;
    public float towerCreationTime;
    private float towerTimer = 0f;
    private Vector3 newTowerPos;
    private Quaternion newTowerAngle;
    private GameObject towerPreview;

    void Awake()
    {
        Input.simulateMouseWithTouches = false;
    }

    void Update()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        GameObject[] towers = GameObject.FindGameObjectsWithTag(Constants.TOWER_TAG);
        Quaternion previewAngle;

        GameObject currentResource;
        float prob = Random.Range(0, 1.0f);

        towerFeasible = false;

        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            towerTimer = 0f;

            resourceTimer = 0f;
            if (clickedObjectType == 2)
            {
                activeResource.GetComponentInChildren<ParticleSystem>().Stop();
            }

            clickedObjectType = 0;
            if (towerPreview)
            {
                towerPreview.GetComponentInChildren<ParticleSystem>().Stop();
            }

            if (TowerManager.GetTowerChoice() == -1 || GameManager.Instance.GetResource() < towerCost)
            {
                Destroy(towerPreview);
                towerPreview = null;
            }

            if (Physics.Raycast(ray, out hit, maxInteractionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            {
                float distanceToOrigin = (hit.transform.position - transform.position).magnitude;
                if (hit.collider.gameObject.CompareTag(Constants.FLOOR_TAG) /*&& hit.point.z > 0*/ && clickedObjectType == 0 && towerTimer == 0 && TowerManager.GetTowerChoice() != -1 && GameManager.Instance.GetResource() >= towerCost && distanceToOrigin < maxInteractionRange)
                {
                    towerFeasible = true;
                    foreach (GameObject t in towers)
                    {
                        if (Vector3.Distance(t.transform.position, hit.point) < towerDistance && t != towerPreview)
                        {
                            towerFeasible = false;
                            break;
                        }
                    }
                    if (towerFeasible)
                    {
                        previewAngle = Quaternion.identity;
                        previewAngle = Quaternion.Euler(previewAngle.x, Camera.main.transform.eulerAngles.y, previewAngle.z);

                        if (towerPreview == null)
                        {
                            Debug.Log("Create");
                            towerPreview = Instantiate(prefabPreviewTowers[TowerManager.GetTowerChoice()], hit.point, previewAngle);
                            towerPreview.GetComponentInChildren<ParticleSystem>().Stop();
                        }
                        else
                        {
                            towerPreview.transform.rotation = previewAngle;
                            towerPreview.transform.position = hit.point;
                        }
                    }
                    else
                    {
                        Destroy(towerPreview);
                        towerPreview = null;
                    }
                }
                else
                {
                    Destroy(towerPreview);
                    towerPreview = null;
                }
            }
        }
        
        if (prob < resourceCreationProbabilty && resourcesCreated < resourceLimit)
        {
            currentResource = Instantiate(resourcePrefab, new Vector3(Random.Range(-1.0f, 1.0f), 0.015f, Random.Range(-1.4f, 1.4f)), Quaternion.identity);
            currentResource.GetComponentInChildren<ParticleSystem>().Stop();
            resourcesCreated++;
        }
        
        if(clickedObjectType == 2 && resourceTimer != 0 && Time.time - resourceTimer >= towerCreationTime)
        {
            Destroy(activeResource);
            resourceTimer = 0f;
            clickedObjectType = 0;
            GameManager.Instance.CollectResource();
            resourcesCreated--;
        }

        if(clickedObjectType == 1 && towerTimer != 0 && Time.time - towerTimer >= towerCreationTime)
        {
            newTowerPos = towerPreview.transform.position;
            newTowerAngle = towerPreview.transform.rotation;
            Destroy(towerPreview);
            towerPreview = null;
            Instantiate(prefabTowers[TowerManager.GetTowerChoice()], newTowerPos, newTowerAngle);
            towerTimer = 0f;
            clickedObjectType = 0;
            GameManager.Instance.UseResource(towerCost);
        }

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

        if (Physics.Raycast(ray, out hit, maxInteractionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            float distanceToOrigin = (hit.transform.position - transform.position).magnitude;
            if (hit.collider.gameObject.CompareTag(Constants.BALL_TAG) && distanceToOrigin < maxInteractionRange)
            {
                float distFactor = 1 - distanceToOrigin / maxInteractionRange;
                float hitStrength = Mathf.Lerp(minHitStrength, maxHitStrength, distFactor);
                Debug.Log("hit a ball at distance " + distanceToOrigin + ", strength " + hitStrength, hit.collider.gameObject);

                Vector3 direction = transform.rotation * Vector3.forward;
                if (direction.y < 0) direction.y = 0;
                direction.Normalize();

                Vector3 force = hitStrength * direction;
                hit.rigidbody.velocity = Vector3.zero;
                hit.rigidbody.AddForce(force, ForceMode.Impulse);
            }
            
            if (hit.collider.gameObject.CompareTag(Constants.RESOURCE_TAG) && clickedObjectType == 0 && resourceTimer == 0)
            {
                activeResource = hit.collider.gameObject;
                hit.collider.gameObject.GetComponentInChildren<ParticleSystem>().Play();
                resourceTimer = Time.time;
                clickedObjectType = 2;
            }
            
            if (hit.collider.gameObject.CompareTag(Constants.FLOOR_TAG) && clickedObjectType == 0)
            {
                Debug.Log("Start Build");
                towerTimer = Time.time;
                towerPreview.GetComponentInChildren<ParticleSystem>().Play();
                //newTowerPos = hit.point;
                //newTowerAngle = Quaternion.identity;
                //newTowerAngle = Quaternion.Euler(newTowerAngle.x, Camera.main.transform.eulerAngles.y, newTowerAngle.z);
                clickedObjectType = 1;                
            }
        }
    }
}
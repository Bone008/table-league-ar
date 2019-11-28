using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInputController : MonoBehaviour
{
    public float maxInteractionRange = 2f;
    public float maxHitStrength = 30;
    public float towerDistance;
    public GameObject[] prefabTowers;
    public GameObject[] prefabPreviewTowers;
    public float towerCreationTime = 2f;
    private float towerTimer = 0f;
    private Vector3 newTowerPos;
    private Quaternion newTowerAngle;
    private GameObject newTower;
    private int clickedObjectType = 0;
    public int towerCost;
    public float resourceCollectionTime;
    private float resourceTimer = 0f;
    private int resourceCreated = 0;
    public int resourceLimit;
    public GameObject resourcePrefab;
    private GameObject activeResource;
    public float resourceCreationProbabilty;

    void Awake()
    {
        Input.simulateMouseWithTouches = false;
    }

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
        GameObject currentResource;
        float prob = Random.Range(0, 1.0f);
        Debug.Log("Prob value: " + prob);

        if (prob < resourceCreationProbabilty && resourceCreated < resourceLimit)
        {
            currentResource = Instantiate(resourcePrefab, new Vector3(Random.Range(-1.0f, 1.0f), 0.015f, Random.Range(-1.4f, 1.4f)), Quaternion.identity);
            Debug.Log("Resource Created");
            currentResource.GetComponentInChildren<ParticleSystem>().Stop();
            resourceCreated++;
        }

        if (Input.touchCount == 0 && !Input.GetMouseButton(0))
        {
            towerTimer = 0f;
            Destroy(newTower);

            resourceTimer = 0f;
            if(clickedObjectType == 2)
            {
                activeResource.GetComponentInChildren<ParticleSystem>().Stop();
            }

            clickedObjectType = 0;
        }
        if(clickedObjectType == 2 && resourceTimer != 0 && Time.time - resourceTimer >= towerCreationTime)
        {
            Destroy(activeResource);
            resourceTimer = 0f;
            clickedObjectType = 0;
            GameManager.Instance.CollectResource();
            resourceCreated--;
        }
        if(clickedObjectType == 1 && towerTimer != 0 && Time.time - towerTimer >= towerCreationTime)
        {
            Destroy(newTower);
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
        GameObject[] towers = GameObject.FindGameObjectsWithTag(Constants.TOWER_TAG);
        GameObject[] resources = GameObject.FindGameObjectsWithTag(Constants.RESOURCE_TAG);

        if (Physics.Raycast(ray, out hit, maxInteractionRange, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            float distFactor = 1 - hit.distance / maxInteractionRange;
            if (hit.collider.gameObject.CompareTag(Constants.BALL_TAG))
            {
                Debug.Log("hit a ball at distance " + hit.distance, hit.collider.gameObject);
                Vector3 direction = transform.rotation * Vector3.forward;
                if (direction.y < 0) direction.y = 0;
                Vector3 force = distFactor * maxHitStrength * direction.normalized;
                hit.rigidbody.AddForceAtPosition(force, hit.point, ForceMode.Impulse);
            }

            if (hit.collider.gameObject.CompareTag(Constants.RESOURCE_TAG) && clickedObjectType == 0 && resourceTimer == 0)
            {
                //Start Here
                activeResource = hit.collider.gameObject;
                hit.collider.gameObject.GetComponentInChildren<ParticleSystem>().Play();
                resourceTimer = Time.time;
                clickedObjectType = 2;
            }

            if (hit.collider.gameObject.CompareTag(Constants.FLOOR_TAG) /*&& hit.point.z > 0*/ && clickedObjectType == 0 && towerTimer == 0 && TowerManager.GetTowerChoice() != -1 && GameManager.Instance.GetResource() >= towerCost)
            {
                bool create = true;

                foreach (GameObject t in towers)
                {

                    if (Vector3.Distance(t.transform.position, hit.point) < towerDistance)
                    {
                        create = false;
                        break;
                    }
                }

                if (create)
                {
                    towerTimer = Time.time;
                    newTowerPos = hit.point;
                    newTowerAngle = Quaternion.identity;
                    newTowerAngle = Quaternion.Euler(newTowerAngle.x, Camera.main.transform.eulerAngles.y, newTowerAngle.z);
                    newTower = Instantiate(prefabPreviewTowers[TowerManager.GetTowerChoice()], newTowerPos, newTowerAngle);
                    clickedObjectType = 1;
                }
            }            
        }
    }
}
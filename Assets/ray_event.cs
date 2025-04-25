using UnityEngine;
using UnityEngine.Events;

public class ray_event : MonoBehaviour
{
    [Header("Raycast Settings")]
    public float maxDistance = 10f;
    public LayerMask layerMask = Physics.DefaultRaycastLayers;
    
    [Header("Detection Settings")]
    public bool useTagDetection = false;
    public string targetTag = "Target";
    public GameObject targetObject;
    
    [Header("Status")]
    public bool isDetecting = false;
    
    [Header("Events")]
    public UnityEvent onObjectDetected;
    public UnityEvent onObjectLost;
    
    public Camera cam;
    private bool isLookingAtTarget = false;
    private GameObject lastHitObject = null;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("No camera found! Please attach this script to a camera or ensure there is a main camera in the scene.");
            }
        }
        
        if (onObjectDetected == null)
            onObjectDetected = new UnityEvent();
            
        if (onObjectLost == null)
            onObjectLost = new UnityEvent();
    }

    // Update is called once per frame
    void Update()
    {
        CastRay();
    }
    
    void CastRay()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        bool hitDetected = Physics.Raycast(ray, out hit, maxDistance, layerMask);
        
        if (hitDetected)
        {
            bool isTargetObject = false;
            
            if (useTagDetection)
            {
                isTargetObject = hit.collider.CompareTag(targetTag);
            }
            else
            {
                isTargetObject = (targetObject != null && hit.collider.gameObject == targetObject);
            }
            
            if (isTargetObject)
            {
                if (!isLookingAtTarget || lastHitObject != hit.collider.gameObject)
                {
                    isLookingAtTarget = true;
                    isDetecting = true;
                    lastHitObject = hit.collider.gameObject;
                    onObjectDetected.Invoke();
                    Debug.Log("Looking at target: " + hit.collider.gameObject.name);
                }
            }
            else
            {
                HandleLostTarget();
            }
        }
        else
        {
            HandleLostTarget();
        }
    }
    
    private void HandleLostTarget()
    {
        if (isLookingAtTarget)
        {
            isLookingAtTarget = false;
            isDetecting = false;
            lastHitObject = null;
            onObjectLost.Invoke();
            Debug.Log("Lost target");
        }
    }
    
    public bool IsLookingAtTarget()
    {
        return isLookingAtTarget;
    }
    
    public GameObject GetCurrentTarget()
    {
        return lastHitObject;
    }
}

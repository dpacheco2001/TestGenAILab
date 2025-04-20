using UnityEngine;

public class change_texture : MonoBehaviour
{
    public Material newMaterial;  // The material to apply when collision occurs
    public GameObject targetObject;  // The specific object to detect collision with
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Make sure this object has a collider with "Is Trigger" enabled
        Collider collider = GetComponent<Collider>();
        if (collider != null && !collider.isTrigger)
        {
            Debug.LogWarning("The collider on this object should have 'Is Trigger' enabled");
        }
        
        // Check if target object is assigned
        if (targetObject == null)
        {
            Debug.LogWarning("Target object is not assigned! Please assign the 'proveta' object in the inspector.");
        }
    }

    // Called when this trigger collider enters another collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the other object is the target object or has the "proveta" tag
        if (targetObject != null && other.gameObject == targetObject || other.CompareTag("proveta"))
        {
            // Check if the other object has a renderer
            Renderer renderer = other.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Change the material of the object that was hit
                renderer.material = newMaterial;
                Debug.Log("Changed material of " + other.gameObject.name);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class trigger_events : MonoBehaviour
{
    [System.Serializable]
    public enum TriggerType
    {
        Enter,
        Exit,
        Stay
    }

    [System.Serializable]
    public enum TriggerSourceType
    {
        ThisObject,
        CustomTrigger
    }

    [System.Serializable]
    public class TriggerEvent
    {
        public string eventName;
        public bool wasTriggered = false;
        public TriggerType triggerType = TriggerType.Enter;
        public FilterType filterType = FilterType.All;
        
        [Header("Filter Options")]
        public string tagFilter = "";
        public GameObject specificObject;
        
        [Header("Trigger Source")]
        public TriggerSourceType triggerSource = TriggerSourceType.ThisObject;
        public GameObject customTriggerObject;
        
        [Header("Event")]
        public UnityEvent<GameObject> onTriggerEvent;

        // Reset trigger state
        public void Reset()
        {
            wasTriggered = false;
        }
    }

    [System.Serializable]
    public enum FilterType
    {
        All,
        ByTag,
        BySpecificObject
    }

    [Header("Trigger Configuration")]
    public List<TriggerEvent> triggerEvents = new List<TriggerEvent>();

    [Header("Debug Options")]
    public bool showDebugMessages = false;

    private Dictionary<GameObject, List<TriggerEvent>> customTriggers = new Dictionary<GameObject, List<TriggerEvent>>();

    private void Awake()
    {
        // Set up dictionary of custom triggers
        foreach (TriggerEvent triggerEvent in triggerEvents)
        {
            if (triggerEvent.triggerSource == TriggerSourceType.CustomTrigger && 
                triggerEvent.customTriggerObject != null)
            {
                if (!customTriggers.ContainsKey(triggerEvent.customTriggerObject))
                {
                    customTriggers[triggerEvent.customTriggerObject] = new List<TriggerEvent>();
                    
                    // Add trigger component if missing
                    if (!triggerEvent.customTriggerObject.GetComponent<CustomTriggerProxy>())
                    {
                        CustomTriggerProxy proxy = triggerEvent.customTriggerObject.AddComponent<CustomTriggerProxy>();
                        proxy.parentTrigger = this;
                    }
                }
                
                customTriggers[triggerEvent.customTriggerObject].Add(triggerEvent);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessTrigger(gameObject, other.gameObject, TriggerType.Enter);
    }

    private void OnTriggerExit(Collider other)
    {
        ProcessTrigger(gameObject, other.gameObject, TriggerType.Exit);
    }

    private void OnTriggerStay(Collider other)
    {
        ProcessTrigger(gameObject, other.gameObject, TriggerType.Stay);
    }

    public void HandleCustomTrigger(GameObject triggerObj, GameObject otherObj, TriggerType type)
    {
        if (customTriggers.ContainsKey(triggerObj))
        {
            ProcessTrigger(triggerObj, otherObj, type);
        }
    }

    private void ProcessTrigger(GameObject triggerObj, GameObject otherObj, TriggerType type)
    {
        List<TriggerEvent> eventsToProcess = triggerEvents;
        
        if (triggerObj != gameObject && customTriggers.ContainsKey(triggerObj))
        {
            eventsToProcess = customTriggers[triggerObj];
        }
        
        foreach (TriggerEvent triggerEvent in eventsToProcess)
        {
            if (triggerEvent.triggerType != type)
                continue;
                
            // Skip if this event is for a different trigger source
            if ((triggerEvent.triggerSource == TriggerSourceType.ThisObject && triggerObj != gameObject) ||
                (triggerEvent.triggerSource == TriggerSourceType.CustomTrigger && 
                 triggerEvent.customTriggerObject != triggerObj))
                continue;
            
            GameObject targetObject = otherObj;
            bool shouldTrigger = false;

            switch (triggerEvent.filterType)
            {
                case FilterType.All:
                    shouldTrigger = true;
                    break;
                case FilterType.ByTag:
                    shouldTrigger = !string.IsNullOrEmpty(triggerEvent.tagFilter) && 
                                    targetObject.CompareTag(triggerEvent.tagFilter);
                    break;
                case FilterType.BySpecificObject:
                    shouldTrigger = triggerEvent.specificObject != null && 
                                    targetObject == triggerEvent.specificObject;
                    break;
            }

            if (shouldTrigger)
            {
                // Set the flag to indicate this event was triggered
                triggerEvent.wasTriggered = true;
                
                if (showDebugMessages)
                {
                    Debug.Log($"Trigger {triggerEvent.eventName}: {type} with {targetObject.name} using trigger {triggerObj.name}");
                }
                
                triggerEvent.onTriggerEvent?.Invoke(targetObject);
            }
        }
    }

    // Reset all trigger events
    public void ResetAllTriggers()
    {
        foreach (TriggerEvent triggerEvent in triggerEvents)
        {
            triggerEvent.Reset();
        }
    }

    // Check if a specific trigger event was activated
    public bool WasTriggerActivated(string eventName)
    {
        foreach (TriggerEvent triggerEvent in triggerEvents)
        {
            if (triggerEvent.eventName == eventName)
            {
                return triggerEvent.wasTriggered;
            }
        }
        return false;
    }

    // Helper component to route trigger events from custom trigger objects
    public class CustomTriggerProxy : MonoBehaviour
    {
        public trigger_events parentTrigger;
        
        private void OnTriggerEnter(Collider other)
        {
            if (parentTrigger != null)
                parentTrigger.HandleCustomTrigger(gameObject, other.gameObject, TriggerType.Enter);
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (parentTrigger != null)
                parentTrigger.HandleCustomTrigger(gameObject, other.gameObject, TriggerType.Exit);
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (parentTrigger != null)
                parentTrigger.HandleCustomTrigger(gameObject, other.gameObject, TriggerType.Stay);
        }
    }

    // Helper function to add a new trigger event at runtime
    public void AddNewTriggerEvent(string name, TriggerType type, FilterType filter)
    {
        TriggerEvent newEvent = new TriggerEvent
        {
            eventName = name,
            triggerType = type,
            filterType = filter,
            onTriggerEvent = new UnityEvent<GameObject>()
        };
        
        triggerEvents.Add(newEvent);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}


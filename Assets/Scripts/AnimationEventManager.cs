using UnityEngine;
using System.Collections.Generic;
using BNG;
using UnityEngine.Events;

public class AnimationEventManager : MonoBehaviour
{
    [System.Serializable]
    public class ObjectState
    {
        [Header("Object Settings")]
        public string objectName;
        public GameObject targetObject;
        
        [Header("Dial Settings")]
        public HingeHelper dial;
        [Tooltip("El cambio mínimo de ángulo necesario para desactivar el objeto")]
        public float angleChangeThreshold = 5f;
        public float lastAngle;
        
        [Header("Button Settings")]
        public Button button;
        public bool deactivateOnButtonPress;
    }

    public List<ObjectState> objects = new List<ObjectState>();

    private void Start()
    {
        // Configurar eventos para cada objeto
        foreach (var obj in objects)
        {
            if (obj.targetObject == null)
            {
                Debug.LogWarning($"Objeto {obj.objectName} no tiene GameObject asignado!");
                continue;
            }

            // Asegurarse que el objeto empiece activado
            obj.targetObject.SetActive(true);

            // Configurar eventos del dial si existe
            if (obj.dial != null)
            {
                // Inicializar el último ángulo
                obj.lastAngle = obj.dial.transform.localEulerAngles.y;
                
                // Configurar el evento de cambio de ángulo
                if (obj.dial.onHingeChange == null)
                {
                    obj.dial.onHingeChange = new FloatEvent();
                }
                obj.dial.onHingeChange.AddListener((angle) => OnDialAngleChanged(obj.objectName, angle));
                
                Debug.Log($"Configurado dial para objeto: {obj.objectName} - Se desactivará con cambio de ángulo mayor a: {obj.angleChangeThreshold}");
            }

            // Configurar eventos del botón si existe
            if (obj.button != null && obj.deactivateOnButtonPress)
            {
                // Asegurarse de que el botón tenga el evento configurado
                if (obj.button.onButtonDown == null)
                {
                    obj.button.onButtonDown = new UnityEvent();
                }
                obj.button.onButtonDown.AddListener(() => OnButtonPressed(obj.objectName));
                Debug.Log($"Configurado botón para objeto: {obj.objectName}");
            }
        }
    }

    public void ActivateObject(string objectName)
    {
        var obj = objects.Find(o => o.objectName == objectName);
        if (obj != null && obj.targetObject != null)
        {
            obj.targetObject.SetActive(true);
            if (obj.dial != null)
            {
                obj.lastAngle = obj.dial.transform.localEulerAngles.y;
            }
            Debug.Log($"Objeto activado: {objectName}");
        }
    }

    public void DeactivateObject(string objectName)
    {
        var obj = objects.Find(o => o.objectName == objectName);
        if (obj != null && obj.targetObject != null)
        {
            obj.targetObject.SetActive(false);
            Debug.Log($"Objeto desactivado: {objectName}");
        }
    }

    public void DeactivatebyGameObject(GameObject gameObject)
    {
        var obj = objects.Find(o => o.targetObject == gameObject);
        if (obj != null && obj.targetObject != null)
        {
            obj.targetObject.SetActive(false);
            Debug.Log($"Objeto desactivado: {gameObject.name}");
        }
    }

    private void OnDialAngleChanged(string objectName, float currentAngle)
    {
        var obj = objects.Find(o => o.objectName == objectName);
        if (obj != null && obj.targetObject.activeSelf)
        {
            // Calcular el cambio de ángulo (maneja tanto positivos como negativos)
            float angleChange = Mathf.Abs(currentAngle - obj.lastAngle);
            
            // Si el cambio es mayor al umbral, desactivar el objeto
            if (angleChange >= obj.angleChangeThreshold)
            {
                Debug.Log($"Cambio de ángulo detectado: {angleChange} >= {obj.angleChangeThreshold}");
                DeactivateObject(objectName);
            }
            
            // Actualizar el último ángulo
            obj.lastAngle = currentAngle;
        }
    }

    private void OnButtonPressed(string objectName)
    {
        var obj = objects.Find(o => o.objectName == objectName);
        if (obj != null && obj.deactivateOnButtonPress)
        {
            Debug.Log($"Botón presionado para objeto: {objectName}");
            DeactivateObject(objectName);
        }
    }

    // Método para verificar si los componentes están configurados correctamente
    private void OnValidate()
    {
        foreach (var obj in objects)
        {
            if (obj.dial != null)
            {
                if (obj.dial.onHingeChange == null)
                {
                    Debug.LogWarning($"El dial para {obj.objectName} no tiene el evento onHingeChange configurado!");
                }
            }

            if (obj.button != null && obj.deactivateOnButtonPress)
            {
                if (obj.button.onButtonDown == null)
                {
                    Debug.LogWarning($"El botón para {obj.objectName} no tiene el evento onButtonDown configurado!");
                }
            }
        }
    }
} 
using UnityEngine;
using BNG;

public class LinearMover : MonoBehaviour
{
    public HingeHelper hingeHelper;         // Reference to HingeHelper component
    public float maxDisplacement = 1f;      // Maximum displacement
    public int steps = 10;                  // Number of movement steps
    
    [Tooltip("Choose which axis to move along")]
    public enum MoveAxis { X, Y, Z }
    public MoveAxis moveAxis = MoveAxis.X;  // Selectable axis

    [Tooltip("Factor to scale down the movement (0.01 = centimeters)")]
    public float movementScale = 0.01f;     // Factor de escala para el movimiento

    [Header("Movement Settings")]
    public bool invertDirection = true;     // Para controlar la dirección del movimiento
    public bool useHingeLimits = true;      // Para decidir si usar los límites del HingeHelper

    [Header("Hinge Control")]
    [Tooltip("HingeHelper to disable while this mover is active")]
    public HingeHelper hingeToDisable;      // Referencia al HingeHelper que queremos desactivar

    private Vector3 initialLocalPosition;    // Initial local position
    private Vector3 currentLocalPosition;    // Posición local actual
    private float lastNormalizedAngle = 0f; // Último ángulo normalizado
    private bool isFirstUpdate = true;      // Flag para primera actualización
    private float stepSize;                 // Size of each step
    private bool isInitialized = false;
    private bool wasHingeEnabled = false;   // Para guardar el estado original del HingeHelper
    private GrabbableUnityEvents grabbableEvents; // Referencia a los eventos del objeto agarrable

    void Start()
    {
        Debug.Log($"[LinearMover] Start en {gameObject.name}");
        
        // Guardar posición local inicial
        initialLocalPosition = transform.localPosition;
        currentLocalPosition = initialLocalPosition;

        stepSize = (maxDisplacement * movementScale) / steps;

        if (hingeHelper != null)
        {
            hingeHelper.onHingeChange.AddListener(OnAngleChanged);
            isInitialized = true;
        }

        // Obtener y configurar GrabbableUnityEvents
        grabbableEvents = GetComponent<GrabbableUnityEvents>();
        if (grabbableEvents != null)
        {
            Debug.Log($"[LinearMover] GrabbableUnityEvents encontrado en {gameObject.name}");
            
            // Suscribirse a los eventos
            grabbableEvents.onGrab.AddListener(DisableHingeOnGrab);
            grabbableEvents.onRelease.AddListener(EnableHingeOnRelease);
            
            Debug.Log($"[LinearMover] Eventos suscritos correctamente");
        }
        else
        {
            //Debug.LogError($"[LinearMover] ¡No se encontró GrabbableUnityEvents en {gameObject.name}!");
        }

        // Guardar el estado inicial del HingeHelper a desactivar
        if (hingeToDisable != null)
        {
            wasHingeEnabled = hingeToDisable.enabled;
            Debug.Log($"[LinearMover] Estado inicial de hingeToDisable: {wasHingeEnabled}");
        }
        else
        {
           // Debug.LogError($"[LinearMover] ¡hingeToDisable no está asignado en {gameObject.name}!");
        }
    }

    void OnEnable()
    {
        Debug.Log($"[LinearMover] OnEnable en {gameObject.name}");
        // Asegurarse de que los eventos estén suscritos
        if (grabbableEvents != null)
        {
            grabbableEvents.onGrab.AddListener(DisableHingeOnGrab);
            grabbableEvents.onRelease.AddListener(EnableHingeOnRelease);
        }
    }

    void OnDisable()
    {
        Debug.Log($"[LinearMover] OnDisable en {gameObject.name}");
        // Limpiar eventos
        if (grabbableEvents != null)
        {
            grabbableEvents.onGrab.RemoveListener(DisableHingeOnGrab);
            grabbableEvents.onRelease.RemoveListener(EnableHingeOnRelease);
        }
    }

    public void pruebaOnGrab()
    {
        Debug.Log($"[LinearMover] prueba llamada en {gameObject.name}");
    }

    public void pruebaOnRelease()
    {
        Debug.Log($"[LinearMover] pruebaOnRelease llamada en {gameObject.name}");
    }

    public void DisableHingeOnGrab(Grabber grabber)
    {
        Debug.Log($"[LinearMover] DisableHingeOnGrab llamado en {gameObject.name} por el grabber {grabber.name}");
        if (hingeToDisable != null)
        {
            Debug.Log($"[LinearMover] Desactivando hingeToDisable. Estado anterior: {hingeToDisable.enabled}");
            hingeToDisable.enabled = false;
        }
        else
        {
            Debug.LogError($"[LinearMover] ¡hingeToDisable es null en DisableHingeOnGrab!");
        }
    }

    // Método público para llamar desde GrabbableUnityEvents - OnRelease
    public void EnableHingeOnRelease()
    {
        Debug.Log($"[LinearMover] EnableHingeOnRelease llamado en {gameObject.name}");
        if (hingeToDisable != null)
        {
            Debug.Log($"[LinearMover] Reactivando hingeToDisable a su estado original: {wasHingeEnabled}");
            hingeToDisable.enabled = wasHingeEnabled;
        }
        else
        {
            Debug.LogError($"[LinearMover] ¡hingeToDisable es null en EnableHingeOnRelease!");
        }
    }

    void OnAngleChanged(float angle)
    {
        if (!isInitialized) return;

        // Calcular el ángulo normalizado
        float normalizedAngle;
        if (useHingeLimits && hingeHelper.useLimits)
        {
            float totalRange = hingeHelper.maxAngle - hingeHelper.minAngle;
            normalizedAngle = (angle - hingeHelper.minAngle) / totalRange;
        }
        else
        {
            normalizedAngle = angle / 360f;
        }

        // Invertir si es necesario
        if (invertDirection)
        {
            normalizedAngle = 1f - normalizedAngle;
        }

        // En la primera actualización, solo guardar el ángulo
        if (isFirstUpdate)
        {
            lastNormalizedAngle = normalizedAngle;
            isFirstUpdate = false;
            return;
        }

        // Calcular el cambio en el movimiento
        float deltaMovement = (normalizedAngle - lastNormalizedAngle) * maxDisplacement * movementScale;
        
        // Actualizar la posición local actual
        Vector3 movement = Vector3.zero;
        switch (moveAxis)
        {
            case MoveAxis.X:
                movement = new Vector3(deltaMovement, 0, 0);
                break;
            case MoveAxis.Y:
                movement = new Vector3(0, deltaMovement, 0);
                break;
            case MoveAxis.Z:
                movement = new Vector3(0, 0, deltaMovement);
                break;
        }

        // Actualizar posición local
        currentLocalPosition += movement;
        transform.localPosition = currentLocalPosition;
        lastNormalizedAngle = normalizedAngle;
    }

    void OnDestroy()
    {
        Debug.Log($"[LinearMover] OnDestroy llamado en {gameObject.name}");
        
        if (hingeHelper != null)
        {
            hingeHelper.onHingeChange.RemoveListener(OnAngleChanged);
        }

        // Desuscribirse de los eventos
        if (grabbableEvents != null)
        {
            grabbableEvents.onGrab.RemoveListener(DisableHingeOnGrab);
            grabbableEvents.onRelease.RemoveListener(EnableHingeOnRelease);
        }

        // Restaurar el estado original del HingeHelper
        if (hingeToDisable != null)
        {
            Debug.Log($"[LinearMover] Restaurando estado final de hingeToDisable a: {wasHingeEnabled}");
            hingeToDisable.enabled = wasHingeEnabled;
        }
    }

    // Método que se conectará en el Inspector
    public void OnGrabHandler()
    {
        Debug.Log($"[LinearMover] OnGrabHandler llamado en {gameObject.name}");
        if (hingeToDisable != null)
        {
            Debug.Log($"[LinearMover] Desactivando hingeToDisable. Estado anterior: {hingeToDisable.enabled}");
            hingeToDisable.enabled = false;
        }
    }

    // Método que se conectará en el Inspector
    public void OnReleaseHandler()
    {
        Debug.Log($"[LinearMover] OnReleaseHandler llamado en {gameObject.name}");
        if (hingeToDisable != null)
        {
            Debug.Log($"[LinearMover] Reactivando hingeToDisable a su estado original: {wasHingeEnabled}");
            hingeToDisable.enabled = wasHingeEnabled;
        }
    }
}

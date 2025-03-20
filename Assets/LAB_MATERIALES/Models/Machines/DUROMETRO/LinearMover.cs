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

    private Vector3 initialPosition;        // Initial position
    private Vector3 currentPosition;        // Posición actual
    private float lastNormalizedAngle = 0f; // Último ángulo normalizado
    private bool isFirstUpdate = true;      // Flag para primera actualización
    private float stepSize;                 // Size of each step
    private bool isInitialized = false;
    private bool wasHingeEnabled = false;   // Para guardar el estado original del HingeHelper

    void Start()
    {
        // Guardar posición inicial
        initialPosition = transform.position;
        currentPosition = initialPosition;


        stepSize = (maxDisplacement * movementScale) / steps;

        if (hingeHelper != null)
        {
            hingeHelper.onHingeChange.AddListener(OnAngleChanged);
            isInitialized = true;
        }

        // Guardar el estado inicial del HingeHelper a desactivar
        if (hingeToDisable != null)
        {
            wasHingeEnabled = hingeToDisable.enabled;
        }
    }

    void OnAngleChanged(float angle)
    {
        if (!isInitialized) return;

        // Desactivar el otro HingeHelper cuando empezamos a mover
        if (hingeToDisable != null && hingeToDisable.enabled)
        {
            hingeToDisable.enabled = false;
        }

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
        
        // Actualizar la posición actual
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

        // Actualizar posición
        currentPosition += movement;
        transform.position = currentPosition;
        lastNormalizedAngle = normalizedAngle;
    }

    void OnDestroy()
    {
        if (hingeHelper != null)
        {
            hingeHelper.onHingeChange.RemoveListener(OnAngleChanged);
        }

        // Restaurar el estado original del HingeHelper
        if (hingeToDisable != null)
        {
            hingeToDisable.enabled = wasHingeEnabled;
        }
    }
}

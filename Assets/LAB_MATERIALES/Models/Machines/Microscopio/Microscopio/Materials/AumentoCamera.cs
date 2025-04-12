using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class AumentoCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera cam;
    public Camera camL;
    public Camera camR;
    
    [Header("Configuración de Aumento")]
    [Tooltip("Grados de rotación necesarios para cambiar el aumento")]
    public float gradosPorAumento = 45f;
    
    [Tooltip("Campo de visión mínimo de la cámara")]
    public float minFOV = 6f;
    
    [Tooltip("Campo de visión máximo de la cámara")]
    public float maxFOV = 37f;

    [Header("Rangos de Aumento")]
    [Tooltip("Margen de error permitido en grados")]
    public float margenError = 5f;

    [Header("Referencias")]
    [Tooltip("Referencia al HingeHelper que controla la rotación")]
    public HingeHelper hingeHelper;

    void Start()
    {
        // Si tenemos un HingeHelper asignado, nos suscribimos a sus eventos
        if (hingeHelper != null)
        {
            hingeHelper.onHingeChange.AddListener(UpdateFOV);
        }
    }

    void OnDestroy()
    {
        // Nos aseguramos de desuscribirnos al destruir el objeto
        if (hingeHelper != null)
        {
            hingeHelper.onHingeChange.RemoveListener(UpdateFOV);
        }
    }

    private void UpdateFOV(float angle)
    {
        // Normalizamos el ángulo a un valor positivo
        angle = Mathf.Abs(angle);

        // Definimos los rangos para cada aumento
        if (IsInRange(angle, 0, margenError))
        {
            // Posición inicial - menor aumento
            SetFOV(maxFOV);
        }
        else if (IsInRange(angle, 45, margenError))
        {
            // Primer aumento
            SetFOV(Mathf.Lerp(maxFOV, minFOV, 0.33f));
        }
        else if (IsInRange(angle, 90, margenError))
        {
            // Segundo aumento
            SetFOV(Mathf.Lerp(maxFOV, minFOV, 0.66f));
        }
        else if (IsInRange(angle, 135, margenError))
        {
            // Máximo aumento
            SetFOV(minFOV);
        }
    }

    private bool IsInRange(float angle, float target, float margin)
    {
        return angle >= (target - margin) && angle <= (target + margin);
    }

    private void SetFOV(float fov)
    {
        cam.fieldOfView = fov;
        camL.fieldOfView = fov;
        camR.fieldOfView = fov;
    }
}

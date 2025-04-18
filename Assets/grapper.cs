using UnityEngine;

public class grapper : MonoBehaviour
{
    public ParticleSystem waterParticles;
    public Transform pourPoint; // Punto desde donde se vierte (la punta del objeto)
    private bool isPouring = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (waterParticles == null)
        {
            Debug.LogWarning("[grapper] No particle system assigned to the grapper!");
        }
        
        if (pourPoint == null)
        {
            pourPoint = transform;
            Debug.LogWarning("[grapper] Pour point not assigned, using object's transform");
        }
        
        // Posicionar el sistema de partículas en el punto de vertido
        if (waterParticles != null)
        {
            waterParticles.transform.position = pourPoint.position;
            waterParticles.Stop(); // Asegurarse de que esté detenido al inicio
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Obtener el ángulo local en X
        float rawAngle = transform.localEulerAngles.x;
        
        // Normalizar el ángulo para que esté en el rango -180 a 180
        // (Unity usa 0-360, pero para la lógica necesitamos -180 a 180)
        float normalizedAngle = rawAngle;
        if (normalizedAngle > 180)
        {
            normalizedAngle -= 360;
        }
        
        // Debug info para ambos formatos de ángulo
        Debug.Log($"[grapper] Ángulo X raw: {rawAngle:F1}°, normalizado: {normalizedAngle:F1}°");
        
        // Condición: activar si el ángulo es mayor a 180 o negativo
        bool shouldPour = (rawAngle > 180) || (normalizedAngle < 0);
        
        // Activar o desactivar el sistema de partículas
        if (shouldPour && !isPouring)
        {
            StartPouring();
        }
        else if (!shouldPour && isPouring)
        {
            StopPouring();
        }
    }

    void StartPouring()
    {
        if (waterParticles != null)
        {
            // Asegurarse de que esté en la posición correcta
            waterParticles.transform.position = pourPoint.position;
            
            // Alinear la dirección de emisión con la gravedad
            waterParticles.transform.rotation = Quaternion.LookRotation(Vector3.down);
            
            waterParticles.Play();
            isPouring = true;
            Debug.Log("[grapper] Empezando a verter agua");
        }
    }

    void StopPouring()
    {
        if (waterParticles != null)
        {
            waterParticles.Stop();
            isPouring = false;
            Debug.Log("[grapper] Deteniendo el vertido de agua");
        }
    }
}

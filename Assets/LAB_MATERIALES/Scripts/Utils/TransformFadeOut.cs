using UnityEngine;
using System.Collections;

public class TransformFadeOut : MonoBehaviour
{
    [Tooltip("Objeto cuyo transform se verificará para desaparecer.")]
    public Transform targetObject;

    [Tooltip("Centro de la zona de desaparición.")]
    public Transform center;

    [Tooltip("Radio en metros para activar el fade out (en el plano horizontal XZ).")]
    public float radius = 0.2f;

    [Tooltip("Tiempo en segundos que dura el fade out (desaparición).")]
    public float fadeDuration = 1.0f;

    private Vector3 originalScale;

    void Start()
    {
        if(targetObject != null)
        {
            originalScale = targetObject.localScale;
        }
    }

    void Update()
    {
        if(targetObject == null || center == null)
            return;

        // Proyectamos las posiciones en el plano XZ (ignorando Y)
        Vector3 targetXZ = new Vector3(targetObject.position.x, 0f, targetObject.position.z);
        Vector3 centerXZ = new Vector3(center.position.x, 0f, center.position.z);

        float distance = Vector3.Distance(targetXZ, centerXZ);

        // Si la distancia en XZ es menor o igual que el radio, ajustamos la escala gradualmente.
        if(distance <= radius)
        {
            float t = 1f - Mathf.Clamp01(distance / radius);
            center.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            if(t >= 1f)
            {
                center.gameObject.SetActive(false);
            }
        }
        // Opcional: si el objeto sale del radio, podrías restaurar su escala original.
        // else {
        //     targetObject.localScale = originalScale;
        // }
    }
}

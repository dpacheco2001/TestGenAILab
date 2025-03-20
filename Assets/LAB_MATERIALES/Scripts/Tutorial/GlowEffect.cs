using UnityEngine;

public class GlowEffect : MonoBehaviour
{
    [Header("Glow Settings")]
    [Tooltip("Color del glow/emisión.")]
    public Color glowColor = Color.cyan;
    [Tooltip("Intensidad de la emisión.")]
    public float glowIntensity = 2f;
    [Tooltip("Si quieres pulsar el glow, este valor determina la velocidad.")]
    public float pulseSpeed = 1f;
    [Tooltip("Si se activa, el glow pulsará.")]
    public bool pulsate = false;

    private Material[] materials;
    private float baseIntensity;

    void Start()
    {
        // Obtiene todos los materiales del objeto
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            materials = rend.materials;
            // Configura la emisión para cada material que la soporte.
            foreach (Material mat in materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.EnableKeyword("_EMISSION");
                    // Guarda la intensidad base (podrías querer ajustar esto según tu material)
                    baseIntensity = glowIntensity;
                    mat.SetColor("_EmissionColor", glowColor * glowIntensity);
                }
            }
        }
    }

    void Update()
    {
        if(pulsate && materials != null)
        {
            // Calcula una intensidad pulsante
            float pulse = baseIntensity + Mathf.PingPong(Time.time * pulseSpeed, baseIntensity);
            foreach(Material mat in materials)
            {
                if(mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", glowColor * pulse);
                }
            }
        }
    }

    // Método para activar o desactivar el glow de forma programática.
    public void SetGlowActive(bool active)
    {
        if(materials == null) return;

        foreach(Material mat in materials)
        {
            if(mat.HasProperty("_EmissionColor"))
            {
                if(active)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", glowColor * glowIntensity);
                }
                else
                {
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }
}

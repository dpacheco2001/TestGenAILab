using UnityEngine;
using UnityEngine.UI;

public class SubcontentPopManager : MonoBehaviour
{
    [Header("Toggles de este subgrupo (p.e. Tareas, Fotos, Docs)")]
    public Toggle[] subToggles;

    [Header("Contenidos asociados (cada uno con ScaleInterpolator)")]
    public ScaleInterpolator[] subContents;

    [Header("Tiempo de animación (segundos)")]
    public float animationDuration = 1f;

    [Header("Índice del toggle por defecto (0-based)")]
    public int defaultSubIndex = 0;

    // Indice actualmente activo (ya aplicado)
    private int currentActiveSubIndex = -1;
    // Índice deseado (incluso si el panel está apagado)
    private int desiredActiveSubIndex = -1;
    
    // Umbral para considerar que el objeto está apagado (escala ≈ 0)
    private const float scaleThreshold = 0.01f;

    void Start()
    {
        if (subToggles.Length != subContents.Length)
        {
            Debug.LogError("El número de toggles no coincide con el número de contenidos.");
            return;
        }

        // Inicializamos cada toggle y su contenido según el valor por defecto
        for (int i = 0; i < subToggles.Length; i++)
        {
            int index = i;
            bool isDefault = (i == defaultSubIndex);
            subToggles[i].isOn = isDefault;
            subContents[i].animationDuration = animationDuration;
            if (isDefault)
            {
                subContents[i].PopUp();
                currentActiveSubIndex = i;
                desiredActiveSubIndex = i;
            }
            else
            {
                subContents[i].PopOff();
            }
            
            // Registramos el listener para cada toggle
            subToggles[i].onValueChanged.AddListener((isOn) => OnSubToggleChanged(index, isOn));
        }
    }

    void Update()
    {
        // Si el contenedor (o panel) se reactiva (scale > 0) y el índice deseado difiere del actual,
        // se actualiza el contenido, aplicando PopUp al deseado y PopOff a los demás.
        if (transform.localScale.magnitude > scaleThreshold && desiredActiveSubIndex != currentActiveSubIndex)
        {
            ApplySubToggleChange(desiredActiveSubIndex);
        }
    }

    /// <summary>
    /// Se ejecuta cuando cambia el estado de un toggle.
    /// Guarda el índice deseado y, si el panel está activo (scale > 0), aplica el cambio.
    /// </summary>
    /// <param name="index">Índice del toggle que cambió.</param>
    /// <param name="isOn">Nuevo estado del toggle.</param>
    void OnSubToggleChanged(int index, bool isOn)
    {
        if (isOn)
        {
            desiredActiveSubIndex = index;
            
            // Si el contenedor está apagado, no se realiza la animación hasta que se reactive.
            if (transform.localScale.magnitude < scaleThreshold)
                return;

            // Si el contenedor está activo y el índice deseado difiere del actual, se actualiza
            if (currentActiveSubIndex != index)
            {
                ApplySubToggleChange(index);
            }
        }
        else
        {
            // Si un toggle se desactiva, se hace PopOff de su contenido
            subContents[index].animationDuration = animationDuration;
            subContents[index].PopOff();
        }
    }

    /// <summary>
    /// Aplica el cambio en el contenido secundario:
    /// Hace PopUp al contenido del índice indicado y PopOff a los demás toggles.
    /// </summary>
    /// <param name="newIndex">El nuevo índice que debe estar activo.</param>
    void ApplySubToggleChange(int newIndex)
    {
        subContents[newIndex].animationDuration = animationDuration;
        subContents[newIndex].PopUp();

        for (int i = 0; i < subToggles.Length; i++)
        {
            if (i != newIndex)
            {
                if (subToggles[i].isOn)
                {
                    subToggles[i].isOn = false;
                }
                subContents[i].animationDuration = animationDuration;
                subContents[i].PopOff();
            }
        }
        currentActiveSubIndex = newIndex;
    }
}

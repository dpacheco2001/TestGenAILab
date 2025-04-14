using UnityEngine;
using UnityEngine.UI;

public class ToggleGroupPopManager : MonoBehaviour
{
    [Header("Toggles (orden debe coincidir con el contenido)")]
    public Toggle[] toggles;
    
    [Header("Contenidos asociados (cada uno con ScaleInterpolator)")]
    public ScaleInterpolator[] contentItems;
    
    [Header("Duración de la animación (segundos)")]
    public float animationDuration = 1.0f;
    
    [Header("Índice del Toggle por defecto (0-based)")]
    public int defaultIndex = 0;
    
    // Guardamos el índice del toggle actualmente activo
    private int currentActiveIndex;

    void Start()
    {
        if (toggles.Length != contentItems.Length)
        {
            Debug.LogError("El número de toggles y de elementos de contenido debe ser igual.");
            return;
        }

        // Inicializamos con el toggle por defecto
        currentActiveIndex = defaultIndex;
        
        for (int i = 0; i < toggles.Length; i++)
        {
            // Si es el toggle por defecto, se enciende; los demás se apagan
            bool initialValue = (i == defaultIndex);
            toggles[i].isOn = initialValue;
            contentItems[i].animationDuration = animationDuration;
            if (initialValue)
            {
                contentItems[i].PopUp();
            }
            else
            {
                contentItems[i].PopOff();
            }
            
            // Capturamos el índice de forma local para asignarlo al listener
            int index = i;
            toggles[i].onValueChanged.AddListener((isOn) => OnToggleChanged(index, isOn));
        }
    }

    /// <summary>
    /// Maneja el cambio de estado de cada toggle.
    /// Si se activa un toggle diferente al actualmente activo, se hace pop up de su contenido y pop off de los demás.
    /// Si se activa el toggle ya activo, no se realiza ninguna acción.
    /// </summary>
    /// <param name="index">Índice del toggle que cambió</param>
    /// <param name="isOn">Nuevo estado del toggle</param>
    void OnToggleChanged(int index, bool isOn)
    {
        if (isOn)
        {
            // Si el toggle activo ya es el mismo, no hacemos nada.
            if (currentActiveIndex == index)
                return;

            // Actualizamos el índice del toggle activo
            currentActiveIndex = index;
            
            // Activa (pop up) el contenido asociado al toggle que se acaba de activar
            contentItems[index].animationDuration = animationDuration;
            contentItems[index].PopUp();

            // Desactiva (pop off) el contenido de los demás toggles
            for (int i = 0; i < toggles.Length; i++)
            {
                if (i != index)
                {
                    if (toggles[i].isOn)
                    {
                        toggles[i].isOn = false;
                    }
                    contentItems[i].animationDuration = animationDuration;
                    contentItems[i].PopOff();
                }
            }
        }
        else
        {
            // Si el toggle se desactiva, hacemos PopOff de su contenido.
            // (Aquí podrías también evitar que se desactive si quieres que siempre haya uno activo.)
            contentItems[index].animationDuration = animationDuration;
            contentItems[index].PopOff();
        }
    }
}

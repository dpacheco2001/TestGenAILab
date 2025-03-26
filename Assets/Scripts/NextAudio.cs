using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class VRButtonAudioNavigation : MonoBehaviour
{
    public GameObject[] audioGameObjects; // Los GameObjects que tienen los AudioSources
    private int currentIndex = -1; // -1 indica que ningún audio está activo
    private bool buttonPressed = false; // Para evitar múltiples activaciones
    private List<InputDevice> devices = new List<InputDevice>(); // Lista de dispositivos
    private bool isFirstActivation = true; // Para saber si es la primera vez que presionas un botón

    void Start()
    {
        // Asegúrate de que haya al menos un GameObject en la lista
        if (audioGameObjects.Length == 0)
        {
            Debug.LogError("No hay GameObjects con AudioSources en la escena.");
        }
        else
        {
            // Desactivar TODOS los GameObjects de audio al inicio
            for (int i = 0; i < audioGameObjects.Length; i++)
            {
                audioGameObjects[i].SetActive(false); // Todos desactivados inicialmente
            }

            // DEBUG: Mostrar cuántos audios hay disponibles
            Debug.Log("Total de audios disponibles: " + audioGameObjects.Length);
            Debug.Log("La escena inicia en silencio. Presiona un botón para activar el primer audio.");
        }
    }

    void Update()
    {
        // Intentar obtener los dispositivos en cada frame
        GetDevices();
        
        // Verificar cada dispositivo para botones
        bool buttonBPressed = CheckButtonPress(CommonUsages.secondaryButton); // Botón B
        bool buttonAPressed = CheckButtonPress(CommonUsages.primaryButton);   // Botón A
        bool buttonXPressed = CheckButtonPress(CommonUsages.primaryButton, XRNode.LeftHand); // Botón X
        bool buttonYPressed = CheckButtonPress(CommonUsages.secondaryButton, XRNode.LeftHand); // Botón Y
        
        // DEBUG de botones
        if (buttonXPressed) Debug.Log("Botón X presionado");

        
        // Verificamos cualquiera de los botones para avanzar
        bool anyButtonPressed = buttonXPressed;
        
        if (anyButtonPressed)
        {
            if (!buttonPressed) // Evitar que se active varias veces con un solo clic
            {
                buttonPressed = true;
                Debug.Log("Procesando la activación del ciclo de audio.");
                
                if (isFirstActivation)
                {
                    // Primera activación: mostrar el primer audio
                    isFirstActivation = false;
                    currentIndex = 0;
                    audioGameObjects[currentIndex].SetActive(true);
                    Debug.Log("Primer audio activado: " + audioGameObjects[currentIndex].name);
                }
                else
                {
                    // Activaciones posteriores: ciclar al siguiente audio
                    CycleAudio();
                }
            }
        }
        else
        {
            buttonPressed = false; // Resetear cuando se suelta el botón
        }
    }
    
    private void GetDevices()
    {
        InputDevices.GetDevices(devices);
        if (devices.Count == 0)
        {
            Debug.LogWarning("No se detectaron dispositivos VR");
        }
    }
    
    private bool CheckButtonPress(InputFeatureUsage<bool> button, XRNode node = XRNode.RightHand)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (device.isValid)
        {
            if (device.TryGetFeatureValue(button, out bool isPressed) && isPressed)
            {
                return true;
            }
        }
        return false;
    }

    void CycleAudio()
    {
        // Desactivar el GameObject de audio actual
        audioGameObjects[currentIndex].SetActive(false);

        // Aumentar el índice para seleccionar el siguiente audio
        currentIndex = (currentIndex + 1) % audioGameObjects.Length;

        // Activar el siguiente GameObject de audio
        audioGameObjects[currentIndex].SetActive(true);

        // DEBUG: Mostrar qué audio se está reproduciendo ahora
        Debug.Log("Reproduciendo audio #" + (currentIndex + 1) + ": " + audioGameObjects[currentIndex].name);
    }
}
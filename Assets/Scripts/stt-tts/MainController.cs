using System;
using System.Collections.Generic;
// Cambiamos esta línea para importar específicamente Stopwatch sin el namespace completo
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;

public class MainController : MonoBehaviour
{
    public AudioRecorder audioRecorder;               // Asigna desde el Inspector
    public TranscriptionManager transcriptionManager;   // Asigna desde el Inspector
    public Text transcriptionText;                      // Referencia al TextMeshPro en el Canvas
    public Button recordButton;                         // Referencia al botón de grabación (modo no-VR, opcional)
    public Image volumeIndicator;                       // Opcional: indicador visual de volumen

    [Header("VR Settings")]
    [SerializeField] private bool enableVRInput = true;
    [SerializeField] private InputDeviceCharacteristics controllerCharacteristics =
        InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
    [SerializeField] private bool useSecondaryButton = false;  // True para botón B/Y, False para botón A/X

    private bool isRecording = false;
    private InputDevice targetController;
    private bool wasPressed = false;

    void Start()
    {
        // Verifica que las referencias estén asignadas
        if (transcriptionManager == null)
        {
            UnityEngine.Debug.LogError("TranscriptionManager reference not set. Please assign in Inspector.");
        }
        if (audioRecorder == null)
        {
            UnityEngine.Debug.LogError("AudioRecorder reference not set. Please assign in Inspector.");
        }
        if (transcriptionText != null)
        {
            transcriptionText.text = "Mantén presionado para grabar";
        }

        // Si se usa el botón UI (modo no-VR), se podría asignar un listener
        if (recordButton != null)
        {
            recordButton.onClick.AddListener(ToggleRecording);
        }
        else
        {
            UnityEngine.Debug.LogWarning("Record button reference not set. Using VR controller input only.");
        }

        // Inicializa el controlador VR
        if (enableVRInput)
        {
            TryInitializeController();
        }
    }

    void TryInitializeController()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            targetController = devices[0];
            UnityEngine.Debug.Log($"Target controller found: {targetController.name}");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Target controller not found. Will try again in Update.");
        }
    }

    async void StartRecording()
    {
        if (isRecording) return; // Evita iniciar si ya está grabando
        
        // Inicia grabación
        if (audioRecorder != null)
        {
            audioRecorder.StartRecording();
        }

        // Si se usa UI, actualiza el texto del botón
        if (recordButton != null)
        {
            TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Grabando...";
            }
        }

        if (transcriptionText != null)
        {
            transcriptionText.text = "Grabando... (suelta para finalizar)";
        }

        isRecording = true;
    }

    async void StopRecording()
    {
        if (!isRecording) return; // Evita detener si no está grabando
        
        // Detiene grabación
        if (audioRecorder != null)
        {
            audioRecorder.StopRecording();
        }

        if (recordButton != null)
        {
            TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = "Mantén para Grabar";
            }
        }

        isRecording = false;

        if (transcriptionText != null)
        {
            transcriptionText.text = "Procesando transcripción...";
        }

        // Mide el tiempo de procesamiento
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        // Obtiene los datos de audio y los envía a transcripción
        byte[] wavData = audioRecorder != null ? audioRecorder.GetWavData() : null;
        if (wavData == null || wavData.Length == 0)
        {
            if (transcriptionText != null)
            {
                transcriptionText.text = "Error: No se pudo obtener audio";
            }
            return;
        }

        if (transcriptionManager == null)
        {
            UnityEngine.Debug.LogError("TranscriptionManager reference is null");
            if (transcriptionText != null)
            {
                transcriptionText.text = "Error: TranscriptionManager no configurado";
            }
            return;
        }

        string transcription = await transcriptionManager.TranscribeAudio(wavData);

        // Detiene el cronómetro
        stopwatch.Stop();
        float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;

        if (transcriptionText != null)
        {
            if (!string.IsNullOrEmpty(transcription))
            {
                transcriptionText.text = transcription + $"\n\nTiempo de procesamiento: {elapsedSeconds:F2} segundos";
            }
            else
            {
                transcriptionText.text = "Error en la transcripción";
            }
        }
    }
    
    // Para mantener compatibilidad con el botón UI
    public void ToggleRecording()
    {
        if (!isRecording)
            StartRecording();
        else
            StopRecording();
    }

    void Update()
    {
        // Procesa la entrada del controlador VR
        if (enableVRInput)
        {
            // Si el controlador no está inicializado, intenta de nuevo
            if (!targetController.isValid)
            {
                TryInitializeController();
                return;
            }

            bool primaryButtonPressed = false;
            bool secondaryButtonPressed = false;

            // Obtiene el estado de los botones
            targetController.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed);
            targetController.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPressed);

            // Selecciona el botón según la configuración (A/X o B/Y)
            bool buttonPressed = useSecondaryButton ? secondaryButtonPressed : primaryButtonPressed;

            // NUEVO: Inicia al presionar y detiene al soltar
            if (buttonPressed && !wasPressed) // Botón recién presionado
            {
                wasPressed = true;
                StartRecording();
            }
            else if (!buttonPressed && wasPressed) // Botón recién soltado
            {
                wasPressed = false;
                StopRecording();
            }
        }

        // Actualiza el indicador de volumen mientras se graba
        if (isRecording && volumeIndicator != null && audioRecorder != null)
        {
            float audioLevel = audioRecorder.GetCurrentAudioLevel();
            volumeIndicator.fillAmount = Mathf.Lerp(volumeIndicator.fillAmount,
                                                    Mathf.Clamp01(audioLevel * 5),
                                                    Time.deltaTime * 10f);
        }
    }
}

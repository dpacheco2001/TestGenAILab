using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainController : MonoBehaviour {
    public AudioRecorder audioRecorder;           // Asigna desde el Inspector
    public TranscriptionManager transcriptionManager; // Asigna desde el Inspector
    public Text transcriptionText;     // Referencia al TextMeshPro en el Canvas
    public Button recordButton;                   // Referencia al botón de grabación
    public Image volumeIndicator;                 // Opcional: indicador visual de volumen
    
    private bool isRecording = false;

    void Start() {
        // Verify component references
        if (transcriptionManager == null)
        {
            Debug.LogError("TranscriptionManager reference not set. Please assign in Inspector.");
        }
        
        if (audioRecorder == null)
        {
            Debug.LogError("AudioRecorder reference not set. Please assign in Inspector.");
        }
        
        if (transcriptionText != null) {
            transcriptionText.text = "Presiona el botón para grabar";
        }
        
        // Make sure the button is connected to the ToggleRecording method
        if (recordButton != null)
        {
            recordButton.onClick.AddListener(ToggleRecording);
        }
        else
        {
            Debug.LogError("Record button reference not set. Please assign in Inspector.");
        }
    }

    async void ToggleRecording() {
        if (!isRecording) {
            // Inicia grabación
            if (audioRecorder != null) {
                audioRecorder.StartRecording();
            }
            
            if (recordButton != null) {
                TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) {
                    buttonText.text = "Detener Grabación";
                }
            }
            
            if (transcriptionText != null) {
                transcriptionText.text = "Grabando...";
            }
            
            isRecording = true;
        } else {
            // Detiene grabación
            if (audioRecorder != null) {
                audioRecorder.StopRecording();
            }
            
            if (recordButton != null) {
                TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) {
                    buttonText.text = "Iniciar Grabación";
                }
            }
            
            isRecording = false;
            
            if (transcriptionText != null) {
                transcriptionText.text = "Procesando transcripción...";
            }

            // Start measuring time
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // Obtiene los datos de audio y envía a transcripción
            byte[] wavData = audioRecorder != null ? audioRecorder.GetWavData() : null;
            if (wavData == null || wavData.Length == 0) {
                if (transcriptionText != null) {
                    transcriptionText.text = "Error: No se pudo obtener audio";
                }
                return;
            }
            
            // Check if transcriptionManager is available
            if (transcriptionManager == null)
            {
                Debug.LogError("TranscriptionManager reference is null");
                if (transcriptionText != null)
                {
                    transcriptionText.text = "Error: TranscriptionManager no configurado";
                }
                return;
            }
            
            string transcription = await transcriptionManager.TranscribeAudio(wavData);
            
            // Stop measuring time
            stopwatch.Stop();
            float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;
            
            if (transcriptionText != null) {
                if (!string.IsNullOrEmpty(transcription)) {
                    transcriptionText.text = transcription + $"\n\nTiempo de procesamiento: {elapsedSeconds:F2} segundos";
                } else {
                    transcriptionText.text = "Error en la transcripción";
                }
            }
        }
    }
    
    void Update() {
        // Actualizar indicador de volumen si está grabando
        if (isRecording && volumeIndicator != null) {
            float audioLevel = audioRecorder.GetCurrentAudioLevel();
            volumeIndicator.fillAmount = Mathf.Lerp(volumeIndicator.fillAmount, 
                                                  Mathf.Clamp01(audioLevel * 5), 
                                                  Time.deltaTime * 10f);
        }
        
        // Opcional: permitir grabación con tecla (como en el código Python)
        if (Input.GetKeyDown(KeyCode.R)) {
            ToggleRecording();
        }
    }
}
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
        // Configura la acción del botón
        recordButton.onClick.AddListener(ToggleRecording);
        
        // Inicializar texto
        if (transcriptionText != null) {
            transcriptionText.text = "Presiona el botón para grabar";
        }
    }

    async void ToggleRecording() {
        if (!isRecording) {
            // Inicia grabación
            audioRecorder.StartRecording();
            recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "Detener Grabación";
            if (transcriptionText != null) {
                transcriptionText.text = "Grabando...";
            }
            isRecording = true;
        } else {
            // Detiene grabación
            audioRecorder.StopRecording();
            recordButton.GetComponentInChildren<TextMeshProUGUI>().text = "Iniciar Grabación";
            isRecording = false;
            
            if (transcriptionText != null) {
                transcriptionText.text = "Procesando transcripción...";
            }

            // Obtiene los datos de audio y envía a transcripción
            byte[] wavData = audioRecorder.GetWavData();
            if (wavData == null || wavData.Length == 0) {
                if (transcriptionText != null) {
                    transcriptionText.text = "Error: No se pudo obtener audio";
                }
                return;
            }
            
            string transcription = await transcriptionManager.TranscribeAudio(wavData);
            if (!string.IsNullOrEmpty(transcription)) {
                transcriptionText.text = transcription;
            } else {
                transcriptionText.text = "Error en la transcripción";
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
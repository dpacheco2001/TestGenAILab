// using System;
// using System.Collections; 
// using System.Collections.Generic;
// using Stopwatch = System.Diagnostics.Stopwatch;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using UnityEngine.XR;

// public class SpeechAssistantController : MonoBehaviour
// {
//     [Header("Speech Recognition")]
//     public AudioRecorder audioRecorder;                
//     public TranscriptionManager transcriptionManager;  
//     public Text transcriptionText;                     
//     public Button recordButton;                        
//     public Image volumeIndicator;                      

//     [Header("Assistant Integration")]
//     public Assistant assistant;                       
//     public string studentCode = "20190051";            
//     public Text assistantResponseText;                 
//     public bool autoSendTranscription = true;          

//     [Header("Text-to-Speech")]
//     public ElevenLabsTTS textToSpeech;                
//     public bool autoSpeakResponse = true;              
//     public float speakDelay = 0.1f;                    
//     [Header("VR Settings")]
//     [SerializeField] private bool enableVRInput = true;
//     [SerializeField] private InputDeviceCharacteristics controllerCharacteristics =
//         InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
//     [SerializeField] private bool useSecondaryButton = false;  

//     private bool isRecording = false;
//     private InputDevice targetController;
//     private bool wasPressed = false;

//     void Start()
//     {
//         if (transcriptionManager == null)
//         {
//             Debug.LogError("TranscriptionManager reference not set. Please assign in Inspector.");
//         }
//         if (audioRecorder == null)
//         {
//             Debug.LogError("AudioRecorder reference not set. Please assign in Inspector.");
//         }
//         if (assistant == null)
//         {
//             Debug.LogError("Assistant reference not set. Please assign in Inspector.");
//         }
        
//         if (transcriptionText != null)
//         {
//             transcriptionText.text = "Mantén presionado para grabar";
//         }

//         if (recordButton != null)
//         {
//             recordButton.onClick.AddListener(ToggleRecording);
//         }
//         if (enableVRInput)
//         {
//             TryInitializeController();
//         }
//     }

//     void TryInitializeController()
//     {
//         List<InputDevice> devices = new List<InputDevice>();
//         InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

//         if (devices.Count > 0)
//         {
//             targetController = devices[0];
//             Debug.Log($"Target controller found: {targetController.name}");
//         }
//         else
//         {
//             Debug.LogWarning("Target controller not found. Will try again in Update.");
//         }
//     }

//     async void StartRecording()
//     {
//         if (isRecording) return; 
        
//         if (audioRecorder != null)
//         {
//             audioRecorder.StartRecording();
//         }

//         if (recordButton != null)
//         {
//             TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
//             if (buttonText != null)
//             {
//                 buttonText.text = "Grabando...";
//             }
//         }

//         if (transcriptionText != null)
//         {
//             transcriptionText.text = "Grabando... (suelta para finalizar)";
//         }

//         isRecording = true;
//     }

//     async void StopRecording()
//     {
//         if (!isRecording) return; 
        
//         if (audioRecorder != null)
//         {
//             audioRecorder.StopRecording();
//         }

//         if (recordButton != null)
//         {
//             TextMeshProUGUI buttonText = recordButton.GetComponentInChildren<TextMeshProUGUI>();
//             if (buttonText != null)
//             {
//                 buttonText.text = "Mantén para Grabar";
//             }
//         }

//         isRecording = false;

//         if (transcriptionText != null)
//         {
//             transcriptionText.text = "Procesando transcripción...";
//         }

//         Stopwatch stopwatch = new Stopwatch();
//         stopwatch.Start();


//         byte[] wavData = audioRecorder != null ? audioRecorder.GetWavData() : null;
//         if (wavData == null || wavData.Length == 0)
//         {
//             if (transcriptionText != null)
//             {
//                 transcriptionText.text = "Error: No se pudo obtener audio";
//             }
//             return;
//         }

//         if (transcriptionManager == null)
//         {
//             Debug.LogError("TranscriptionManager reference is null");
//             if (transcriptionText != null)
//             {
//                 transcriptionText.text = "Error: TranscriptionManager no configurado";
//             }
//             return;
//         }

//         string transcription = await transcriptionManager.TranscribeAudio(wavData);

//         stopwatch.Stop();
//         float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;

//         if (transcriptionText != null)
//         {
//             if (!string.IsNullOrEmpty(transcription))
//             {
//                 transcriptionText.text = transcription + $"\n\nTiempo de procesamiento: {elapsedSeconds:F2} segundos";
                
//                 if (autoSendTranscription && assistant != null)
//                 {
//                     SendTranscriptionToAssistant(transcription);
//                 }
//             }
//             else
//             {
//                 transcriptionText.text = "Error en la transcripción";
//             }
//         }
//     }
    
//     public void SendTranscriptionToAssistant(string transcription)
//     {
//         if (assistant == null)
//         {
//             Debug.LogError("Assistant reference is null");
//             return;
//         }

//         if (assistantResponseText != null)
//         {
//             assistantResponseText.text = "Procesando...";
//         }
        

//         assistant.SendRequest(transcription, studentCode, (assistantResponse, fullResponse) => {
//             Debug.Log("Assistant response: " + assistantResponse);
            

//             if (assistantResponseText != null)
//             {
//                 assistantResponseText.text = assistantResponse;
//             }
            
//             if (autoSpeakResponse && textToSpeech != null && !string.IsNullOrEmpty(assistantResponse))
//             {
//                 SpeakAssistantResponse(assistantResponse);
//             }
//         });
//     }
    
//     public void SpeakAssistantResponse(string response)
//     {
//         if (textToSpeech == null)
//         {
//             Debug.LogError("ElevenLabsTTS reference is null");
//             return;
//         }
        
//         // Configuramos el texto a hablar en el componente TTS
//         textToSpeech.textToSpeak = response;
        
//         // Pequeño retraso para asegurar que la UI se actualice antes de reproducir el audio
//         StartCoroutine(SpeakWithDelay(speakDelay));
//     }
    
//     private IEnumerator SpeakWithDelay(float delay)
//     {
//         yield return new WaitForSeconds(delay);
//         textToSpeech.GenerateAndPlaySpeech();
//     }
    
//     // Para mantener compatibilidad con el botón UI
//     public void ToggleRecording()
//     {
//         if (!isRecording)
//             StartRecording();
//         else
//             StopRecording();
//     }

//     void Update()
//     {
 
//         if (enableVRInput)
//         {
//             if (!targetController.isValid)
//             {
//                 TryInitializeController();
//                 return;
//             }

//             bool primaryButtonPressed = false;
//             bool secondaryButtonPressed = false;


//             targetController.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed);
//             targetController.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPressed);

//             bool buttonPressed = useSecondaryButton ? secondaryButtonPressed : primaryButtonPressed;

         
//             if (buttonPressed && !wasPressed) 
//             {
//                 wasPressed = true;
//                 StartRecording();
//             }
//             else if (!buttonPressed && wasPressed)
//             {
//                 wasPressed = false;
//                 StopRecording();
//             }
//         }

//         if (isRecording && volumeIndicator != null && audioRecorder != null)
//         {
//             float audioLevel = audioRecorder.GetCurrentAudioLevel();
//             volumeIndicator.fillAmount = Mathf.Lerp(volumeIndicator.fillAmount,
//                                                    Mathf.Clamp01(audioLevel * 5),
//                                                    Time.deltaTime * 10f);
//         }
//     }
// }
using System;
using System.Collections;
using System.Collections.Generic;
using Stopwatch = System.Diagnostics.Stopwatch;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;
using System.Text;
using NativeWebSocket;

public class SpeechAssistantControllerWS : MonoBehaviour
{
    [Header("Speech Recognition")]
    public AudioRecorder audioRecorder;                
    public TranscriptionManager transcriptionManager;  
    public Text transcriptionText;                     
    public Button recordButton;                        
    public Image volumeIndicator;                      

    [Header("WebSocket Settings")]
    public string webSocketUrl = "ws://localhost:6789"; 
    public string studentCode = "20190051";             
    public Text assistantResponseText;                  
    public bool autoSendTranscription = true;           

    [Header("Text-to-Speech")]
    public ElevenLabsTTS textToSpeech;                  
    public bool autoSpeakResponse = true;               
    public float speakDelay = 0.1f;                     

    [Header("VR Settings")]
    [SerializeField] private bool enableVRInput = true;
    [SerializeField] private InputDeviceCharacteristics controllerCharacteristics =
        InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
    [SerializeField] private bool useSecondaryButton = false;  

    private WebSocket websocket;
    private bool isRecording = false;
    private InputDevice targetController;
    private bool wasPressed = false;
    private bool isConnected = false;
    private bool isFirstMessageSent = false;

    public bool streaming = false;

    [Header("Utils for tools")]

    public GameObject guia_arrow;
    public GameObject ensenar_mando;

    public GameObject imagen;

    public ResourceUIManager resourceUIManager;

    public bool empezar_con_tutorial = true;

    [Header("User Input Simulation")]
    public bool simulateUserInput = false;
    public string simulatedTranscription = "Hola Robert!";

    public class WebSocketResult
    {
        public bool HasToolCall { get; set; }
        public List<string> ToolNames { get; set; } = new List<string>();
        public string CleanMessage { get; set; }
    }

  public WebSocketResult ProcessMessage(string message)
    {
        var result = new WebSocketResult();
        var cleanedLines = new List<string>();
        var lines = message.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        int i = 0;

        while (i < lines.Length)
        {
            string trimmed = lines[i].Trim();

    
            if (trimmed.Equals("toolcall", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("```toolcall", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("´´´toolcall", StringComparison.OrdinalIgnoreCase))
            {
                result.HasToolCall = true;
                if (i + 1 < lines.Length)
                {
                    string toolName = lines[i + 1].Trim();
                    result.ToolNames.Add(toolName);
                }

           
                i += 2;
                while (i < lines.Length && 
                    !lines[i].Trim().Equals("```") && 
                    !lines[i].Trim().Equals("´´´"))
                {
                    i++;
                }
                if (i < lines.Length) i++;
                continue;
            }

  
            if (trimmed.StartsWith("```mermaid", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("´´´mermaid",  StringComparison.OrdinalIgnoreCase))
            {

                string ensayoName = "Recomendaciones Robert"; 
                var parts = trimmed.Split(new[]{'|'}, 2);
                if (parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1]))
                    ensayoName = parts[1].Trim();


                var sb = new StringBuilder();
                i++;
                while (i < lines.Length &&
                    !lines[i].Trim().Equals("```") &&
                    !lines[i].Trim().Equals("´´´"))
                {
                    sb.AppendLine(lines[i]);
                    i++;
                }

                if (i < lines.Length) i++;

                Debug.LogError(sb.ToString());

                MermaidRenderer.RenderAndAddToUI(sb.ToString(), ensayoName);
                continue;
            }

            if ( trimmed.StartsWith("```videos_encontrados", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("´´´videos_encontrados", StringComparison.OrdinalIgnoreCase))
            {
   
            i++;

            string nombre      = null;
            string descripcion = null;
            string url         = null;
            string thumbUrl    = null;

 
            while (i < lines.Length &&
                  !lines[i].Trim().Equals("```") &&
                  !lines[i].Trim().Equals("´´´"))
            {
                var line = lines[i].Trim();
                if (line.StartsWith("nombre:"))      nombre      = line.Substring(7).Trim();
                else if (line.StartsWith("descripcion:")) descripcion = line.Substring(12).Trim();
                else if (line.StartsWith("url:"))         url         = line.Substring(4).Trim();
                else if (line.StartsWith("thumbnail:"))   thumbUrl    = line.Substring(10).Trim();
                i++;
            }
            if (i < lines.Length) i++;

            resourceUIManager.StartCoroutine(
                resourceUIManager.AddVideoResource(
                    nombre,
                    descripcion,
                    url,
                    thumbUrl
                )
            );
            continue;
            }

            if (trimmed.StartsWith("```images_encontradas", StringComparison.OrdinalIgnoreCase) ||
            trimmed.StartsWith("´´´images_encontradas", StringComparison.OrdinalIgnoreCase))
            {
                i++;
                // Variables para cada imagen
                string imgName = null;
                string imgData = null;

                while (i < lines.Length &&
                    !lines[i].Trim().Equals("```") &&
                    !lines[i].Trim().Equals("´´´"))
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith("nombre:"))
                    {
                        imgName = line.Substring(7).Trim();
                    }
                    else if (line.StartsWith("data:"))
                    {
                        imgData = line.Substring(5).Trim();
                    }
                    i++;
                }
                if (i < lines.Length) i++;

                // Lanza coroutine para reconstruir la textura desde Base64
                resourceUIManager.StartCoroutine(
                    resourceUIManager.AddImageResource(
                        imgName,
                        imgData
                    )
                );
                continue;
            }


            

            // ——— línea normal —————————————————————————
            cleanedLines.Add(lines[i]);
            i++;
        }

        result.CleanMessage = string.Join(Environment.NewLine, cleanedLines);
        return result;
    }

    async void Start()
    {

        if (transcriptionManager == null)
        {
            Debug.LogError("TranscriptionManager reference not set. Please assign in Inspector.");
        }
        if (audioRecorder == null)
        {
            Debug.LogError("AudioRecorder reference not set. Please assign in Inspector.");
        }
        
        if (transcriptionText != null)
        {
            transcriptionText.text = "Mantén presionado para grabar";
        }


        if (recordButton != null)
        {
            recordButton.onClick.AddListener(ToggleRecording);
        }


        if (enableVRInput)
        {
            TryInitializeController();
        }

        
        await ConnectToWebSocket();
    }

    private async System.Threading.Tasks.Task ConnectToWebSocket()
    {
        websocket = new WebSocket(webSocketUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connection opened");

            isConnected = true;
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError($"WebSocket error: {e}");
            isConnected = false;
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket connection closed");
            isConnected = false;
        };

        websocket.OnMessage += (bytes) =>
        {
     
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log($"WebSocket message received: {message}");
            WebSocketResult result = ProcessMessage(message);
            Debug.LogError("Se detectó toolcall? " + result.HasToolCall);
            Debug.LogError("ToolNames: " + string.Join(", ", result.ToolNames));
            Debug.LogError("Mensaje limpio:");
            Debug.LogError(result.CleanMessage);
            if(result.HasToolCall){
                if (ToolCallsRepository.Instance != null)
                {
                    foreach (var toolName in result.ToolNames)
                    {
                        ToolCallsRepository.Instance.InvokeToolCall(toolName);
                    }
                }
                else    
                {
                    Debug.LogWarning("ToolCallsRepository no está inicializado.");
                }
                ProcessWebSocketResponse(result.CleanMessage);
            }
            else{
                ProcessWebSocketResponse(result.CleanMessage);
            }
            
            

        };

 
        await websocket.Connect();
    }

    void Update()
    {   
        if(isConnected && !isFirstMessageSent && empezar_con_tutorial){
            SendTranscriptionToWebSocket("Hacer una busqueda episodica con query: El usuario me pidio que iniciemos con el tutorial");
            isFirstMessageSent = true;
        }
        #if !UNITY_WEBGL || UNITY_EDITOR

        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
        #endif


        if (enableVRInput)
        {
         
            if (!targetController.isValid)
            {
                TryInitializeController();
                return;
            }

            bool primaryButtonPressed = false;
            bool secondaryButtonPressed = false;


            targetController.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPressed);
            targetController.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPressed);


            bool buttonPressed = useSecondaryButton ? secondaryButtonPressed : primaryButtonPressed;

  
            if (buttonPressed && !wasPressed) 
            {
                Debug.LogError("Entre jaja");
                wasPressed = true;
                StartRecording();
            }
            else if (!buttonPressed && wasPressed) 
            {
                wasPressed = false;
                StopRecording();
            }
        }
        else{
            if(simulateUserInput && simulatedTranscription != null){
                simulateUserInput = false;
                SendTranscriptionToWebSocket(simulatedTranscription);
            }
        }


        if (isRecording && volumeIndicator != null && audioRecorder != null)
        {
            float audioLevel = audioRecorder.GetCurrentAudioLevel();
            volumeIndicator.fillAmount = Mathf.Lerp(volumeIndicator.fillAmount,
                                                   Mathf.Clamp01(audioLevel * 5),
                                                   Time.deltaTime * 10f);
        }
    }

    void TryInitializeController()
    {
        List<InputDevice> devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(controllerCharacteristics, devices);

        if (devices.Count > 0)
        {
            targetController = devices[0];
            Debug.Log($"Target controller found: {targetController.name}");
        }

    }


         
    
    async void StartRecording()
    {
        if (isRecording) return; 
        

        if (audioRecorder != null)
        {
            audioRecorder.StartRecording();
        }


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
        if (!isRecording) return; 
        

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


        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();


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
            Debug.LogError("TranscriptionManager reference is null");
            if (transcriptionText != null)
            {
                transcriptionText.text = "Error: TranscriptionManager no configurado";
            }
            return;
        }

        string transcription = await transcriptionManager.TranscribeAudio(wavData);


        stopwatch.Stop();
        float elapsedSeconds = stopwatch.ElapsedMilliseconds / 1000f;


        if (transcription != null)
        {
 
            if (autoSendTranscription)
            {
                SendTranscriptionToWebSocket(transcription);
            }
  
        }
    }
    

    public async void SendTranscriptionToWebSocket(string transcription)
    {
        if (websocket == null || !isConnected)
        {
            Debug.LogError("WebSocket not connected");
            return;
        }

   
        if (assistantResponseText != null)
        {
            assistantResponseText.text = "Procesando...";
        }
        

        string jsonMessage = JsonUtility.ToJson(new WebSocketMessage
        {
            message = transcription,
            codigo = studentCode
        });


        Debug.Log($"Sending to WebSocket: {jsonMessage}");
        await websocket.SendText(jsonMessage);
    }
    

    private void ProcessWebSocketResponse(string response)
    {
        try
        {
            string assistantResponse = response;
            

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                Debug.Log($"Assistant response: {assistantResponse}");
                

                if (assistantResponseText != null)
                {
                    assistantResponseText.text = assistantResponse;
                }
                
                if (autoSpeakResponse && textToSpeech != null && !string.IsNullOrEmpty(assistantResponse))
                {
                    SpeakAssistantResponse(assistantResponse);
                }
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Error parsing WebSocket response: {e.Message}");
            Debug.Log($"Raw response: {response}");
        }
    }

    public void SpeakAssistantResponse(string response)
    {
        if (textToSpeech == null)
        {
            Debug.LogError("ElevenLabsTTS reference is null");
            return;
        }
        
        // Se pasa el texto directamente al método, evitando sobrescribir una variable global.
        StartCoroutine(SpeakWithDelay(speakDelay, response));
    }
        
    private IEnumerator SpeakWithDelay(float delay, string response)
    {
        yield return new WaitForSeconds(delay);
        textToSpeech.GenerateSpeechAndSave(response);
    }
    
    // Para mantener compatibilidad con el botón UI
    public void ToggleRecording()
    {
        if (!isRecording)
            StartRecording();
        else
            StopRecording();
    }

    void OnDestroy()
    {
        // Cierra la conexión WebSocket al destruir el objeto
        if (websocket != null && isConnected)
        {
            websocket.Close();
        }
    }

    void OnApplicationQuit()
    {
        // Cierra la conexión WebSocket al salir de la aplicación
        if (websocket != null && isConnected)
        {
            websocket.Close();
        }
    }

    // Clases para serialización de mensajes
    [Serializable]
    private class WebSocketMessage
    {
        public string message;
        public string codigo;
    }
    
    [Serializable]
    private class WebSocketResponse
    {
        public string assistant;
        public string human;
    }
}

// Clase para ejecutar código en el hilo principal
// Para implementar esto necesitarás crear este script también
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private readonly Queue<Action> _executionQueue = new Queue<Action>();
    
    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            GameObject go = new GameObject("UnityMainThreadDispatcher");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return _instance;
    }

    public void Update()
    {
        lock(_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public void Enqueue(Action action)
    {
        lock(_executionQueue)
        {
            _executionQueue.Enqueue(action);
        }
    }
}
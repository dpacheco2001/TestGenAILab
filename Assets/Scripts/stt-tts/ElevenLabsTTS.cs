using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class ElevenLabsTTS : MonoBehaviour
{
    [SerializeField] private string apiKey = "sk_296cb211c63d8d51029d997376ba4e18b303e5b370f299c3";
    [SerializeField] private string voiceID = "94zOad0g7T7K4oa7zhDq";
    [SerializeField] private string modelID = "eleven_flash_v2_5";
    // Variable opcional para un valor por defecto, pero no se usará para encolar solicitudes.
    [SerializeField] public string defaultTextToSpeak = "Hola que tal me llamo Mateo y hoy seré tu asistente para este laboratorio";

    // Evento global opcional para notificar cada vez que se reproduce un audio.
    public event Action<AudioClip> OnAudioPlayed;

    // AudioSource para reproducir audio
    private AudioSource audioSource;

    // Indicador de que se está procesando una petición.
    private bool isProcessingRequest = false;
    // Cola de peticiones; cada una contiene el texto a convertir y su callback.
    private Queue<RequestData> requestQueue = new Queue<RequestData>();

    private class RequestData
    {
        public string text;
        public Action<AudioClip> callback;

        public RequestData(string text, Action<AudioClip> callback)
        {
            this.text = text;
            this.callback = callback;
        }
    }

    void Start()
    {
        Debug.Log("ElevenLabsTTS initialized.");
        
        // Obtiene o añade un AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Encola una solicitud de generación de audio utilizando el texto proporcionado.
    /// </summary>
    /// <param name="text">Texto a convertir en audio</param>
    /// <param name="callback">
    /// Callback que se invoca al terminar la generación con el parámetro
    /// siendo el AudioClip generado o null en caso de error.
    /// </param>
    public void GenerateSpeechAndSave(string text, Action<AudioClip> callback = null)
    {
        requestQueue.Enqueue(new RequestData(text, callback));
        if (!isProcessingRequest)
        {
            ProcessNextRequest();
        }
    }

    /// <summary>
    /// Método auxiliar para definir el texto y generar el audio.
    /// </summary>
    /// <param name="text">Texto a convertir en audio</param>
    /// <param name="callback">Callback opcional</param>
    public void SpeakText(string text, Action<AudioClip> callback = null)
    {
        GenerateSpeechAndSave(text, callback);
    }

    /// <summary>
    /// Procesa la siguiente petición en la cola, si existe.
    /// </summary>
    private void ProcessNextRequest()
    {
        if (requestQueue.Count > 0)
        {
            isProcessingRequest = true;
            RequestData request = requestQueue.Dequeue();
            StartCoroutine(GetTTS(request.text, request.callback));
        }
        else
        {
            isProcessingRequest = false;
        }
    }
    
    [System.Serializable]
    public class ElevenLabsRequest
    {
        public string text;
        public string model_id;
    }
    
    private IEnumerator GetTTS(string text, Action<AudioClip> callback)
    {
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceID}";

        var requestData = new ElevenLabsRequest
        {
            text = text,
            model_id = modelID
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log($"Enviando solicitud a ElevenLabs...\n{jsonBody}");

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("xi-api-key", apiKey);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "audio/mpeg");

            yield return www.SendWebRequest();

            AudioClip audioClip = null;

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                Debug.LogError($"Respuesta: {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Audio recibido correctamente. Tamaño de datos: {www.downloadHandler.data.Length} bytes");
                yield return StartCoroutine(CreateAndPlayAudioClip(www.downloadHandler.data, clip => {
                    audioClip = clip;
                }));
            }

            callback?.Invoke(audioClip);
            OnAudioPlayed?.Invoke(audioClip);
            
            // Si hay más peticiones en la cola, procesamos la siguiente después de que
            // termine de reproducirse el clip actual o inmediatamente si no hay clip
            if (requestQueue.Count > 0)
            {
                if (audioClip != null && audioSource.isPlaying)
                {
                    // Esperar a que termine el audio actual antes de procesar la siguiente solicitud
                    yield return new WaitForSeconds(audioClip.length);
                }
                ProcessNextRequest();
            }
            else
            {
                isProcessingRequest = false;
            }
        }
    }

    /// <summary>
    /// Crea un AudioClip a partir de los datos MP3 y lo reproduce.
    /// </summary>
    private IEnumerator CreateAndPlayAudioClip(byte[] audioData, Action<AudioClip> onAudioClipCreated)
    {
        // Creamos un archivo temporal para cargar el audio
        string tempFilePath = $"{Application.temporaryCachePath}/temp_audio_{DateTime.Now.Ticks}.mp3";
        
        // Escribir los datos en un archivo temporal
        try {
            System.IO.File.WriteAllBytes(tempFilePath, audioData);
        }
        catch (System.Exception ex) {
            Debug.LogError($"Error al escribir el archivo temporal: {ex.Message}");
            onAudioClipCreated?.Invoke(null);
            yield break;
        }
        
        // Crear la solicitud web para cargar el audio desde el archivo
        UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + tempFilePath, AudioType.MPEG);
        
        // Esperar a que la solicitud se complete
        yield return www.SendWebRequest();
        
        // Intentar eliminar el archivo temporal ahora que ya no se necesita
        try {
            if (System.IO.File.Exists(tempFilePath)) {
                System.IO.File.Delete(tempFilePath);
            }
        }
        catch (System.Exception ex) {
            Debug.LogWarning($"No se pudo eliminar el archivo temporal: {ex.Message}");
        }
        
        // Procesar el resultado de la solicitud web
        if (www.result != UnityWebRequest.Result.Success) {
            Debug.LogError($"Error al cargar el audio: {www.error}");
            onAudioClipCreated?.Invoke(null);
        }
        else {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip == null) {
                Debug.LogError("No se pudo crear el AudioClip");
                onAudioClipCreated?.Invoke(null);
            }
            else {
                // Reproducir el audio inmediatamente
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log($"Reproduciendo audio de {clip.length} segundos");
                
                onAudioClipCreated?.Invoke(clip);
            }
        }
    }
}

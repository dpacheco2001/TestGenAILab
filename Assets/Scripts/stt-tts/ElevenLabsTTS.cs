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

    // Evento global opcional para notificar cada vez que se genera un audio.
    public event Action<string> OnAudioGenerated;

    // Indicador de que se está procesando una petición.
    private bool isProcessingRequest = false;
    // Cola de peticiones; cada una contiene el texto a convertir y su callback.
    private Queue<RequestData> requestQueue = new Queue<RequestData>();

    private class RequestData
    {
        public string text;
        public Action<string> callback;

        public RequestData(string text, Action<string> callback)
        {
            this.text = text;
            this.callback = callback;
        }
    }

    void Start()
    {
        Debug.Log("ElevenLabsTTS initialized.");
        // Limpiar cualquier estado residual al iniciar
        CleanUpTempFiles();
        ClearQueue();
    }

    void OnDestroy()
    {
        // Limpiar la cola cuando se destruye el objeto
        ClearQueue();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        // Limpiar la cola cuando se pausa la aplicación
        if (pauseStatus)
        {
            ClearQueue();
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        // Limpiar la cola cuando se pierde el foco
        if (!hasFocus)
        {
            ClearQueue();
        }
    }

    /// <summary>
    /// Limpia la cola de solicitudes y resetea el estado de procesamiento.
    /// También elimina archivos temporales existentes.
    /// </summary>
    public void ClearQueue()
    {
        requestQueue.Clear();
        isProcessingRequest = false;
        CleanUpTempFiles();
        Debug.Log("Cola de solicitudes TTS limpiada y archivos temporales eliminados.");
    }

    /// <summary>
    /// Detiene todas las corrutinas relacionadas con TTS y limpia la cola.
    /// </summary>
    public void StopAllTTSProcessing()
    {
        StopAllCoroutines();
        ClearQueue();
        Debug.Log("Todas las solicitudes TTS detenidas y cola limpiada.");
    }

    /// <summary>
    /// Encola una solicitud de generación de audio utilizando el texto proporcionado.
    /// </summary>
    /// <param name="text">Texto a convertir en audio</param>
    /// <param name="callback">
    /// Callback que se invoca al terminar la generación con el parámetro
    /// siendo la ruta del archivo generado o null en caso de error.
    /// </param>
    public void GenerateSpeechAndSave(string text, Action<string> callback = null)
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
    public void SpeakText(string text, Action<string> callback = null)
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
    private IEnumerator GetTTS(string text, Action<string> callback)
    {
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceID}";

        var requestData = new ElevenLabsRequest
        {
            text = text,
            model_id = modelID
        };

        string jsonBody = JsonUtility.ToJson(requestData);
        Debug.Log($"Enviando solicitud a ElevenLabs...\n{jsonBody}");

        string generatedFilePath = null;

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("xi-api-key", apiKey);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "audio/mpeg");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                Debug.LogError($"Respuesta: {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Audio recibido correctamente. Tamaño de datos: {www.downloadHandler.data.Length} bytes");

                generatedFilePath = GetUniqueTempFilePath();
                File.WriteAllBytes(generatedFilePath, www.downloadHandler.data);
                Debug.Log($"Audio guardado en: {generatedFilePath}");
            }
        }

        callback?.Invoke(generatedFilePath);
        OnAudioGenerated?.Invoke(generatedFilePath);
        ProcessNextRequest();
    }

    /// <summary>
    /// Genera un nombre de archivo único en la carpeta temporal, usando un contador consecutivo.
    /// </summary>
    /// <returns>Ruta completa del archivo temporal</returns>
    private string GetUniqueTempFilePath()
    {
        string directory = Application.temporaryCachePath;
        int counter = 0;
        string filePath;
        do
        {
            filePath = Path.Combine(directory, $"tts_audio_{counter}.mp3");
            counter++;
        } while (File.Exists(filePath));

        return filePath;
    }

    /// <summary>
    /// Elimina todos los archivos temporales de audio TTS.
    /// </summary>
    private void CleanUpTempFiles()
    {
        try
        {
            string directory = Application.temporaryCachePath;
            string[] audioFiles = Directory.GetFiles(directory, "tts_audio_*.mp3");
            
            foreach (string file in audioFiles)
            {
                try
                {
                    File.Delete(file);
                    Debug.Log($"Archivo temporal eliminado: {file}");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"No se pudo eliminar el archivo {file}: {e.Message}");
                }
            }
            
            if (audioFiles.Length > 0)
            {
                Debug.Log($"Se eliminaron {audioFiles.Length} archivos temporales de TTS.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al limpiar archivos temporales: {e.Message}");
        }
    }
}

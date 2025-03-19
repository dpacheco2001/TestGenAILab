using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class ElevenLabsTTS : MonoBehaviour
{
    [SerializeField] private string apiKey = "sk_060b722156b8f4e269411520928a5530c22f46f73fd32329";
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

    private IEnumerator GetTTS(string text, Action<string> callback)
    {
        // Endpoint para la generación de TTS.
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceID}";

        // Cuerpo de la petición en formato JSON usando el texto pasado como parámetro.
        string jsonBody = "{" +
            $"\"text\": \"{text}\"," +
            $"\"model_id\": \"{modelID}\"" +
        "}";

        Debug.Log("Enviando solicitud a ElevenLabs...");

        string generatedFilePath = null;

        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("xi-api-key", apiKey);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "audio/mpeg");  // Se solicita el formato MP3

            // Envía la solicitud y espera la respuesta.
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                Debug.LogError($"Respuesta: {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Audio recibido correctamente. Tamaño de datos: {www.downloadHandler.data.Length} bytes");

                // Genera un nombre único para el archivo temporal.
                generatedFilePath = GetUniqueTempFilePath();
                File.WriteAllBytes(generatedFilePath, www.downloadHandler.data);
                Debug.Log($"Audio guardado en: {generatedFilePath}");
            }
        }

        // Se notifica mediante el callback y el evento global.
        callback?.Invoke(generatedFilePath);
        OnAudioGenerated?.Invoke(generatedFilePath);

        // Procesa la siguiente petición en la cola.
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
}

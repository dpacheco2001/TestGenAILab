using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class TranscriptionManager : MonoBehaviour
{
    [Header("API Configuration")]
    [SerializeField] private string groqApiKey = "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"; // Introduce tu API key de Groq en el Inspector
    [SerializeField] private string apiEndpoint = "https://api.groq.com/openai/v1/audio/transcriptions";
    // Método principal que llama el MainController
    public async Task<string> TranscribeAudio(byte[] audioData)
    {
        try
        {
            // Guarda temporalmente el archivo WAV
            string tempWavPath = Path.Combine(Application.temporaryCachePath, "temp_recording.wav");
            File.WriteAllBytes(tempWavPath, audioData);
            
            // Crea el form para la solicitud HTTP
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", audioData, "audio.wav", "audio/wav");
            form.AddField("model", "whisper-large-v3");
            form.AddField("language", "es");
            
            // Prepara la solicitud UnityWebRequest
            using (UnityWebRequest www = UnityWebRequest.Post(apiEndpoint, form))
            {
                // Configura los headers para la API de Groq
                www.SetRequestHeader("Authorization", "Bearer " + groqApiKey);
                
                // Envía la solicitud y espera la respuesta
                var operation = www.SendWebRequest();
                while (!operation.isDone)
                    await Task.Delay(10);
                
                // Manejo de errores HTTP
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("Error en la API: " + www.error);
                    Debug.LogError("Respuesta: " + www.downloadHandler.text);
                    return null;
                }
                
                // Procesa la respuesta JSON
                string jsonResponse = www.downloadHandler.text;
                Debug.Log("Respuesta API: " + jsonResponse);
                
                // Extrae el texto de la transcripción del JSON
                // Formato esperado: {"text":"la transcripción aquí"}
                TranscriptionResponse response = JsonUtility.FromJson<TranscriptionResponse>(jsonResponse);
                return response.text;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error en la transcripción: " + e.Message);
            return null;
        }
        finally
        {
            // Limpia archivo temporal
            string tempWavPath = Path.Combine(Application.temporaryCachePath, "temp_recording.wav");
            if (File.Exists(tempWavPath))
            {
                File.Delete(tempWavPath);
            }
        }
    }
    
    // Clase para deserializar la respuesta JSON
    [Serializable]
    private class TranscriptionResponse
    {
        public string text;
    }
}
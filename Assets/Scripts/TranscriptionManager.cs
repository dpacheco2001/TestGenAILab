using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using UnityEngine;
using System.Text;
using System.IO;

public class TranscriptionManager : MonoBehaviour {
    [SerializeField] private string apiKey = ""; // Tu API key de Groq
    [SerializeField] private string model = "whisper-large-v3-turbo";
    [SerializeField] private string language = "es";
    
    private readonly HttpClient httpClient = new HttpClient();
    
    private void Awake() {
        // Configurar el timeout del cliente HTTP
        httpClient.Timeout = TimeSpan.FromSeconds(30);
    }
    
    public async Task<string> TranscribeAudio(byte[] audioData) {
        if (string.IsNullOrEmpty(apiKey)) {
            Debug.LogError("API Key no configurada");
            return null;
        }
        
        try {
            Debug.Log($"Enviando {audioData.Length} bytes para transcripción...");
            
            // Crear el contenido multipart
            using (var content = new MultipartFormDataContent()) {
                // Agregar el archivo de audio
                var audioContent = new ByteArrayContent(audioData);
                audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                content.Add(audioContent, "file", "audio.wav");
                
                // Agregar los parámetros
                content.Add(new StringContent(model), "model");
                content.Add(new StringContent(language), "language");
                content.Add(new StringContent("json"), "response_format");
                
                // Configurar la solicitud
                using (var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/audio/transcriptions")) {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                    request.Content = content;
                    
                    // Enviar la solicitud
                    var response = await httpClient.SendAsync(request);
                    
                    // Verificar la respuesta
                    if (response.IsSuccessStatusCode) {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        Debug.Log($"Respuesta API: {jsonResponse}");
                        
                        // Parsear la respuesta JSON
                        GroqResponse groqResponse = JsonUtility.FromJson<GroqResponse>(jsonResponse);
                        return groqResponse.text;
                    } else {
                        string errorContent = await response.Content.ReadAsStringAsync();
                        Debug.LogError($"Error en API: {response.StatusCode}, {errorContent}");
                        return null;
                    }
                }
            }
        } catch (Exception ex) {
            Debug.LogError($"Error en la transcripción: {ex.Message}");
            return null;
        }
    }
}

[Serializable]
public class GroqResponse {
    public string text;
}
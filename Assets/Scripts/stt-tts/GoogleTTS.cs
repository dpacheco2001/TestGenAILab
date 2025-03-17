using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using System.Text;

public class GoogleCloudTTS : MonoBehaviour
{
    [SerializeField] private string apiKey = "YOUR_GOOGLE_CLOUD_API_KEY";
    [SerializeField] private string model = "es-ES-Wavenet-E";
    [SerializeField] private string languageCode = "es-ES";
    [SerializeField] private string textToSpeak = "Si prefieres una actualización automática, también puedes usar la herramienta GeForce Experience de NVIDIA.";
    [SerializeField] private AudioEncoding audioEncoding = AudioEncoding.MP3;
    
    // Reference to an AudioSource component to play the audio
    private AudioSource audioSource;

    // Enum to match Google's audio encoding options
    public enum AudioEncoding
    {
        MP3,
        LINEAR16,
        OGG_OPUS
    }

    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        Debug.Log("GoogleCloudTTS initialized. Press G key to generate speech.");
    }
    
    void Update()
    {
        // Press G key to generate speech
        if (Input.GetKeyDown(KeyCode.G))
        {
            Debug.Log("Generating speech with Google Cloud...");
            GenerateAndPlaySpeech();
        }
    }

    public void GenerateAndPlaySpeech()
    {
        StartCoroutine(GetTTS());
    }

    private IEnumerator GetTTS()
    {
        string url = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={apiKey}";
        
        // Convert our enum to Google's format
        string encodingStr = "";
        switch (audioEncoding)
        {
            case AudioEncoding.MP3:
                encodingStr = "MP3";
                break;
            case AudioEncoding.LINEAR16:
                encodingStr = "LINEAR16";
                break;
            case AudioEncoding.OGG_OPUS:
                encodingStr = "OGG_OPUS";
                break;
        }

        // Create request body based on Google Cloud TTS API format
        string jsonBody = "{\n" +
            "\"input\": {\n" +
            $"  \"text\": \"{textToSpeak}\"\n" +
            "},\n" +
            "\"voice\": {\n" +
            $"  \"languageCode\": \"{languageCode}\",\n" +
            $"  \"name\": \"{model}\"\n" +
            "},\n" +
            "\"audioConfig\": {\n" +
            $"  \"audioEncoding\": \"{encodingStr}\"\n" +
            "}\n" +
            "}";

        Debug.Log("Sending request to Google Cloud TTS...");
        Debug.Log($"Request body: {jsonBody}");
        
        // Create web request
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for response
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log("Response received successfully");
                
                // Parse JSON response to get audio content
                string jsonResponse = www.downloadHandler.text;
                GoogleTTSResponse response = JsonUtility.FromJson<GoogleTTSResponse>(jsonResponse);
                
                if (string.IsNullOrEmpty(response.audioContent))
                {
                    Debug.LogError("No audio content received in response");
                    yield break;
                }
                
                // Decode the base64 audio content
                byte[] audioBytes = Convert.FromBase64String(response.audioContent);
                Debug.Log($"Audio decoded successfully. Size: {audioBytes.Length} bytes");
                
                // Save audio to a temporary file and play it
                string tempFilePath = Path.Combine(Application.temporaryCachePath, "google_tts_audio.mp3");
                File.WriteAllBytes(tempFilePath, audioBytes);
                
                // Wait a frame to ensure file is written
                yield return null;
                
                // Load and play the audio
                StartCoroutine(LoadAndPlayAudio(tempFilePath));
            }
        }
    }

    private IEnumerator LoadAndPlayAudio(string filePath)
    {
        Debug.Log($"Loading audio from: {filePath}");
        
        AudioType audioType = AudioType.MPEG;
        if (audioEncoding == AudioEncoding.OGG_OPUS)
            audioType = AudioType.OGGVORBIS;
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, audioType))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error loading audio: {www.error}");
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                
                if (clip == null)
                {
                    Debug.LogError("Failed to create AudioClip");
                }
                else
                {
                    Debug.Log($"AudioClip created successfully. Length: {clip.length} seconds");
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("Playing audio from Google Cloud TTS");
                    
                    // Delete temp file after a delay to ensure it's not in use
                    StartCoroutine(DeleteFileWithDelay(filePath, clip.length + 1f));
                }
            }
        }
    }
    
    private IEnumerator DeleteFileWithDelay(string filePath, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"Deleted temporary file: {filePath}");
        }
    }

    public void SetText(string text)
    {
        textToSpeak = text;
    }
    
    // Class to deserialize the Google TTS response
    [Serializable]
    private class GoogleTTSResponse
    {
        public string audioContent;
    }
}
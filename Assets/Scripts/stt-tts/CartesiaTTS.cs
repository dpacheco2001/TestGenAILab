using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class CartesiaTTS : MonoBehaviour
{
    [SerializeField] private string apiKey = "YOUR_CARTESIA_API_KEY";
    [SerializeField] private string modelId = "sonic-2";
    [SerializeField] private string voiceId = "79743797-2087-422f-8dc7-86f9efca85f1";
    [SerializeField] private string language = "es";
    [SerializeField] private string textToSpeak = "Hola que tal me llamo Mateo y hoy seré tu asistente para este laboratorio";
    [SerializeField] private int bitRate = 128000;
    [SerializeField] private int sampleRate = 44100;
    
    // Reference to an AudioSource component to play the audio
    private AudioSource audioSource;

    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        Debug.Log("CartesiaTTS initialized. Press C key to generate speech.");
    }
    
    void Update()
    {
        // Press C key to generate speech
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Generating speech with Cartesia...");
            GenerateAndPlaySpeech();
        }
    }

    public void GenerateAndPlaySpeech()
    {
        StartCoroutine(GetTTS());
    }

    private IEnumerator GetTTS()
    {
        string url = "https://api.cartesia.ai/tts/bytes";

        // Create request body based on Cartesia API format
        string jsonBody = "{\n" +
            $"\"model_id\": \"{modelId}\",\n" +
            $"\"transcript\": \"{textToSpeak}\",\n" +
            "\"voice\": {\n" +
            "    \"model\": \"id\",\n" +
            $"    \"id\": \"{voiceId}\"\n" +
            "},\n" +
            "\"output_format\": {\n" +
            "    \"container\": \"mp3\",\n" +
            $"    \"bit_rate\": {bitRate},\n" +
            $"    \"sample_rate\": {sampleRate}\n" +
            "},\n" +
            $"\"language\": \"{language}\"\n" +
            "}";

        Debug.Log("Sending request to Cartesia...");
        Debug.Log($"Request body: {jsonBody}");
        
        // Create web request
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cartesia-Version", "2024-06-10");
            www.SetRequestHeader("X-API-Key", apiKey);
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
                Debug.Log($"Audio received successfully. Data size: {www.downloadHandler.data.Length} bytes");
                
                // Save audio to a temporary file
                string tempFilePath = Path.Combine(Application.temporaryCachePath, "cartesia_audio.mp3");
                File.WriteAllBytes(tempFilePath, www.downloadHandler.data);
                Debug.Log($"Audio saved to: {tempFilePath}");
                
                // Wait a frame to ensure file is written
                yield return null;
                
                // Start another coroutine to load and play the audio
                StartCoroutine(LoadAndPlayAudio(tempFilePath));
            }
        }
    }

    private IEnumerator LoadAndPlayAudio(string filePath)
    {
        Debug.Log($"Loading audio from: {filePath}");
        
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
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
                    Debug.Log("Playing audio from Cartesia");
                }
            }
        }
    }

    public void SetText(string text)
    {
        textToSpeak = text;
    }
}
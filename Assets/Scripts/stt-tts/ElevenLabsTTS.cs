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
    [SerializeField] public string textToSpeak = "Hola que tal me llamo Mateo y hoy seré tu asistente para este laboratorio";
    
    // Reference to an AudioSource component to play the audio
    private AudioSource audioSource;
    
    // Event to notify when speech generation is complete
    public event Action OnSpeechComplete;
    
    // Is speech currently being generated or played
    public bool IsSpeaking { get; private set; }

    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        Debug.Log("ElevenLabsTTS initialized.");
    }
    
    void Update()
    {
        // We can keep this for testing purposes
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Generating speech...");
            GenerateAndPlaySpeech();
        }
    }

    public void GenerateAndPlaySpeech()
    {
        StartCoroutine(GetTTS());
    }
    
    // New method to set text and generate speech in one call
    public void SpeakText(string text)
    {
        textToSpeak = text;
        GenerateAndPlaySpeech();
    }

    private IEnumerator GetTTS()
    {
        // Use the standard endpoint instead of streaming
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceID}";

        // Create request body
        string jsonBody = "{" +
            $"\"text\": \"{textToSpeak}\"," +
            $"\"model_id\": \"{modelID}\"" +
        "}";

        IsSpeaking = true;
        Debug.Log("Sending request to ElevenLabs...");
        
        // Create web request
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("xi-api-key", apiKey);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "audio/mpeg");  // Explicitly request MP3 format

            // Send the request and wait for response
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
                IsSpeaking = false;
                OnSpeechComplete?.Invoke();
            }
            else
            {
                Debug.Log($"Audio received successfully. Data size: {www.downloadHandler.data.Length} bytes");
                
                // Save audio to a temporary file
                string tempFilePath = Path.Combine(Application.temporaryCachePath, "tts_audio.mp3");
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
                IsSpeaking = false;
                OnSpeechComplete?.Invoke();
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                
                if (clip == null)
                {
                    Debug.LogError("Failed to create AudioClip");
                    IsSpeaking = false;
                    OnSpeechComplete?.Invoke();
                }
                else
                {
                    Debug.Log($"AudioClip created successfully. Length: {clip.length} seconds");
                    audioSource.clip = clip;
                    audioSource.Play();
                    Debug.Log("Playing audio");
                    
                    // Wait until audio playback completes
                    yield return new WaitForSeconds(clip.length);
                    
                    IsSpeaking = false;
                    OnSpeechComplete?.Invoke();
                }
            }
        }
    }
}
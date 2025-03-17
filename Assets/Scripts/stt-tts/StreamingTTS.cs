using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;

public class StreamingTTS : MonoBehaviour
{
    [SerializeField] private string apiKey = "YOUR_API_KEY";
    [SerializeField] private string modelId = "sonic-2";
    [SerializeField] private string voiceId = "79743797-2087-422f-8dc7-86f9efca85f1";
    [SerializeField] private string language = "es";
    [SerializeField] private string textToSpeak = "Hola que tal me llamo Mateo. Hoy seré tu asistente para este laboratorio. Espero que disfrutes la experiencia de audio por streaming.";
    
    private AudioSource audioSource;
    private Queue<AudioClip> audioQueue = new Queue<AudioClip>();
    private bool isPlaying = false;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        Debug.Log("StreamingTTS initialized. Press S key to generate speech.");
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Starting streaming TTS...");
            StartStreamingTTS();
        }
        
        // Check if we should play the next audio clip
        if (!audioSource.isPlaying && audioQueue.Count > 0 && isPlaying)
        {
            PlayNextClip();
        }
    }
    
    public void StartStreamingTTS()
    {
        // Clear any previous audio
        audioQueue.Clear();
        isPlaying = false;
        
        // Split text into sentences
        string[] sentences = SplitIntoSentences(textToSpeak);
        
        // Start generating audio for each sentence
        StartCoroutine(GenerateAudioForSentences(sentences));
    }
    
    private string[] SplitIntoSentences(string text)
    {
        // Simple sentence splitter (can be improved)
        return Regex.Split(text, @"(?<=[\.!\?])\s+");
    }
    
    private IEnumerator GenerateAudioForSentences(string[] sentences)
    {
        for (int i = 0; i < sentences.Length; i++)
        {
            string sentence = sentences[i];
            if (string.IsNullOrWhiteSpace(sentence))
                continue;
                
            Debug.Log($"Generating audio for sentence {i+1}/{sentences.Length}: {sentence}");
            
            yield return StartCoroutine(RequestAudioForText(sentence, i == 0));
            
            // Small delay between requests to avoid rate limiting
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    private IEnumerator RequestAudioForText(string text, bool isFirst)
    {
        string url = "https://api.cartesia.ai/tts/bytes";
        
        string jsonBody = "{\n" +
            $"\"model_id\": \"{modelId}\",\n" +
            $"\"transcript\": \"{text}\",\n" +
            "\"voice\": {\n" +
            "    \"model\": \"id\",\n" +
            $"    \"id\": \"{voiceId}\"\n" +
            "},\n" +
            "\"output_format\": {\n" +
            "    \"container\": \"mp3\",\n" +
            $"    \"bit_rate\": 128000,\n" +
            $"    \"sample_rate\": 44100\n" +
            "},\n" +
            $"\"language\": \"{language}\"\n" +
            "}";
        
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Cartesia-Version", "2024-06-10");
            www.SetRequestHeader("X-API-Key", apiKey);
            www.SetRequestHeader("Content-Type", "application/json");
            
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
            }
            else
            {
                string tempFilePath = Path.Combine(Application.temporaryCachePath, $"tts_chunk_{DateTime.Now.Ticks}.mp3");
                File.WriteAllBytes(tempFilePath, www.downloadHandler.data);
                
                yield return StartCoroutine(LoadAudioClip(tempFilePath, isFirst));
            }
        }
    }
    
    private IEnumerator LoadAudioClip(string filePath, bool playImmediately)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            
            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip != null)
                {
                    // Add to queue
                    audioQueue.Enqueue(clip);
                    Debug.Log($"Added clip to queue. Queue size: {audioQueue.Count}");
                    
                    // If this is the first clip or we're not currently playing, start playback
                    if (playImmediately && !isPlaying)
                    {
                        isPlaying = true;
                        PlayNextClip();
                    }
                }
            }
            else
            {
                Debug.LogError($"Error loading audio: {www.error}");
            }
        }
    }
    
    private void PlayNextClip()
    {
        if (audioQueue.Count > 0)
        {
            AudioClip nextClip = audioQueue.Dequeue();
            audioSource.clip = nextClip;
            audioSource.Play();
            Debug.Log($"Playing next clip. Remaining in queue: {audioQueue.Count}");
        }
        else
        {
            isPlaying = false;
            Debug.Log("Finished playing all audio clips");
        }
    }
    
    public void SetText(string text)
    {
        textToSpeak = text;
    }
}
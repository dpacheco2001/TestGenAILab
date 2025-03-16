using UnityEngine;
using System.IO;
using System.Threading.Tasks;

public class AudioRecorder : MonoBehaviour {
    public AudioClip recordedClip;
    private bool isRecording = false;
    private string device;
    public int sampleRate = 16000; // 16kHz is optimal for Whisper
    public float maxRecordingTime = 30f;
    
    [Header("Optimization Settings")]
    [SerializeField] private bool useVoiceDetection = true;
    [SerializeField] private float voiceThreshold = 0.02f;
    [SerializeField] private float autoStopSilenceTime = 1.5f; // Stop after 1.5s of silence
    
    private float silenceTimer = 0f;
    private bool hasDetectedVoice = false;
    
    void Start() {
        if (Microphone.devices.Length > 0) {
            device = Microphone.devices[0];
            Debug.Log($"Micrófono seleccionado: {device}");
        } else {
            Debug.LogError("No se encontró dispositivo de grabación");
        }
    }
    
    void Update() {
        if (isRecording && useVoiceDetection) {
            float level = GetCurrentAudioLevel();
            
            // Check for voice activity
            if (level > voiceThreshold) {
                silenceTimer = 0f;
                if (!hasDetectedVoice) {
                    hasDetectedVoice = true;
                    Debug.Log("Voice detected");
                }
            }
            // If we've detected voice before and now have silence
            else if (hasDetectedVoice) {
                silenceTimer += Time.deltaTime;
                
                // Auto-stop after silence threshold
                if (silenceTimer >= autoStopSilenceTime) {
                    Debug.Log($"Auto-stopping after {autoStopSilenceTime}s of silence");
                    StopRecording();
                }
            }
        }
    }

    public void StartRecording() {
        if (!isRecording && device != null) {
            // Reset voice detection state
            hasDetectedVoice = false;
            silenceTimer = 0f;
            
            recordedClip = Microphone.Start(device, false, Mathf.FloorToInt(maxRecordingTime), sampleRate);
            isRecording = true;
            Debug.Log("Grabación iniciada");
        }
    }

    public void StopRecording() {
        if (isRecording) {
            Microphone.End(device);
            isRecording = false;
            Debug.Log("Grabación detenida");
        }
    }

    public async Task<byte[]> GetWavDataAsync() {
        if (recordedClip == null) return null;
        
        // Process audio on background thread
        return await Task.Run(() => {
            return ConvertAndOptimizeAudioClip(recordedClip);
        });
    }
    
    public byte[] GetWavData() {
        if (recordedClip == null) return null;
        return ConvertAndOptimizeAudioClip(recordedClip);
    }
    
    private byte[] ConvertAndOptimizeAudioClip(AudioClip clip) {
        if (clip == null) return null;
        
        // Get raw data
        float[] data = new float[clip.samples * clip.channels];
        clip.GetData(data, 0);
        
        int trimStart = 0;
        int trimEnd = data.Length - 1;
        
        // Find actual audio content (trim silence)
        if (useVoiceDetection) {
            float threshold = voiceThreshold * 0.75f; // Slightly lower threshold for trimming
            
            // Find start (first non-silent sample)
            for (; trimStart < data.Length; trimStart++) {
                if (Mathf.Abs(data[trimStart]) > threshold)
                    break;
            }
            
            // Find end (last non-silent sample)
            for (; trimEnd >= 0; trimEnd--) {
                if (Mathf.Abs(data[trimEnd]) > threshold)
                    break;
            }
            
            // Ensure we have valid indices
            if (trimEnd <= trimStart) {
                // No clear audio found, use the full clip
                trimStart = 0;
                trimEnd = data.Length - 1;
            }
        }
        
        // Optimize: Ensure we don't have too short of a clip
        int minSamples = clip.frequency / 10; // At least 100ms
        if (trimEnd - trimStart < minSamples) {
            // Extend to ensure minimum size
            trimStart = Mathf.Max(0, trimStart - minSamples/2);
            trimEnd = Mathf.Min(data.Length - 1, trimEnd + minSamples/2);
        }
        
        // Prepare trimmed data if needed
        float[] processedData;
        if (trimStart > 0 || trimEnd < data.Length - 1) {
            int newLength = trimEnd - trimStart + 1;
            processedData = new float[newLength];
            System.Array.Copy(data, trimStart, processedData, 0, newLength);
            Debug.Log($"Audio trimmed from {data.Length} to {newLength} samples");
        } else {
            processedData = data;
        }
        
        // Convert to WAV
        using (MemoryStream stream = new MemoryStream()) {
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                // RIFF header
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + processedData.Length * 2); // File size
                writer.Write(new char[] { 'W', 'A', 'V', 'E' });
                
                // Format chunk
                writer.Write(new char[] { 'f', 'm', 't', ' ' });
                writer.Write(16); // Chunk size
                writer.Write((ushort)1); // Audio format (1 for PCM)
                writer.Write((ushort)clip.channels); // Channels
                writer.Write(clip.frequency); // Sample rate
                writer.Write(clip.frequency * clip.channels * 2); // Byte rate
                writer.Write((ushort)(clip.channels * 2)); // Block align
                writer.Write((ushort)16); // Bits per sample
                
                // Data chunk
                writer.Write(new char[] { 'd', 'a', 't', 'a' });
                writer.Write(processedData.Length * 2); // Chunk size
                
                // Convert float samples to 16-bit integers
                for (int i = 0; i < processedData.Length; i++) {
                    writer.Write((short)(processedData[i] * 32767));
                }
            }
            
            return stream.ToArray();
        }
    }
    
    public float GetCurrentAudioLevel() {
        if (!isRecording) return 0;
        
        int pos = Microphone.GetPosition(device);
        float[] samples = new float[128];
        
        if (pos >= 128 && recordedClip != null) {
            recordedClip.GetData(samples, pos - 128);
            float sum = 0;
            for (int i = 0; i < samples.Length; i++) {
                sum += Mathf.Abs(samples[i]);
            }
            
            return sum / samples.Length;
        }
        
        return 0;
    }
}
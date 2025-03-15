using UnityEngine;
using System.IO;

public class AudioRecorder : MonoBehaviour {
    public AudioClip recordedClip;
    private bool isRecording = false;
    private string device;
    public int sampleRate = 16000;
    public float maxRecordingTime = 30f; // Max recording time in seconds

    void Start() {
        if (Microphone.devices.Length > 0) {
            device = Microphone.devices[0];
            Debug.Log($"Micrófono seleccionado: {device}");
        } else {
            Debug.LogError("No se encontró dispositivo de grabación");
        }
    }

    public void StartRecording() {
        if (!isRecording && device != null) {
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

    public byte[] GetWavData() {
        if (recordedClip == null) return null;
        
        // Convierte el AudioClip a datos WAV
        return ConvertAudioClipToWav(recordedClip);
    }
    
    private byte[] ConvertAudioClipToWav(AudioClip clip) {
        if (clip == null) return null;
        
        float[] data = new float[clip.samples * clip.channels];
        clip.GetData(data, 0);
        
        using (MemoryStream stream = new MemoryStream()) {
            using (BinaryWriter writer = new BinaryWriter(stream)) {
                // RIFF header
                writer.Write(new char[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + clip.samples * clip.channels * 2); // File size
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
                writer.Write(clip.samples * clip.channels * 2); // Chunk size
                
                // Convert float samples to 16-bit integers
                for (int i = 0; i < data.Length; i++) {
                    writer.Write((short)(data[i] * 32767));
                }
            }
            
            return stream.ToArray();
        }
    }
    
    // Devuelve el nivel de audio actual (para visualización)
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public class AudioFolderWatcher : MonoBehaviour
{
    // Ruta de la carpeta en la que se guardan los audios (usamos la carpeta temporal de Unity)
    private string folderPath;
    // Referencia al AudioSource para reproducir los audios
    private AudioSource audioSource;
    // Contador para identificar el siguiente archivo a reproducir
    private int currentIndex = 0;

    void Start()
    {
        folderPath = Application.temporaryCachePath;
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Si no hay un AudioSource, se agrega uno.
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        // Inicia la corrutina que vigila la carpeta y reproduce audios.
        StartCoroutine(WatchAndPlayAudios());
    }

    /// <summary>
    /// Corrutina que verifica de forma continua la existencia de archivos de audio con nombres consecutivos.
    /// </summary>
    IEnumerator WatchAndPlayAudios()
    {
        while (true)
        {
            // Construye la ruta del archivo esperado.
            string filePath = Path.Combine(folderPath, $"tts_audio_{currentIndex}.mp3");
            if (File.Exists(filePath))
            {
                Debug.Log($"Se encontró el archivo: {filePath}. Cargando y reproduciendo...");
                // Se carga y reproduce el audio
                yield return StartCoroutine(LoadAndPlayAudio(filePath));
                // Se pasa al siguiente archivo
                currentIndex++;
            }
            else
            {
                // Si no se encuentra el siguiente archivo y se han reproducido algunos,
                // se asume que ya se completó la secuencia.
                if (currentIndex > 0)
                {
                    Debug.Log("No se encontró el siguiente archivo. Se procede a limpiar los audios reproducidos.");
                    CleanupPlayedAudios();
                    currentIndex = 0;
                }
                // Espera un segundo antes de volver a revisar.
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Carga el audio de la ruta especificada y lo reproduce.
    /// </summary>
    IEnumerator LoadAndPlayAudio(string filePath)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error al cargar el audio desde {filePath}: {www.error}");
                yield break;
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip == null)
                {
                    Debug.LogError("Error: No se pudo crear el AudioClip desde " + filePath);
                    yield break;
                }
                audioSource.clip = clip;
                audioSource.Play();
                Debug.Log($"Reproduciendo audio: {filePath}");
                // Espera a que termine la reproducción.
                yield return new WaitWhile(() => audioSource.isPlaying);
            }
        }
    }

    /// <summary>
    /// Elimina todos los archivos de audio reproducidos en la secuencia.
    /// </summary>
    void CleanupPlayedAudios()
    {
        for (int i = 0; i < currentIndex; i++)
        {
            string filePath = Path.Combine(folderPath, $"tts_audio_{i}.mp3");
            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    Debug.Log($"Se eliminó el archivo: {filePath}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error al eliminar el archivo {filePath}: {ex.Message}");
                }
            }
        }
    }
}

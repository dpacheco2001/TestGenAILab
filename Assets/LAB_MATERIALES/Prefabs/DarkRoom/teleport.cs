using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Collections;

public class TeleporterWithVideoSequence : MonoBehaviour
{
    public Transform destinationPoint;
    public Transform objectToTeleport;
    public Transform playerController;
    public VideoPlayer videoPlayer;
    public VideoClip singleVideo; // Un solo video en lugar de lista
    
    [Header("GameObjects to Toggle")]
    public GameObject gameObjectToToggle1;
    public GameObject gameObjectToToggle2;
    
    [Header("Timing Settings")]
    public float videoDelay = 2f; // Delay en segundos antes de reproducir el video
    public bool shouldTeleportAndPlay = false;
    public bool forceReturnAndStop = false;
    public UnityEvent onVideoSequenceFinished;

    private Transform targetTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 playerOriginalPosition;
    private Quaternion playerOriginalRotation;
    private bool hasValidPlayerToTrack;
    private bool hasSavedOriginalState;
    private bool isAtDestination;
    private int currentVideoIndex;

    void Awake()
    {
        hasValidPlayerToTrack = playerController != null;
        if (videoPlayer != null)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.loopPointReached += OnVideoEnd;
        }
    }

    void Start()
    {
        targetTransform = objectToTeleport != null ? objectToTeleport : transform;
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoEnd;
    }

    void Update()
    {
        if (shouldTeleportAndPlay && !isAtDestination)
        {
            TeleportAndPlay();
            shouldTeleportAndPlay = false;
        }
        if (forceReturnAndStop)
        {
            ReturnToOriginalPosition(true);
            forceReturnAndStop = false;
        }
    }

    public void TeleportAndPlay()
    {
        // Guardar posiciones originales si no se han guardado
        if (!hasSavedOriginalState)
        {
            originalPosition = targetTransform.position;
            originalRotation = targetTransform.rotation;
            if (hasValidPlayerToTrack)
            {
                playerOriginalPosition = playerController.position;
                playerOriginalRotation = playerController.rotation;
            }
            hasSavedOriginalState = true;
        }
        
        // Iniciar corrutina para el delay completo (teletransporte + video)
        StartCoroutine(TeleportAndPlayAfterDelay());
    }
    
    private IEnumerator TeleportAndPlayAfterDelay()
    {
        // Esperar el delay antes de hacer CUALQUIER cosa
        yield return new WaitForSeconds(videoDelay);
        
        // Desactivar los GameObjects antes del teletransporte
        if (gameObjectToToggle1 != null)
            gameObjectToToggle1.SetActive(false);
        if (gameObjectToToggle2 != null)
            gameObjectToToggle2.SetActive(false);
        
        // Ahora hacer el teletransporte
        targetTransform.position = destinationPoint.position;
        targetTransform.rotation = destinationPoint.rotation;
        if (hasValidPlayerToTrack)
            playerController.position = destinationPoint.position;
        
        isAtDestination = true;
        
        // Y reproducir el video inmediatamente después del teletransporte
        if (singleVideo != null)
            PlaySingleVideo();
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (vp == videoPlayer)
        {
            // Cuando termina el video único, regresar automáticamente
            onVideoSequenceFinished.Invoke();
            ReturnToOriginalPosition(true);
        }
    }

    void PlaySingleVideo()
    {
        videoPlayer.clip = singleVideo;
        videoPlayer.Stop();
        videoPlayer.Play();
    }

    public void ReturnToOriginalPosition(bool stopVideo = true)
    {
        if (isAtDestination)
        {
            if (stopVideo && videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
                // NO limpiamos el clip para que no se deseleccione
                onVideoSequenceFinished.Invoke();
            }
            targetTransform.position = originalPosition;
            targetTransform.rotation = originalRotation;
            if (hasValidPlayerToTrack)
            {
                playerController.position = playerOriginalPosition;
                playerController.rotation = playerOriginalRotation;
            }
            
            // Reactivar los GameObjects cuando se regresa
            if (gameObjectToToggle1 != null)
                gameObjectToToggle1.SetActive(true);
            if (gameObjectToToggle2 != null)
                gameObjectToToggle2.SetActive(true);
            
            isAtDestination = false;
        }
    }
    
    // Función sin argumentos para usar en botones de Unity
    public void ReturnToOriginal()
    {
        ReturnToOriginalPosition(true);
    }
}

using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using System.Collections.Generic;

public class TeleporterWithVideoSequence : MonoBehaviour
{
    public Transform destinationPoint;
    public Transform objectToTeleport;
    public Transform playerController;
    public VideoPlayer videoPlayer;
    public List<VideoClip> videoSequence;
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

    void TeleportAndPlay()
    {
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
        targetTransform.position = destinationPoint.position;
        targetTransform.rotation = destinationPoint.rotation;
        if (hasValidPlayerToTrack)
            playerController.position = destinationPoint.position;
        currentVideoIndex = 0;
        if (videoSequence != null && videoSequence.Count > 0)
            PlayVideo(currentVideoIndex);
        isAtDestination = true;
    }

    void OnVideoEnd(VideoPlayer vp)
    {
        if (vp == videoPlayer)
        {
            currentVideoIndex++;
            if (videoSequence != null && currentVideoIndex < videoSequence.Count)
                PlayVideo(currentVideoIndex);
            else
            {
                onVideoSequenceFinished.Invoke();
                ReturnToOriginalPosition(true);
            }
        }
    }

    void PlayVideo(int index)
    {
        videoPlayer.clip = videoSequence[index];
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
                videoPlayer.clip = null;
                onVideoSequenceFinished.Invoke();
            }
            targetTransform.position = originalPosition;
            targetTransform.rotation = originalRotation;
            if (hasValidPlayerToTrack)
            {
                playerController.position = playerOriginalPosition;
                playerController.rotation = playerOriginalRotation;
            }
            isAtDestination = false;
            currentVideoIndex = -1;
        }
    }
}

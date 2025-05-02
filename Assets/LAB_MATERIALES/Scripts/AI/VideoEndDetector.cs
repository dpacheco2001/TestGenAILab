using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

public class VideoEndDetector : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public UnityEvent onVideoFinished;

    void Reset()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    void OnEnable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached += HandleVideoEnd;
    }

    void OnDisable()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= HandleVideoEnd;
    }

    void HandleVideoEnd(VideoPlayer vp)
    {
        onVideoFinished.Invoke();
    }
}

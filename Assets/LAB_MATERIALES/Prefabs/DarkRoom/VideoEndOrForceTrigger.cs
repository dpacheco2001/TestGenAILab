using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;

public class VideoEndOrForceTrigger : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public UnityEvent onFinished;
    public bool forceFinish;

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

    void Update()
    {
        if (forceFinish)
        {
            TriggerFinished();
            forceFinish = false;
        }
    }

    void HandleVideoEnd(VideoPlayer vp)
    {
        TriggerFinished();
    }

    public void ForceFinish()
    {
        TriggerFinished();
    }

    void TriggerFinished()
    {
        onFinished.Invoke();
    }
}

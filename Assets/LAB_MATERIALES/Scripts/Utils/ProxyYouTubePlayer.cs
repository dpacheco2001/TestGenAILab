using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class ProxyYouTubePlayer : MonoBehaviour
{
    [Header("VideoPlayer Component")]
    public VideoPlayer videoPlayer;

    public int port = 8070; 

    [Header("YouTube URL")]
    [Tooltip("Ej: https://www.youtube.com/watch?v=zYEKeEOKNi0")]
    public string youTubeUrl;

    void Start()
    {
        string proxyHost = "127.0.0.1";  
        string proxyUrl  = $"http://{proxyHost}:{port}/stream?url=" 
                        + UnityWebRequest.EscapeURL(youTubeUrl);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url    = proxyUrl;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += vp => vp.Play();
    }
}

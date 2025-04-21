using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class VideoViewportContent : MonoBehaviour
{
    [Header("Video Components")]
    public VideoPlayer videoPlayer;
    public RawImage    videoDisplay;
    public AudioSource audioSource;  

    [Header("Thumbnail (Preview)")]
    public Image       thumbnailImage;

    [Header("Playback Controls")]
    public Button      playPauseButton;
    public Sprite      playIcon;
    public Sprite      pauseIcon;
    public Slider      progressSlider;
    public TMP_Text    currentTimeText;
    public TMP_Text    durationText;

    [Header("Loading UI")]
    public GameObject  loadingPanel;
    public float       spinnerSpeed = 180f;

    [Header("Metadata UI")]
    public TMP_Text    header;
    public TMP_Text    subHeader;
    public TMP_Text    description;

    [Header("Back Button")]
    public Button      backButton;

    bool   _isDragging    = false;
    bool   _startedStream = false;
    string _originalUrl;


    public void Setup(ResourceUIManager.ResourceData data)
    {
        header.text      = data.header;
        subHeader.text   = data.subHeader;
        description.text = data.description;

        thumbnailImage.sprite = data.thumbnail;
        thumbnailImage.gameObject.SetActive(true);
        videoDisplay.gameObject.SetActive(false);
        loadingPanel.SetActive(false);
        backButton.gameObject.SetActive(false);


        videoPlayer.source          = VideoSource.Url;
        videoPlayer.renderMode      = VideoRenderMode.APIOnly;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.errorReceived    += OnError;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;

   
        if (audioSource != null)
            videoPlayer.SetTargetAudioSource(0, audioSource);

        _originalUrl = data.videoUrl;

        playPauseButton.onClick.RemoveAllListeners();
        playPauseButton.onClick.AddListener(OnPlayPauseButtonClicked);
        playPauseButton.image.sprite = playIcon;
        playPauseButton.interactable  = true;

        progressSlider.onValueChanged.RemoveAllListeners();
        progressSlider.onValueChanged.AddListener(OnSliderValueChanged);
        progressSlider.minValue = 0;
        progressSlider.value    = 0;

  
        var trigger = progressSlider.GetComponent<EventTrigger>()
                      ?? progressSlider.gameObject.AddComponent<EventTrigger>();
        trigger.triggers = new List<EventTrigger.Entry>();

        var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        down.callback.AddListener(_ => _isDragging = true);
        trigger.triggers.Add(down);

        var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        up.callback.AddListener(_ => {
            _isDragging = false;
            if (videoPlayer.isPrepared)
                videoPlayer.time = progressSlider.value;
        });
        trigger.triggers.Add(up);

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(OnBackButton);

        currentTimeText.text = "00:00";
        durationText.text    = "00:00";

        _isDragging    = false;
        _startedStream = false;
    }

    void Update()
    {
        if (loadingPanel.activeSelf)
            loadingPanel.transform.Rotate(0, 0, -spinnerSpeed * Time.deltaTime);

        if (_startedStream && videoPlayer.isPrepared && videoPlayer.isPlaying && !_isDragging)
        {
            float t = (float)videoPlayer.time;
            
            progressSlider.SetValueWithoutNotify(t);
            currentTimeText.text = FormatTime(t);
        }
    }

    void OnPlayPauseButtonClicked()
    {
        if (!_startedStream)
        {
            StartStream();
            return;
        }

        if (!videoPlayer.isPrepared) return;

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            playPauseButton.image.sprite = playIcon;
        }
        else
        {
            videoPlayer.Play();
            playPauseButton.image.sprite = pauseIcon;
        }
    }

    void StartStream()
    {
        _startedStream = true;
        loadingPanel.SetActive(true);
        playPauseButton.interactable = false;

        string proxyUrl = "http://127.0.0.1:8070/stream?url="
                        + UnityEngine.Networking.UnityWebRequest.EscapeURL(_originalUrl);
        videoPlayer.url = proxyUrl;
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        loadingPanel.SetActive(false);
        thumbnailImage.gameObject.SetActive(false);
        videoDisplay.gameObject.SetActive(true);

        videoDisplay.texture = vp.texture;
        var fitter = videoDisplay.GetComponent<AspectRatioFitter>()
                    ?? videoDisplay.gameObject.AddComponent<AspectRatioFitter>();
        fitter.aspectMode  = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = (float)vp.texture.width / vp.texture.height;

        backButton.gameObject.SetActive(true);

        double dur = vp.length;
        durationText.text       = FormatTime(dur);
        progressSlider.maxValue = (float)dur;

        videoPlayer.Play();
        playPauseButton.image.sprite = pauseIcon;
        playPauseButton.interactable  = true;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        playPauseButton.image.sprite = playIcon;
        progressSlider.SetValueWithoutNotify(progressSlider.maxValue);
        currentTimeText.text         = durationText.text;
    }

    void OnSliderValueChanged(float val)
    {
        if (videoPlayer.isPrepared)
        {
            videoPlayer.time = val;
            currentTimeText.text = FormatTime(val);
        }
    }

    void OnBackButton()
    {
        Debug.Log("Atrás");
        videoPlayer.Stop();
        _startedStream = false;

        videoDisplay.gameObject.SetActive(false);
        backButton.gameObject.SetActive(false);
        thumbnailImage.gameObject.SetActive(true);

        playPauseButton.image.sprite = playIcon;
        progressSlider.SetValueWithoutNotify(0);
        currentTimeText.text = "00:00";
    }

    void OnError(VideoPlayer vp, string msg)
    {
        Debug.LogError($"VideoPlayer error: {msg}");
    }

    string FormatTime(double s)
    {
        int m = (int)(s / 60);
        int ss = (int)(s % 60);
        return $"{m:00}:{ss:00}";
    }
}

using UnityEngine;

public class send_flag : MonoBehaviour
{
    public SpeechAssistantControllerWS speechAssistantControllerWS;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speechAssistantControllerWS = FindAnyObjectByType<SpeechAssistantControllerWS>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void send_flag_to_server(string flag){
        speechAssistantControllerWS.SendTranscriptionToWebSocket(flag);
    }
}

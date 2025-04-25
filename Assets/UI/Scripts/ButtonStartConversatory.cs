using UnityEngine;
using TMPro;

public class ButtonStartConversatory : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public TMP_Text nameText;
    private SpeechAssistantControllerWS speechAssistantControllerWS;

    void Start()
    {
        speechAssistantControllerWS = FindFirstObjectByType<SpeechAssistantControllerWS>();
        if (nameText == null)
        {
            Debug.LogError("nameText is not assigned in the inspector.");
        }
    }

    public void startConversatory(){
        //[INICIAR_CONVERSATORIO:INSPECCIÓN VISUAL DE UNA PIEZA FRACTURADA]
        string input = "[INICIAR_CONVERSATORIO:"+nameText.text+"]";
        speechAssistantControllerWS.SendTranscriptionToWebSocket(input);
    }
}

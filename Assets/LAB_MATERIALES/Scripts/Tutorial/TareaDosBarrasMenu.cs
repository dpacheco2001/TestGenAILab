using UnityEngine;

public class TareaDosBarrasMenu : MonoBehaviour
{
    public bool isCompleted2B = false;
    public bool isCompletedVerEnsayos = false;
    private SpeechAssistantControllerWS speechAssistantController;
    void Start()
    {
        speechAssistantController = FindFirstObjectByType<SpeechAssistantControllerWS>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void completarTareaDosBarrasMenu(){
        if(!isCompleted2B){
            isCompleted2B = true;
            speechAssistantController.SendTranscriptionToWebSocket("*El usuario ha interactuado satisfactoria mente con algún botón de la barra lateral y de la barra superior*");
        }
    }

    public void completarTareaVerEnsayos(){
        if(!isCompletedVerEnsayos){
            isCompletedVerEnsayos = true;
            speechAssistantController.SendTranscriptionToWebSocket("*El usuario ha interactuado satisfactoria mente con el botón de ver ensayos*");
        }

    }


}

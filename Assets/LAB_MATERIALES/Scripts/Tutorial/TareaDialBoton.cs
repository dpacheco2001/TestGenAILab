using UnityEngine;

public class TareaDialBoton : MonoBehaviour
{
    public bool botonApretado = false;
    public bool dialGirado = false;
    private SpeechAssistantControllerWS _speechAssistantControllerWS;
    void Start()
    {
        _speechAssistantControllerWS = FindObjectOfType<SpeechAssistantControllerWS>();
        if (_speechAssistantControllerWS == null)
        {
            Debug.LogError("No se encontró SpeechAssistantControllerWS en la escena.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void seApretoBoton()
    {
        if(!botonApretado){
            botonApretado = true;
            if(dialGirado){
                _speechAssistantControllerWS.SendTranscriptionToWebSocket("*El usuario ha apretado el botón y girado el dial exitosamente*");
            }
        }
    }

    public void seGiroDial()
    {
        if(!dialGirado){
            dialGirado = true;
            if(botonApretado){
                _speechAssistantControllerWS.SendTranscriptionToWebSocket("*El usuario ha apretado el botón y girado el dial exitosamente*");
            }
        }
    }
}

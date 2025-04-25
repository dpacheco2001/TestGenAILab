using UnityEngine;

public class TutorialAM : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private bool has_activated_zoom_dial = false;
    private bool has_activated_x_dial = false;
    private bool has_activated_y_dial = false;

    private bool has_put_on_platform = false;

    private SpeechAssistantControllerWS speechAssistantControllerWSS;


    void Start()
    {
        speechAssistantControllerWSS = FindFirstObjectByType<SpeechAssistantControllerWS>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateZoomDial()
    {
        if (!has_activated_zoom_dial)
        {
            has_activated_zoom_dial = true;
            // Aquí puedes agregar el código para activar el zoom del dial
            speechAssistantControllerWSS.SendTranscriptionToWebSocket("[El usuario ha usado correctamente el dial de zoom]");
        }
    }
    public void ActivateXDial()
    {
        if (!has_activated_x_dial)
        {
            has_activated_x_dial = true;
        }

        if(has_activated_y_dial){
            speechAssistantControllerWSS.SendTranscriptionToWebSocket("[El usuario ha usado correctamente el dial de X y el dial de Y]");
        }

    }
    public void ActivateYDial()
    {
        if (!has_activated_y_dial)
        {
            has_activated_y_dial = true;
        }

        if(has_activated_x_dial){
            speechAssistantControllerWSS.SendTranscriptionToWebSocket("[El usuario ha usado correctamente el dial de X y el dial de Y]");
        }
    }

    public void PutOnPlatform()
    {
        if (!has_put_on_platform)
        {
            has_put_on_platform = true;
            speechAssistantControllerWSS.SendTranscriptionToWebSocket("[El usuario ha puesto correctamente el objeto en la plataforma]");
        }
}
}

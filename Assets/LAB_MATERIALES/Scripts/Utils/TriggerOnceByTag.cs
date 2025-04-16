using UnityEngine;

public class TriggerOnceByTag : MonoBehaviour
{
    [Header("Tag que debe tener el objeto para activar el trigger")]
    public string requiredTag = "Cabeza"; 

    private bool hasTriggered = false;
    public string lugar_que_llego="Inspección Visual";
    private HandlerFlags handlerFlags;
    private SpeechAssistantControllerWS speechAssistantController;
    public string marcarDesactivado = "GUIA_IV";
    private void Start()
    {
        handlerFlags = FindFirstObjectByType<HandlerFlags>();
        speechAssistantController = FindFirstObjectByType<SpeechAssistantControllerWS>();
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (hasTriggered)
            return;

      
        if (other.CompareTag(requiredTag))
        {
            Debug.Log("OnTriggerEnter activado por objeto con tag: " + requiredTag + ". Objeto: " + other.name);
            
            handlerFlags.MarcarComoDesactivado(marcarDesactivado);
            speechAssistantController.SendTranscriptionToWebSocket("*El usuario ha llegado satisfactoriamenta a la zona de " + lugar_que_llego + "*");

            hasTriggered = true;
        }
    }
}

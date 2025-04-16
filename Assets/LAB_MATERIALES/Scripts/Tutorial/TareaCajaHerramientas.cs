using UnityEngine;
using System.Collections.Generic;

public class TareaCajaHerramientas : MonoBehaviour
{
    public string targetTag = "Herramienta";
    public ToolCallsRepository toolCallsRepository;

    public int counter = 0;
    public int cantidadHerramientas = 2; 
    private SpeechAssistantControllerWS speechAssistantController; 
    private HandlerFlags handlerFlags;
    public bool isCompleted = false; 

    // HashSet para almacenar los nombres de las herramientas que ya se contabilizaron
    private HashSet<string> countedTools = new HashSet<string>();

    void Start()
    {
        toolCallsRepository = FindFirstObjectByType<ToolCallsRepository>();
        speechAssistantController = FindFirstObjectByType<SpeechAssistantControllerWS>();
        handlerFlags = FindFirstObjectByType<HandlerFlags>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            // Verifica si la herramienta ya fue contada (basado en el nombre del objeto)
            if (countedTools.Contains(other.gameObject.name))
            {
                Debug.Log("La herramienta " + other.gameObject.name + " ya fue contada.");
                return;
            }

            // Si no está en el HashSet, la agregamos y aumentamos el contador
            countedTools.Add(other.gameObject.name);
            counter++;
            Debug.Log("Objeto con tag " + targetTag + " detectado. Contador: " + counter);
            
            if (counter >= cantidadHerramientas && !isCompleted)
            {
                speechAssistantController.SendTranscriptionToWebSocket("*El usuario ha puesto satisfactoriamente todas las herramientas en la caja.*");
                toolCallsRepository.martillo_0.enabled = false;
                toolCallsRepository.martillo_1.enabled = false;
                toolCallsRepository.destornillador_0.enabled = false;
                toolCallsRepository.destornillador_1.enabled = false;
                toolCallsRepository.destornillador_2.enabled = false;
                handlerFlags.MarcarComoDesactivado("PERSONA_HERRAMIENTAS");
                handlerFlags.MarcarComoDesactivado("MANDO_HERRAMIENTAS");
                
                
                isCompleted = true;
            }
        }
    }
}

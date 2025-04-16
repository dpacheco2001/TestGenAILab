using UnityEngine;

public class TareaCajaHerramientas : MonoBehaviour
{

    public string targetTag = "Herramienta";
    public ToolCallsRepository toolCallsRepository;


    public int counter = 0;
    public int cantidadHerramientas = 2; 
    private SpeechAssistantControllerWS speechAssistantController; 

    public bool isCompleted = false; 

    void Start()
    {
        
        toolCallsRepository = FindObjectOfType<ToolCallsRepository>();
        speechAssistantController = FindObjectOfType<SpeechAssistantControllerWS>();
    }
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag(targetTag))
        {
            counter++;
            Debug.Log("Objeto con tag " + targetTag + " detectado. Contador: " + counter);
            if(counter >= cantidadHerramientas && !isCompleted)
            {
                
                speechAssistantController.SendTranscriptionToWebSocket("*El usuario ha puesto satisfactoriamente todas las herramientas en la caja.*");
                toolCallsRepository.martillo_0.enabled = false;
                toolCallsRepository.martillo_1.enabled = false;
                toolCallsRepository.destornillador_0.enabled = false;
                toolCallsRepository.destornillador_1.enabled = false;
                toolCallsRepository.destornillador_2.enabled = false;
            }
        }
    }   

    

}

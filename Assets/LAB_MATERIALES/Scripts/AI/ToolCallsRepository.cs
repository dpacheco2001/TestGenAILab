using System;
using System.Reflection;
using UnityEngine;

public class ToolCallsRepository : MonoBehaviour
{
    // Patrón Singleton (opcional) para acceder fácilmente a esta instancia
    public static ToolCallsRepository Instance { get; private set; }
    public HandlerFlags handlerFlags; 
    public SpeechAssistantControllerWS speechAssistantControllerWSS;

    [Header("Tutorial")]
    public CheckVisibleObjectsHotZone checkVisibleObjectsHotZone; 
    public Outline martillo_0;
    public Outline martillo_1;
    public Outline destornillador_0;
    public Outline destornillador_1;
    public Outline destornillador_2;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Opcional: conservar este objeto al cambiar de escena
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        handlerFlags = FindAnyObjectByType<HandlerFlags>();
        checkVisibleObjectsHotZone = FindAnyObjectByType<CheckVisibleObjectsHotZone>();
        speechAssistantControllerWSS = FindAnyObjectByType<SpeechAssistantControllerWS>();
    }

    
    /// <summary>
    /// Busca e invoca el método que corresponda al toolcall recibido.
    /// </summary>
    /// <param name="toolCallName">Nombre del toolcall</param>
    public void InvokeToolCall(string toolCallName)
    {
        // Buscamos el método en esta clase (puede ser público o privado)
        MethodInfo method = GetType().GetMethod(toolCallName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method != null)
        {
            // Se invoca el método sin argumentos, asumimos que la firma es void MiMetodo()
            method.Invoke(this, null);
            Debug.LogError($"ToolCall ejecutado: {toolCallName}");
        }
        else
        {
            Debug.LogWarning($"No se encontró el método para toolcall: {toolCallName}");
        }
    }

    // Ejemplo de método toolcall
    private void mostrar_tutorial_mover_cabeza()
    {
        Debug.LogError("Ejecutando el tutorial para mover la cabeza.");
        handlerFlags.ActivarObjeto("TUTORIAL_MOVER_CABEZA");
        checkVisibleObjectsHotZone.enabled = true;
        checkVisibleObjectsHotZone.showHotZone = true; // Habilitar la visualización de la hotzone
    }

    private void mostrar_boton_asistente(){
        Debug.LogError("Ejecutando el tutorial para mostrar el botón del asistente.");
        handlerFlags.ActivarObjeto("BOTON_ASISTENTE");
        StartCoroutine(DesactivarDespuesDeTiempo("BOTON_ASISTENTE", 20f));

    }
    private System.Collections.IEnumerator DesactivarDespuesDeTiempo(string nombreObjeto, float tiempo){
        yield return new WaitForSeconds(tiempo);
        Debug.Log($"Desactivando {nombreObjeto} después de {tiempo} segundos");
        handlerFlags.MarcarComoDesactivado(nombreObjeto);
    }
    
    private void resaltar_herramientas(){
        Debug.LogError("Ejecutando el tutorial para resaltar herramientas.");
        handlerFlags.ActivarObjeto("HERRAMIENTAS");
        martillo_0.enabled = true;
        martillo_1.enabled = true;
        destornillador_0.enabled = true;
        destornillador_1.enabled = true;
        destornillador_2.enabled = true;
    }

    private void iniciar_tutorial_diales_botones(){
        Debug.LogError("Ejecutando el tutorial para iniciar diales y botones.");
        handlerFlags.ActivarObjeto("BOTON_TUTORIAL");
        handlerFlags.ActivarObjeto("DIAL_TUTORIAL");
    }
}

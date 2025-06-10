using System;
using System.Collections;
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
    public Outline botellaMicroestructura;
    public TeleporterWithVideoSequence teleportVideo1;
    public TeleporterWithVideoSequence teleportVideo2;
    public TeleporterWithVideoSequence teleportVideo3;
    public TeleporterWithVideoSequence teleportLlamadaAtencion;
    public TeleporterWithVideoSequence teleportTerminaExperiencia;

    public GameObject videoJefe;


    public bool simular_algo=false;
    private bool ya_ejecutado = false;

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
        //checkVisibleObjectsHotZone = FindAnyObjectByType<CheckVisibleObjectsHotZone>();
        speechAssistantControllerWSS = FindAnyObjectByType<SpeechAssistantControllerWS>();
    }

    void Update()
    {
        if (simular_algo && !ya_ejecutado)
        {
            mostrar_video_jefe();
            ya_ejecutado = true; // Para que no se ejecute más veces
        }
        
        // Si quieres resetear para probar de nuevo
        if (!simular_algo)
        {
            ya_ejecutado = false;
        }
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
        // checkVisibleObjectsHotZone.enabled = true;
        // checkVisibleObjectsHotZone.showHotZone = true; // Habilitar la visualización de la hotzone
    }

    private void mostrar_boton_asistente(){
        Debug.LogError("Ejecutando el tutorial para mostrar el botón del asistente.");
        handlerFlags.ActivarObjeto("BOTON_ASISTENTE");
        StartCoroutine(DesactivarDespuesDeTiempo("BOTON_ASISTENTE", 40f));

    }
    private System.Collections.IEnumerator DesactivarDespuesDeTiempo(string nombreObjeto, float tiempo){
        yield return new WaitForSeconds(tiempo);
        Debug.Log($"Desactivando {nombreObjeto} después de {tiempo} segundos");
        handlerFlags.MarcarComoDesactivado(nombreObjeto);
    }

    private System.Collections.IEnumerator EsperarTiempo( float tiempo){
        yield return new WaitForSeconds(tiempo);
    }
    
    private void resaltar_herramientas(){
        Debug.LogError("Ejecutando el tutorial para resaltar herramientas.");
        handlerFlags.ActivarObjeto("PERSONA_HERRAMIENTAS"); 
        handlerFlags.ActivarObjeto("MANDO_HERRAMIENTAS");
        //Se desactivaran cuando completen
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

    private void mostrar_boton_menu(){
        handlerFlags.ActivarObjeto("BOTON_MENU");
    }

    private void resaltar_tarjeta_ensayos_disponibles(){
        Debug.LogError("Ejecutando el tutorial para resaltar tarjeta de ensayos disponibles.");
        handlerFlags.ActivarObjeto("RESALTAR_VER_ENSAYOS");
    }

    private void termino_tutorial(){
        teleportVideo1.shouldTeleportAndPlay = true;
    }

    //Inspección visual
    private void guiar_a_inspeccion_visual(){
        handlerFlags.ActivarObjeto("UbicacionIV");
        //StartCoroutine(DesactivarDespuesDeTiempo("GUIA_IV", 40f));
    }

    //Esto es cuando ya llego
    private void mostrar_mando_coger(){
        handlerFlags.ActivarObjeto("MANDO_IV");
        StartCoroutine(DesactivarDespuesDeTiempo("MANDO_IV", 40f));
    }

    private void mostrar_ruptura(){
        Debug.LogError("Ejecutando el tutorial para mostrar la ruptura.");
        handlerFlags.ActivarObjeto("MOSTRAR_RUPTURA");
        StartCoroutine(DesactivarDespuesDeTiempo("MOSTRAR_RUPTURA", 70f));
    }

    //El usuario logro agarrar la pieza

    private void mostrar_pizarra_1(){
        Debug.LogError("Ejecutando el tutorial para mostrar la pizarra.");
        handlerFlags.ActivarObjeto("mostrar_pizarra_1");
    }

    private void mostrar_pizarra_2(){
        Debug.LogError("Ejecutando el tutorial para mostrar la pizarra.");
        handlerFlags.DesactivarObjeto("mostrar_pizarra_1");
        handlerFlags.ActivarObjeto("mostrar_pizarra_2");
    }

    private void mostrar_video_jefe(){
        Debug.LogError("Ejecutando el tutorial para mostrar el video del jefe.");
        handlerFlags.ActivarObjeto("video1");
    }

    private void mostrar_pizarra_3(){
        Debug.LogError("Ejecutando el tutorial para mostrar la pizarra.");
        handlerFlags.DesactivarObjeto("mostrar_pizarra_2");
        handlerFlags.ActivarObjeto("mostrar_pizarra_3");
    }

    private void mostrar_quiz_1(){
        Debug.LogError("Ejecutando el tutorial para mostrar el quiz 1.");
        handlerFlags.DesactivarObjeto("mostrar_pizarra_3");
        handlerFlags.ActivarObjeto("quiz1");
    }

    private void mostrar_quiz_2(){
        Debug.LogError("Ejecutando el tutorial para mostrar el quiz 2.");
        handlerFlags.DesactivarObjeto("quiz1");
        handlerFlags.ActivarObjeto("quiz2");
    }

    private void mostrar_quiz_3(){
        Debug.LogError("Ejecutando el tutorial para mostrar el quiz 3.");
        handlerFlags.DesactivarObjeto("quiz2");
        handlerFlags.ActivarObjeto("quiz3");
    }

    private void mostrar_funcionamiento_plumon(){
        Debug.LogError("Ejecutando el tutorial para mostrar el funcionamiento del plumón.");
        handlerFlags.ActivarObjeto("PLUMON");
        StartCoroutine(DesactivarDespuesDeTiempo("PLUMON", 70f));
    }

    //Origen
    private void mostrar_imagen_origen(){
        Debug.LogError("Ejecutando el tutorial para mostrar la imagen de origen.");
        handlerFlags.ActivarObjeto("IMAGEN_ORIGEN");
    }

    //Analisis microestructural
    private void guiar_analisis_microestructural(){
        Debug.LogError("Ejecutando el tutorial para guiar el análisis microestructural.");
        handlerFlags.ActivarObjeto("GUIA_AM"); //Estas se apagaran cuando llegue al ensayo
        StartCoroutine(DesactivarDespuesDeTiempo("GUIA_IV", 40f));
    }
    //Cuando llego
    private void video_preparacion_pieza_mc(){
        Debug.LogError("Ejecutando el tutorial para mostrar el video de preparación de la pieza.");
        handlerFlags.ActivarObjeto("VIDEO_PREPARACION_PIEZA_MC");
    }

    private void mostrar_pieza_mitad(){
        Debug.LogError("Ejecutando el tutorial para mostrar la pieza a la mitad.");
        handlerFlags.ActivarObjeto("PIEZA_MITAD");
    }

    private void mostrar_pieza_posprocesada(){
        Debug.LogError("Ejecutando el tutorial para mostrar la pieza posprocesada.");
        handlerFlags.ActivarObjeto("PIEZA_POSPROCESADA");
    }

    //Mostrar dial aumento en analisis microestrucutral
    private void mostrar_dial_aumento(){
        Debug.LogError("Mostrando holograma de aumento.");
        handlerFlags.ActivarObjeto("AUMENTOAXIS");
    }

    private void mostrar_diales_plataforma(){
        Debug.LogError("Activando diales de plataforma.");
        handlerFlags.ActivarObjeto("DIAL1");
        handlerFlags.ActivarObjeto("DIAL2");
    }
    
    //Las ultimas

    private void resaltar_zona_canal_lubricación(){
        Debug.LogError("Resaltando zona del canal de lubricación ");
        handlerFlags.ActivarObjeto("ZONA_CANAL_LUBRICACION");
        DesactivarDespuesDeTiempo("ZONA_CANAL_LUBRICACION", 20f);
    }

    private void mostrar_flecha_zona_rara(){
        Debug.LogError("Mostrando flecha de zona rara.");
        handlerFlags.ActivarObjeto("FLECHA_ZONA_RARA");
        DesactivarDespuesDeTiempo("FLECHA_ZONA_RARA", 20f);
    }

    private void mostrar_botella_reactivo(){
        Debug.LogError("Resaltando botella de reactivo.");
        botellaMicroestructura.enabled = true;
    }

    //UTILS

    private void deseo_ver()
    {
        StartCoroutine(deseo_ver_coroutine());
    }
    private IEnumerator deseo_ver_coroutine()
    {
        Debug.LogError("Mandando imagen...");
        int width = 512;
        int height = 512;
        yield return new WaitForEndOfFrame();

        var rt = new RenderTexture(width, height, 24);
        Camera.main.targetTexture = rt;
        Camera.main.Render();
        RenderTexture.active = rt;

        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
        Destroy(rt);

        byte[] jpg = tex.EncodeToJPG(75);
        string b64 = Convert.ToBase64String(jpg);
        string message = "[imagen:" + b64 + "]";
        speechAssistantControllerWSS.SendTranscriptionToWebSocket(message);

        Destroy(tex);
    }




    //Dureza
    private void activar_camino_dureza(){
        Debug.LogError("Activando camino de dureza.");
        handlerFlags.ActivarObjeto("GUIA_MD");
    }
    
    private void señalar_posicion_de_muestra_dureza(){
        Debug.LogError("Señalando posición de muestra de dureza.");
        handlerFlags.ActivarObjeto("Renderer_Dureza");
    }

    private void animacion_diales_frontal(){
        Debug.LogError("Mostrando dureza frontal.");
        handlerFlags.ActivarObjeto("DIALVICK2");
        
    }

    private void animacion_diales_lateral(){
        Debug.LogError("Mostrando dureza lateral.");
        handlerFlags.ActivarObjeto("DIALVICK1");
    }

    private void boton_inicio_dureza(){
        Debug.LogError("Activando botón de inicio de dureza.");
        handlerFlags.ActivarObjeto("run");
        handlerFlags.ActivarObjeto("contador_boton");
    }

    private void highlight_botones_subir_bajar(){
        Debug.LogError("Resaltando botones de subir y bajar.");
        handlerFlags.ActivarObjeto("up");
        handlerFlags.ActivarObjeto("down");
        
    }
    
    private void highlight_boton_setear(){
        Debug.LogError("Resaltando botón de setear.");
        handlerFlags.ActivarObjeto("save");
    }

    
    private void seleccionar_peso(){
        Debug.LogError("Seleccionando peso.");
        handlerFlags.ActivarObjeto("peso");
    }
    
    private void highlight_pantalla_resultados(){
        Debug.LogError("Resaltando pantalla de resultados.");
        handlerFlags.ActivarObjeto("MonitorLine");
    }

    // Recibe un string con formato "[nombreObjeto] mensaje a enviar"
    public void toggle_event(string mensaje){
        // Extraer el nombre del objeto entre corchetes
        if (string.IsNullOrEmpty(mensaje) || !mensaje.Contains("[") || !mensaje.Contains("]"))
            return;
            
        int inicioCorchete = mensaje.IndexOf("[");
        int finCorchete = mensaje.IndexOf("]");
        
        if (inicioCorchete >= finCorchete)
            return;
            
        string nombreObjeto = mensaje.Substring(inicioCorchete + 1, finCorchete - inicioCorchete - 1);
        
        // Verificar si el objeto está activo
        if (handlerFlags != null && handlerFlags.EstadoObjeto(nombreObjeto)) {
            // Extraer el mensaje sin los corchetes para enviarlo
            string mensajeSinCorchetes = mensaje.Substring(finCorchete + 1).Trim();
            // Solo enviar el mensaje si el objeto está activo
            speechAssistantControllerWSS.SendTranscriptionToWebSocket(mensajeSinCorchetes);
        }
    }

    private void señalar_posicion_de_muestra_quimica(){
        Debug.LogError("Señalando posición de muestra química.");
        handlerFlags.ActivarObjeto("Renderer_Quimico");
    }   
    
    private void activar_camino_quimico(){
        Debug.LogError("Activando camino de química.");
        handlerFlags.ActivarObjeto("GUIA_AQ");
    }

    private void animacion_boton_quimica(){
        Debug.LogError("Mostrando botón de química.");
        handlerFlags.ActivarObjeto("BOTONQUIMICA");
    }

    private void mostrar_ejercicios_quimica(){
        Debug.LogError("Mostrando ejercicios de química.");
        handlerFlags.ActivarObjeto("VICKERS_EXERCISE");
    }   

    private void mostrar_zona_quimica(){
        Debug.LogError("Mostrando zona de química.");
        handlerFlags.ActivarObjeto("ZONA_QUIMICA");
    }

    private void mostrar_zona_dureza(){
        Debug.LogError("Mostrando zona de corte.");
        handlerFlags.ActivarObjeto("ZONA_CORTE");
    }

    private void mostrar_ejercicios_dureza(){
        Debug.LogError("Mostrando ejercicios de dureza.");
        handlerFlags.ActivarObjeto("VICKERS_EXERCISE");
    }


    
    // Función para enviar alertas directamente
    public void enviar_alerta(string mensaje){
        // Si el mensaje tiene el formato [objeto] mensaje, verificar si el objeto está activo
        if (!string.IsNullOrEmpty(mensaje) && mensaje.Contains("[") && mensaje.Contains("]")) {
            int inicioCorchete = mensaje.IndexOf("[");
            int finCorchete = mensaje.IndexOf("]");
            
            if (inicioCorchete < finCorchete) {
                string nombreObjeto = mensaje.Substring(inicioCorchete + 1, finCorchete - inicioCorchete - 1);
                string mensajeSinCorchetes = mensaje.Substring(finCorchete + 1).Trim();
                
                // Verificar si el objeto está activo
                if (handlerFlags != null && handlerFlags.EstadoObjeto(nombreObjeto)) {
                    speechAssistantControllerWSS.SendTranscriptionToWebSocket(mensajeSinCorchetes);
                }
            }
        } else {
            // Si no tiene formato especial, enviar directamente
            speechAssistantControllerWSS.SendTranscriptionToWebSocket(mensaje);
        }
    }

    public void advertencia(){
        Debug.LogError("Ejecutando llamada de atención.");
        teleportLlamadaAtencion.shouldTeleportAndPlay = true;
    }

    public void llamada_jefe(){
        Debug.LogError("Ejecutando llamada de jefe.");
        videoJefe.SetActive(true);
    }

    public void mostrar_video_final(){
        Debug.LogError("Ejecutando video final.");
        teleportVideo3.shouldTeleportAndPlay = true;
    }

    public void mostrar_video_resumen_caso_estudio(){
        Debug.LogError("Ejecutando video de resumen del caso de estudio.");
        teleportVideo1.shouldTeleportAndPlay = true;
    }

    public void termina_experiencia(){
        Debug.LogError("Ejecutando termina experiencia.");
        teleportTerminaExperiencia.shouldTeleportAndPlay = true;
    }
}

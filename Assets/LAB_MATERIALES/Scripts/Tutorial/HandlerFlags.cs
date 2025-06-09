using UnityEngine;
using System.Collections.Generic;

public class HandlerFlags : MonoBehaviour
{
    [System.Serializable]
    public class FlagObject
    {
        public string nombre;
        public GameObject objeto;
        [SerializeField]
        private bool _enabled = false;

        public bool enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    if (objeto != null)
                    {
                        objeto.SetActive(value);
                    }
                }
            }
        }

        // Este método se llama cuando se modifica el valor en el Inspector
        private void OnValidate()
        {
            if (objeto != null)
            {
                objeto.SetActive(_enabled);
            }
        }
    }

    [Header("Objetos Controlados")]
    public List<FlagObject> objetosControlados = new List<FlagObject>();

    [Header("Referencias")]
    public SpeechAssistantControllerWS speechAssistant;

    // HashSet para mantener registro de objetos ya procesados (solo se ejecuta una vez por objeto)
    private HashSet<string> objetosProcesados = new HashSet<string>();
    
    // HashSet para mantener registro de mensajes ya enviados (solo se envía una vez por mensaje)
    private HashSet<string> mensajesEnviados = new HashSet<string>();

    private void Start()
    {
        // Asegurarse de que todos los objetos estén en el estado correcto al inicio
        foreach (var obj in objetosControlados)
        {
            if (obj.objeto != null)
            {
                obj.objeto.SetActive(obj.enabled);
            }
        }
    }

    private void Update()
    {
        // Verificar continuamente los estados de los objetos durante el juego
        foreach (var obj in objetosControlados)
        {
            if (obj.objeto != null)
            {
                // Si el estado del GameObject no coincide con el estado enabled, actualizarlo
                if (obj.objeto.activeInHierarchy != obj.enabled)
                {
                    obj.objeto.SetActive(obj.enabled);
                }
            }
        }
    }

    // Activa un objeto específico por nombre
    public void ActivarObjeto(string nombreObjeto)
    {
        FlagObject obj = objetosControlados.Find(x => x.nombre == nombreObjeto);
        if (obj != null && obj.objeto != null)
        {
            obj.enabled = true;
        }
    }

    public void DesactivarObjeto(string nombreObjeto)
    {
        FlagObject obj = objetosControlados.Find(x => x.nombre == nombreObjeto);
        if (obj != null && obj.objeto != null)
        {
            obj.enabled = false;
        }
    }
    // Marca un objeto como desactivado y envía el mensaje completo al asistente (SOLO UNA VEZ POR OBJETO)
    public void MarcarComoDesactivado(string textoCompleto)
    {
        // Buscar el nombre del objeto entre corchetes
        int inicioCorchete = textoCompleto.IndexOf('[');
        int finCorchete = textoCompleto.IndexOf(']');
        
        if (inicioCorchete >= 0 && finCorchete > inicioCorchete)
        {
            // Extraer el nombre del objeto
            string nombreObjeto = textoCompleto.Substring(inicioCorchete + 1, finCorchete - inicioCorchete - 1);
            
            // Verificar si este objeto ya fue procesado anteriormente
            if (objetosProcesados.Contains(nombreObjeto))
            {
                Debug.Log($"El objeto '{nombreObjeto}' ya fue procesado anteriormente. Ignorando llamada duplicada.");
                return; // Salir del método sin hacer nada más
            }
            
            // Agregar el objeto a la lista de procesados
            objetosProcesados.Add(nombreObjeto);
            
            // Buscar y desactivar el objeto
            FlagObject obj = objetosControlados.Find(x => x.nombre == nombreObjeto);
            if (obj != null)
            {
                obj.enabled = false;
                Debug.Log($"Objeto '{nombreObjeto}' desactivado correctamente");
            }
            else
            {
                Debug.LogWarning($"No se encontró el objeto '{nombreObjeto}' en la lista de objetos controlados");
            }
            
            // Enviar el texto completo al asistente de voz
            Debug.Log($"Intentando enviar texto completo: '{textoCompleto}'");
            if (speechAssistant != null)
            {
                Debug.Log("SpeechAssistant encontrado, enviando al WebSocket...");
                speechAssistant.SendTranscriptionToWebSocket(textoCompleto);
                Debug.Log("Texto enviado al WebSocket");
            }
            else
            {
                Debug.LogWarning("SpeechAssistantControllerWS no está asignado en HandlerFlags");
            }
        }
        else
        {
            Debug.LogWarning("No se encontraron corchetes en el texto proporcionado: " + textoCompleto);
        }
    }

    // Verifica el estado de un objeto
    public bool EstadoObjeto(string nombreObjeto)
    {
        FlagObject obj = objetosControlados.Find(x => x.nombre == nombreObjeto);
        return obj != null && obj.enabled;
    }

    public void EnableAndDisableObject(string nombreObjeto, bool enable)
    {
        FlagObject obj = objetosControlados.Find(x => x.nombre == nombreObjeto);    
        if (obj != null)
        {
            obj.enabled = enable;
        }
    }

    // Método para reiniciar la lista de objetos procesados (útil para testing o reiniciar nivel)
    public void ReiniciarObjetosProcesados()
    {
        objetosProcesados.Clear();
        Debug.Log("Lista de objetos procesados reiniciada");
    }

    // Método para verificar si un objeto ya fue procesado
    public bool ObjetoYaProcesado(string nombreObjeto)
    {
        return objetosProcesados.Contains(nombreObjeto);
    }

    // Envía un mensaje al WebSocket SOLO UNA VEZ por mensaje (sin desactivar objetos)
    public void EnviarMensajeUnaVez(string textoCompleto)
    {
        // Verificar si este mensaje ya fue enviado anteriormente
        if (mensajesEnviados.Contains(textoCompleto))
        {
            Debug.Log($"El mensaje '{textoCompleto}' ya fue enviado anteriormente. Ignorando envío duplicado.");
            return; // Salir del método sin enviar nada
        }
        
        // Agregar el mensaje a la lista de enviados
        mensajesEnviados.Add(textoCompleto);
        
        // Enviar el texto completo al asistente de voz
        Debug.Log($"Enviando mensaje único: '{textoCompleto}'");
        if (speechAssistant != null)
        {
            Debug.Log("SpeechAssistant encontrado, enviando al WebSocket...");
            speechAssistant.SendTranscriptionToWebSocket(textoCompleto);
            Debug.Log("Mensaje enviado al WebSocket");
        }
        else
        {
            Debug.LogWarning("SpeechAssistantControllerWS no está asignado en HandlerFlags");
        }
    }

    // Método para reiniciar la lista de mensajes enviados (útil para testing o reiniciar nivel)
    public void ReiniciarMensajesEnviados()
    {
        mensajesEnviados.Clear();
        Debug.Log("Lista de mensajes enviados reiniciada");
    }

    // Método para verificar si un mensaje ya fue enviado
    public bool MensajeYaEnviado(string mensaje)
    {
        return mensajesEnviados.Contains(mensaje);
    }

    // Método para reiniciar AMBAS listas (objetos procesados y mensajes enviados)
    public void ReiniciarTodo()
    {
        objetosProcesados.Clear();
        mensajesEnviados.Clear();
        Debug.Log("Todas las listas reiniciadas (objetos procesados y mensajes enviados)");
    }
}


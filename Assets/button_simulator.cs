using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class button_simulator : MonoBehaviour
{
    [System.Serializable]
    public class BotonConfig
    {
        public Button boton;
        public bool simularPresionado = false;
        [HideInInspector] public bool estadoAnterior = false;
        [HideInInspector] public bool estadoOriginal = true;
    }

    [Header("Configuración de Botones")]
    public List<BotonConfig> botonesUI = new List<BotonConfig>();
    
    // Para facilitar la depuración desde el inspector
    public bool simularTodos = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Guardar el estado original de cada botón
        foreach (BotonConfig config in botonesUI)
        {
            if (config.boton != null)
            {
                config.estadoOriginal = config.boton.interactable;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Verificar si se activó la opción de simular todos
        if (simularTodos)
        {
            foreach (BotonConfig config in botonesUI)
            {
                config.simularPresionado = true;
            }
            simularTodos = false; // Restablecer para que no siga activando continuamente
        }

        // Detectar cambios para cada botón individualmente
        foreach (BotonConfig config in botonesUI)
        {
            if (config.boton != null && config.simularPresionado != config.estadoAnterior)
            {
                ActualizarEstadoBoton(config);
                config.estadoAnterior = config.simularPresionado;
            }
        }
    }

    // Método para actualizar el estado de un botón individual
    void ActualizarEstadoBoton(BotonConfig config)
    {
        if (config.boton == null) return;

        // Si simulamos presionado, desactivamos la interacción
        config.boton.interactable = config.simularPresionado ? false : config.estadoOriginal;
        
        // Invocar eventos del botón si está simulando presionado
        if (config.simularPresionado)
        {
            config.boton.onClick.Invoke();
        }
    }

    // Método público para agregar un botón a la lista
    public void AgregarBoton(Button nuevoBoton)
    {
        if (nuevoBoton != null)
        {
            // Verificar que no exista ya
            foreach (BotonConfig config in botonesUI)
            {
                if (config.boton == nuevoBoton) return;
            }
            
            // Crear nueva configuración
            BotonConfig nuevaConfig = new BotonConfig();
            nuevaConfig.boton = nuevoBoton;
            nuevaConfig.estadoOriginal = nuevoBoton.interactable;
            botonesUI.Add(nuevaConfig);
        }
    }

    // Método público para simular presionado de un botón específico
    public void SimularPresionado(Button boton, bool presionar)
    {
        foreach (BotonConfig config in botonesUI)
        {
            if (config.boton == boton)
            {
                config.simularPresionado = presionar;
                ActualizarEstadoBoton(config);
                break;
            }
        }
    }

    // Método para simular presionado por índice
    public void SimularPresionadoPorIndice(int indice, bool presionar)
    {
        if (indice >= 0 && indice < botonesUI.Count)
        {
            botonesUI[indice].simularPresionado = presionar;
            ActualizarEstadoBoton(botonesUI[indice]);
        }
    }
}

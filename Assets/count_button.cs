using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

[Serializable]
public class ButtonCounter
{
    [Tooltip("Nombre para identificar este contador")]
    public string name;
    
    [Tooltip("Botón al que se conectará este contador")]
    public Button button;
    
    [Tooltip("Activar/desactivar el contador")]
    public bool isActive = true;
    
    [Tooltip("Contador actual de clics")]
    public int currentCount = 0;
    
    [Tooltip("Valor objetivo para activar el evento")]
    public int targetCount = 5;
    
    [Tooltip("Evento que se activa cuando se alcanza el objetivo")]
    public UnityEvent onTargetReached;
    
    [Tooltip("Evento que se activa en cada clic")]
    public UnityEvent onButtonClick;
    
    [Tooltip("Componente Text para mostrar el contador (opcional)")]
    public Text counterText;
}

public class count_button : MonoBehaviour
{
    [Tooltip("Lista de contadores de botones")]
    public List<ButtonCounter> buttonCounters = new List<ButtonCounter>();
    
    void Start()
    {
        // Conectar cada botón con su contador
        ConnectAllButtons();
    }
    
    // Conectar todos los botones con sus respectivos contadores
    public void ConnectAllButtons()
    {
        foreach (var counter in buttonCounters)
        {
            if (counter.button != null)
            {
                // Remueve listeners anteriores para evitar duplicados
                counter.button.onClick.RemoveAllListeners();
                
                // Crear una variable local para capturar el contador actual en el closure
                var localCounter = counter;
                counter.button.onClick.AddListener(() => IncrementCounter(localCounter));
                
                // Actualizar el texto inicial
                UpdateCounterText(counter);
            }
        }
    }
    
    // Incrementar un contador específico
    public void IncrementCounter(ButtonCounter counter)
    {
        // Verificar si el contador está activo
        if (!counter.isActive)
            return;
            
        counter.currentCount++;
        UpdateCounterText(counter);
        
        // Invocar el evento de clic
        counter.onButtonClick?.Invoke();
        
        // Comprobar si se ha alcanzado el objetivo
        if (counter.currentCount == counter.targetCount)
        {
            counter.onTargetReached?.Invoke();
        }
    }
    
    // Método para incrementar un contador por su índice
    public void IncrementCounterByIndex(int index)
    {
        if (index >= 0 && index < buttonCounters.Count)
        {
            IncrementCounter(buttonCounters[index]);
        }
    }
    
    // Método para restablecer un contador específico
    public void ResetCounter(ButtonCounter counter)
    {
        counter.currentCount = 0;
        UpdateCounterText(counter);
    }
    
    // Método para restablecer un contador por su índice
    public void ResetCounterByIndex(int index)
    {
        if (index >= 0 && index < buttonCounters.Count)
        {
            ResetCounter(buttonCounters[index]);
        }
    }
    
    // Actualizar el texto del contador si existe
    private void UpdateCounterText(ButtonCounter counter)
    {
        if (counter.counterText != null)
        {
            counter.counterText.text = counter.currentCount.ToString();
        }
    }
    
    // Método para activar/desactivar un contador
    public void SetCounterActive(int index, bool isActive)
    {
        if (index >= 0 && index < buttonCounters.Count)
        {
            buttonCounters[index].isActive = isActive;
        }
    }
    
    // Método para establecer un nuevo objetivo para un contador
    public void SetTargetCount(int index, int newTarget)
    {
        if (index >= 0 && index < buttonCounters.Count)
        {
            buttonCounters[index].targetCount = newTarget;
        }
    }
}

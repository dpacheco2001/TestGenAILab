using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class VickersMachineController : MonoBehaviour
{
    [Header("Referencias UI")]
    public TextMeshProUGUI mainDisplayText;    
    public TextMeshProUGUI historyDisplayText; 
    public TextMeshProUGUI statsDisplayText;   
    public TextMeshProUGUI diameter1Text;      
    public TextMeshProUGUI diameter2Text;      
    public TextMeshProUGUI weightDisplayText;  
    public TextMeshProUGUI HV; 

    [Header("Sistema de Microscopio")]
    public SimpleMicroscopeSystem microscopeSystem;

    [Header("Referencias de Objeto")]
    public Transform platform;
    public Transform indenter; // Nueva referencia al indentador
    public Animator m_animator; // Nueva referencia al Animator

    [Header("Configuración")]
    public float platformMoveSpeed = 0.1f;
    public float maxHeight = 0.03f;
    public float minHeight = 0f;
    public float indenterMoveDistance = 0.1f; // Distancia que se moverá el indentador
    public float indenterMoveSpeed = 0.5f; // Velocidad del indentador
    
    [Header("Configuración de Peso")]
    public float weightIncrement = 5f;
    public float maxWeight = 100f;
    public float minWeight = 0f;

    private class TestData
    {
        public int testNumber;
        public float diameter1;
        public float diameter2;
        public float hardness;
        public float weight;

        public override string ToString()
        {
            return $"Test #{testNumber}: HV={hardness:F1}, D1={diameter1:F3}mm, D2={diameter2:F3}mm, Weight={weight}kg";
        }
    }

    private float currentPlatformHeight = 0f;
    private float selectedWeight = 0f;
    private bool isWeightSelectionMode = false;
    private List<TestData> testHistory = new List<TestData>();
    private int currentTestNumber = 0;
    private bool isTestInProgress = false;

    private enum MachineState
    {
        PlatformControl,
        WeightSelection,
        Testing
    }
    private MachineState currentState = MachineState.PlatformControl;

    void Start()
    {
        m_animator = indenter.GetComponent<Animator>(); // Obtener el Animator
    }

    public void OnUpButton()
    {
        if (isTestInProgress) return;
        
        switch (currentState)
        {
            case MachineState.PlatformControl:
                if (currentPlatformHeight < maxHeight)
                {
                    currentPlatformHeight += platformMoveSpeed * Time.deltaTime;
                    platform.Translate(Vector3.forward * platformMoveSpeed * Time.deltaTime);
                    UpdateMainDisplay($"Height: {currentPlatformHeight:F2}");
                }
                break;

            case MachineState.WeightSelection:
                selectedWeight = Mathf.Min(selectedWeight + weightIncrement, maxWeight);
                UpdateMainDisplay($"Weight: {selectedWeight:F1}kg");
                break;
        }
    }

    public void OnDownButton()
    {
        if (isTestInProgress) return;
        
        switch (currentState)
        {
            case MachineState.PlatformControl:
                if (currentPlatformHeight > minHeight)
                {
                    currentPlatformHeight -= platformMoveSpeed * Time.deltaTime;
                    platform.Translate(Vector3.back * platformMoveSpeed * Time.deltaTime);
                    UpdateMainDisplay($"Height: {currentPlatformHeight:F2}");
                }
                break;

            case MachineState.WeightSelection:
                selectedWeight = Mathf.Max(selectedWeight - weightIncrement, minWeight);
                UpdateMainDisplay($"Weight: {selectedWeight:F1}kg");
                break;
        }
    }

    public void OnPesoButton()
    {
        if (!isTestInProgress)
        {
            currentState = MachineState.WeightSelection;
            UpdateMainDisplay($"Weight: {selectedWeight:F1}kg");
        }
    }

    public void OnSaveButton()
    {
        if (!isTestInProgress && currentState == MachineState.WeightSelection)
        {
            currentState = MachineState.PlatformControl;
            UpdateMainDisplay($"Height: {currentPlatformHeight:F2}");
        }
    }

    public void OnRunButton()
    {
        if (!isTestInProgress && currentState == MachineState.PlatformControl)
        {
            StartCoroutine(PerformVickersTest());
        }
    }

    private IEnumerator PerformVickersTest()
    {
        isTestInProgress = true;
        currentState = MachineState.Testing;
        UpdateMainDisplay("Testing...");

        // Activar la animación
        m_animator.SetBool("bool_identador", true);
        Debug.Log("Animación iniciada");

        // Esperar a que la animación baje (ajusta este tiempo según tu animación)
        yield return new WaitForSeconds(0.5f);

        // Realizar mediciones
        currentTestNumber++;
        
        TestData newTest = new TestData
        {
            testNumber = currentTestNumber,
            diameter1 = Random.Range(0.1f, 0.5f),
            diameter2 = Random.Range(0.1f, 0.5f)
        };
        
        // Calcular dureza
        float averageDiameter = (newTest.diameter1 + newTest.diameter2) / 2;
        newTest.hardness = 1.854f * (selectedWeight / (averageDiameter * averageDiameter));
        
        testHistory.Add(newTest);
        
        if (microscopeSystem != null)
        {
            microscopeSystem.CreateIndentation(newTest.hardness);
        }

        // Actualizar displays
        UpdateMainDisplay($"HV: {newTest.hardness:F1}");
        UpdateDiameterDisplays(newTest.diameter1, newTest.diameter2, newTest.hardness);
        UpdateHistoryDisplay();
        UpdateStatsDisplay();

        // Esperar un momento antes de subir
        yield return new WaitForSeconds(0.5f);

        // Desactivar la animación (esto hará que suba)
        m_animator.SetBool("bool_identador", false);

        // Esperar a que termine la animación de subida
        yield return new WaitForSeconds(0.5f);

        isTestInProgress = false;
        currentState = MachineState.PlatformControl;
    }

    private void UpdateMainDisplay(string text)
    {
        if (mainDisplayText != null)
        {
            mainDisplayText.text = text;
        }
    }

    private void UpdateDiameterDisplays(float d1, float d2, float hardness)
    {
        if (diameter1Text != null)
            diameter1Text.text = $"{d1:F3}mm";
        if (diameter2Text != null)
            diameter2Text.text = $"{d2:F3}mm";
        if (weightDisplayText != null)
            weightDisplayText.text = $"{selectedWeight:F1}kgf";
        if (HV != null)
            HV.text = $"HV: {hardness:F1}";
    }

    private void UpdateHistoryDisplay()
    {
        if (historyDisplayText != null)
        {
            string history = "Ensayos :\n";
            foreach (var test in testHistory.TakeLast(5))
            {
                history += "-----------------------------\n";
                history += test.ToString() + "\n";
            }
            historyDisplayText.text = history;
        }
    }

    private void UpdateStatsDisplay()
    {
        if (statsDisplayText != null && testHistory.Count > 0)
        {
            float avgHardness = testHistory.Average(t => t.hardness);
            float maxHardness = testHistory.Max(t => t.hardness);
            float minHardness = testHistory.Min(t => t.hardness);
            float avgD1 = testHistory.Average(t => t.diameter1);
            float avgD2 = testHistory.Average(t => t.diameter2);

            string stats = "Statistics:\n" +
                          $"Promedio HV: {avgHardness:F1}\n" +
                          $"Max HV:      {maxHardness:F1}\n" +
                          $"Min HV:      {minHardness:F1}\n" +
                          $"Promedio D1: {avgD1:F3}mm\n" +
                          $"Promedio D2: {avgD2:F3}mm";
            
            statsDisplayText.text = stats;
        }
    }

    public void ResetTest()
    {
        if (!isTestInProgress)
        {
            currentPlatformHeight = 0f;
            platform.localPosition = Vector3.zero;
            UpdateMainDisplay("Ready");
        }
    }

    public void ClearHistory()
    {
        if (!isTestInProgress)
        {
            testHistory.Clear();
            currentTestNumber = 0;
            UpdateHistoryDisplay();
            UpdateStatsDisplay();
        }
    }
}
using System.Collections;
using UnityEngine;
using TMPro;

public class quimicosdatos : MonoBehaviour
{
    public TextMeshProUGUI[] textMeshes1; // Arreglo para la primera iteraci�n
    public TextMeshProUGUI[] textMeshes2; // Arreglo para la segunda iteraci�n
    public TextMeshProUGUI[] textMeshes3; // Arreglo para la tercera iteraci�n
    public TextMeshProUGUI[] textMeshes4; // Arreglo para la cuarta iteraci�n
    public BarraCarga bar; // Variable para indicar si la carga est� completa



    private int currentArrayIndex = 0; // �ndice para llevar el control de la iteraci�n actual

    // Valores base para cada componente qu�mico
    private float baseC = 0.48f;
    private float baseMn = 0.56f;
    private float baseSi = 0.33f;
    private float baseCr = 1.02f;
    private float baseMo = 0.24f;
    private float baseNi = 0.25f;
    private float baseCu = 0.08f;
    private float baseP = 0.010f;
    private float baseS = 0.015f;

    void Update()
    {
        if (bar.isLoadingComplete)
        {
            // Seleccionar el arreglo correspondiente para la iteraci�n actual
            switch (currentArrayIndex)
            {
                case 0:
                    FillTextMeshArray(textMeshes1);
                    break;
                case 1:
                    FillTextMeshArray(textMeshes2);
                    break;
                case 2:
                    FillTextMeshArray(textMeshes3);
                    break;
                case 3:
                    FillTextMeshArray(textMeshes4);
                    break;
            }

            // Incrementa el �ndice de arreglo (y reinicia a 0 despu�s de 4)
            currentArrayIndex = (currentArrayIndex + 1) % 4;
            bar.isLoadingComplete = false; // Reinicia la variable
        }
    }

    void FillTextMeshArray(TextMeshProUGUI[] textMeshArray)
    {
        // Genera valores aleatorios dentro de un rango razonable para cada elemento qu�mico
        float[] randomComposition = new float[9];
        randomComposition[0] = Random.Range(baseC - 0.05f, baseC + 0.05f); // %C
        randomComposition[1] = Random.Range(baseMn - 0.05f, baseMn + 0.05f); // %Mn
        randomComposition[2] = Random.Range(baseSi - 0.05f, baseSi + 0.05f); // %Si
        randomComposition[3] = Random.Range(baseCr - 0.1f, baseCr + 0.1f); // %Cr
        randomComposition[4] = Random.Range(baseMo - 0.05f, baseMo + 0.05f); // %Mo
        randomComposition[5] = Random.Range(baseNi - 0.05f, baseNi + 0.05f); // %Ni
        randomComposition[6] = Random.Range(baseCu - 0.02f, baseCu + 0.02f); // %Cu
        randomComposition[7] = Random.Range(baseP - 0.005f, baseP + 0.005f); // %P
        randomComposition[8] = Random.Range(baseS - 0.005f, baseS + 0.005f); // %S

        // Asigna los valores generados al TextMeshPro correspondiente en el arreglo
        for (int i = 0; i < textMeshArray.Length && i < randomComposition.Length; i++)
        {
            textMeshArray[i].text = randomComposition[i].ToString("F3"); // Formato de tres decimales
        }
    }
}
using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class informe_evaluador : MonoBehaviour
{
    [Header("Campos de UI para el informe evaluador")]
    public TextMeshProUGUI nombreText;
    public TextMeshProUGUI codigoText;
    public TextMeshProUGUI recomendacionUbicacionText;
    public TextMeshProUGUI recomendacionAgujeroText;
    public TextMeshProUGUI recomendacionMecanismoText;
    public TextMeshProUGUI recomendacionOrigenText;
    public TextMeshProUGUI recomendacionPatronesText;
    public TextMeshProUGUI resumenQuizText;
    public TextMeshProUGUI recomendacionesGeneralesText;
    public TextMeshProUGUI vistoBuenoRobertText;

    [System.Serializable]
    public class JsonCalificador
    {
        public string nombre;
        public string codigo;
        public string recomendacion_ubicacion_fractura;
        public string recomendacion_observaciones_agujero;
        public string recomendacion_mecanismo_fractura;
        public string recomendacion_origen_fractura;
        public string recomendacion_patrones;
        public string resumen_quiz;
        public string recomendaciones_generales;
        public string visto_bueno_robert;
    }

    public void MostrarDesdeJson(string json)
    {
        JsonCalificador data = JsonUtility.FromJson<JsonCalificador>(json);
        if (data == null) return;

        if (nombreText) nombreText.text = Limpiar(data.nombre);
        if (codigoText) codigoText.text = Limpiar(data.codigo);
        if (recomendacionUbicacionText) recomendacionUbicacionText.text = Limpiar(data.recomendacion_ubicacion_fractura);
        if (recomendacionAgujeroText) recomendacionAgujeroText.text = Limpiar(data.recomendacion_observaciones_agujero);
        if (recomendacionMecanismoText) recomendacionMecanismoText.text = Limpiar(data.recomendacion_mecanismo_fractura);
        if (recomendacionOrigenText) recomendacionOrigenText.text = Limpiar(data.recomendacion_origen_fractura);
        if (recomendacionPatronesText) recomendacionPatronesText.text = Limpiar(data.recomendacion_patrones);
        if (resumenQuizText) resumenQuizText.text = Limpiar(data.resumen_quiz);
        if (recomendacionesGeneralesText) recomendacionesGeneralesText.text = Limpiar(data.recomendaciones_generales);
        if (vistoBuenoRobertText) vistoBuenoRobertText.text = Limpiar(data.visto_bueno_robert);
    }

    private string Limpiar(string texto)
    {
        if (string.IsNullOrEmpty(texto)) return "";
        return texto.Replace("\\n", "\n").Replace("  ", " ").Trim();
    }
}


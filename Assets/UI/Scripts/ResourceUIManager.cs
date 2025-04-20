using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceUIManager : MonoBehaviour
{
    [Serializable]
    public class ResourceData {
        public string ensayo;
        public string resourceType;
        public string header;
        public string subHeader;
        public string description;
        public Sprite thumbnail;
    }

    [Header("Filtros")]
    public TMP_Dropdown ensayoDropdown;
    public ToggleGroup  resourceToggleGroup;

    [Header("ViewerCards")]
    public Transform    viewerCardParent;
    public GameObject   viewerCardPrefab;

    [Header("Viewport Prefab")]
    public GameObject   viewportPrefab;  // prefab que ahora tiene ViewportContent
    public Transform    viewportParent;  // dónde colgarás todas las instancias

    [Header("Datos")]
    public List<ResourceData> allResources;

    // Diccionario para asociar data ↔ viewport instance
    Dictionary<ResourceData, ViewportContent> _viewportMap = new();

    void Start()
    {
        // Suscribir filtros
        ensayoDropdown.onValueChanged.AddListener(_ => RefreshResources());
        foreach (var t in resourceToggleGroup.GetComponentsInChildren<Toggle>())
            t.onValueChanged.AddListener(isOn => { if (isOn) RefreshResources(); });

        // Primer poblamiento
        RefreshResources();
    }

    public void RefreshResources()
    {
        // Limpiar viejas cards
        foreach (Transform c in viewerCardParent) Destroy(c.gameObject);
        // Limpiar viejos viewports
        foreach (var kv in _viewportMap.Values)
            Destroy(kv.gameObject);
        _viewportMap.Clear();

        // Filtros actuales
        var ensayoTexto  = ensayoDropdown.options[ensayoDropdown.value].text;
        var toggleActivo = resourceToggleGroup.ActiveToggles().FirstOrDefault();
        if (toggleActivo == null) return;
        var tipo = toggleActivo.name;

        // Datos filtrados
        var lista = allResources
            .Where(r => r.ensayo == ensayoTexto && r.resourceType == tipo)
            .ToList();

        // Crear ViewerCards + sus Viewports
        for (int i = 0; i < lista.Count; i++)
        {
            var data = lista[i];

            // 1) Instanciar card
            var goCard = Instantiate(viewerCardPrefab, viewerCardParent, false);
            var card   = goCard.GetComponent<ViewerCard>();
            card.Setup(data, OnCardSelected);

            // 2) Instanciar viewport correspondiente pero oculto
            var goVp = Instantiate(viewportPrefab, viewportParent, false);
            var vp = goVp.GetComponent<ViewportContent>();
            vp.Setup(data);
            goVp.SetActive(false);

            // 3) Guardar en el diccionario
            _viewportMap[data] = vp;
        }

        // Si hay al menos uno, mostrar el primero
        if (lista.Count > 0)
            OnCardSelected(lista[0]);
    }

    void OnCardSelected(ResourceData data)
    {
        // Desactivar todos
        foreach (var vp in _viewportMap.Values)
            vp.gameObject.SetActive(false);

        // Activar el que corresponde
        if (_viewportMap.TryGetValue(data, out var vpToShow))
            vpToShow.gameObject.SetActive(true);
    }
}

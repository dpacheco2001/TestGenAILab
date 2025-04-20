using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceUIManager : MonoBehaviour {
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
    public ToggleGroup resourceToggleGroup;

    [Header("ViewerCards")]
    public Transform viewerCardParent;
    public GameObject viewerCardPrefab;

    [Header("Viewport Prefab")]
    public GameObject viewportPrefab;       // ← Prefab de tu contenido
    public Transform viewportParent;        // ← Donde se instanciará

    // instancia en ejecución
    GameObject _viewportInstance;
    Image    _vpImage;
    TMP_Text _vpHeader, _vpSubHeader, _vpDescription;

    [Header("Datos")]
    public List<ResourceData> allResources;

    void Start() {
        // 1) Instanciar dinámicamente el viewport
        _viewportInstance = Instantiate(viewportPrefab, viewportParent);
        // 2) Cachear referencias de sus hijos
        var t = _viewportInstance.transform;
        _vpImage        = t.Find("Image").GetComponent<Image>();
        _vpHeader       = t.Find("Header").GetComponent<TMP_Text>();
        _vpSubHeader    = t.Find("Subheader").GetComponent<TMP_Text>();
        _vpDescription  = t.Find("Description").GetComponent<TMP_Text>();

        // 3) Suscribirse a filtros
        ensayoDropdown.onValueChanged.AddListener(_ => RefreshResources());
        foreach (var tgl in resourceToggleGroup.GetComponentsInChildren<Toggle>())
            tgl.onValueChanged.AddListener(on => { if (on) RefreshResources(); });

        // 4) Primer poblamiento
        RefreshResources();
    }

    void RefreshResources() {
        var ensayoTexto = ensayoDropdown.options[ensayoDropdown.value].text;
        var toggleActivo = resourceToggleGroup.ActiveToggles().FirstOrDefault();
        var toggleName = toggleActivo?.name ?? "Ninguno";
        Debug.Log($"[UI] Filtro: {ensayoTexto} / {toggleName}");

     
        var lista = allResources
            .Where(r => r.ensayo == ensayoTexto && r.resourceType == toggleName)
            .ToList();
        Debug.Log($"[UI] Repositorios a mostrar: {lista.Count}");


        foreach (Transform c in viewerCardParent)
            Destroy(c.gameObject);


        for (int i = 0; i < lista.Count; i++) {
            var data = lista[i];
            var go   = Instantiate(viewerCardPrefab, viewerCardParent, false);
            var card = go.GetComponent<ViewerCard>();
            card.Setup(data, OnCardSelected);
        }

        if (lista.Count > 0){
            _viewportInstance.SetActive(true);
            OnCardSelected(lista[0]);
        }
        else {
            ClearViewport();
        }
            
    }

    void OnCardSelected(ResourceData d) {
        _vpImage.sprite       = d.thumbnail;
        _vpHeader.text        = d.header;
        _vpSubHeader.text     = d.subHeader;
        _vpDescription.text   = d.description;
    }

    void ClearViewport() {
        _viewportInstance.SetActive(false);
    }
}

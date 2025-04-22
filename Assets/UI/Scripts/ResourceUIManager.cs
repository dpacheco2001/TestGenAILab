using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

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
        public string videoUrl; 

    }

    [Header("Filtros")]
    public TMP_Dropdown ensayoDropdown;
    public ToggleGroup  resourceToggleGroup;

    [Header("ViewerCards")]
    public Transform    viewerCardParent;
    public GameObject   viewerCardPrefab;

    [Header("Viewport Prefab")]
    public GameObject   viewportPrefab;
    public GameObject videoViewportPrefab;   
    public Transform    viewportParent;  

    [Header("Datos")]
    public List<ResourceData> allResources;

    
    Dictionary<ResourceData, MonoBehaviour> _viewportMap = new();

    void Start()
    {
        
        ensayoDropdown.onValueChanged.AddListener(_ => RefreshResources());
        foreach (var t in resourceToggleGroup.GetComponentsInChildren<Toggle>())
            t.onValueChanged.AddListener(isOn => { if (isOn) RefreshResources(); });

        
        RefreshResources();
    }

    public void RefreshResources()
    {
        
        foreach (Transform c in viewerCardParent) Destroy(c.gameObject);
        
        foreach (var kv in _viewportMap.Values)
            Destroy(kv.gameObject);
        _viewportMap.Clear();

        
        var ensayoTexto  = ensayoDropdown.options[ensayoDropdown.value].text;
        var toggleActivo = resourceToggleGroup.ActiveToggles().FirstOrDefault();
        if (toggleActivo == null) return;
        var tipo = toggleActivo.name;


        var lista = allResources
            .Where(r => r.ensayo == ensayoTexto && r.resourceType == tipo)
            .ToList();


        for (int i = 0; i < lista.Count; i++)
        {
            var data = lista[i];


            var isVideo = data.resourceType == "Videos";
            var goCard = Instantiate(viewerCardPrefab, viewerCardParent, false);
            var card   = goCard.GetComponent<ViewerCard>();
            card.Setup(data, OnCardSelected);

            GameObject   goVp    = Instantiate(isVideo ? videoViewportPrefab : viewportPrefab,
                                            viewportParent,
                                            false);

            MonoBehaviour vpComp;

            if (isVideo)
            {
                var vpVideo = goVp.GetComponent<VideoViewportContent>();
                vpVideo.Setup(data);
                vpComp = vpVideo;
            }
            else
            {

                var vp = goVp.GetComponent<ViewportContent>();
                vp.Setup(data);
                vpComp = vp;
            }

            goVp.SetActive(false);
            _viewportMap[data] = vpComp;
        }

        if (lista.Count > 0)
            OnCardSelected(lista[0]);
    }

    void OnCardSelected(ResourceData data)
    {
        
        foreach (var vpInstance in _viewportMap.Values)
            vpInstance.gameObject.SetActive(false);

        
        if (_viewportMap.TryGetValue(data, out var selectedInstance))
            selectedInstance.gameObject.SetActive(true);
    }

    public IEnumerator AddVideoResource(
        string nombre,
        string descripcion,
        string url,
        string thumbnailUrl
    ) {
        using var uwr = UnityWebRequestTexture.GetTexture(thumbnailUrl);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityWebRequest.Result.Success) {
            Debug.LogWarning("Falló descarga thumbnail: " + uwr.error);
            yield break;
        }

        var tex = DownloadHandlerTexture.GetContent(uwr);


        var sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100
        );

        
        var rd = new ResourceData {
            ensayo       = "Recomendaciones Robert",
            resourceType = "Videos",           
            header       = nombre,
            subHeader    = url,
            description  = descripcion,
            thumbnail    = sprite,
            videoUrl     = url
        };

        allResources.Add(rd);
        RefreshResources();
    }

    public IEnumerator AddImageResource(string nombre, string base64Data)
    {
        byte[] bytes = Convert.FromBase64String(base64Data);

        var tex = new Texture2D(2, 2);
        tex.LoadImage(bytes); 


        var sprite = Sprite.Create(
            tex,
            new Rect(0, 0, tex.width, tex.height),
            new Vector2(0.5f, 0.5f),
            100
        );

        // 4) Construir tu ResourceData
        var rd = new ResourceData {
            ensayo       = "Recomendaciones Robert",
            resourceType = "Fotos", 
            header       = nombre,
            subHeader    = "",           
            description  = "",
            thumbnail    = sprite,
            videoUrl     = ""           
        };

        allResources.Add(rd);
        RefreshResources();

        yield return null;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleMicroscopeSystem : MonoBehaviour
{
    [Header("Configuración Básica")]
    public Camera microscopeCamera;     // Cámara del microscopio
    public RawImage displayScreen;      // Imagen UI donde se mostrará la vista
    public GameObject markPrefab;       // Prefab de la marca (cubo negro)
    public Transform testPiece;         // Pieza de prueba
    public float rayLength = 10f;       // Longitud del rayo
    [SerializeField]
    private LayerMask targetLayer = -1; // Layer mask establecido como Everything (-1)
    
    [Header("Control")]
    public bool createMark = false;     // Bool para crear la marca
    
    private RenderTexture microscopeView;
    
    // Factor de escala base para la marca
    public float baseScale = 0.1f;
    public float heightScale = 0.01f;
    
    // Valor de dureza actual
    private float currentHardness;
    
    void Start()
    {
        // Crear y configurar el render texture
        microscopeView = new RenderTexture(512, 512, 24);
        microscopeCamera.targetTexture = microscopeView;
        displayScreen.texture = microscopeView;
    }
    
    void Update()
    {
        // Lanzar raycast desde el centro de la cámara
        Ray ray = new Ray(microscopeCamera.transform.position, microscopeCamera.transform.forward);
        RaycastHit hit;
        // Dibujar el rayo en la ventana de Scene (solo visible en el editor)
        Debug.DrawRay(ray.origin, ray.direction * rayLength, Color.red);
        // Si el bool se activa y el raycast golpea algo, crear una marca
        if (createMark)
        {
            if (Physics.Raycast(ray, out hit, rayLength, targetLayer))
            {
                CreateMark(hit.point, hit.normal);
                Debug.Log($"Marca creada en posición: {hit.point} con dureza: {currentHardness}");
            }
            else
            {
                Debug.Log("No se detectó superficie para crear la marca");
            }
            createMark = false; // Reset automático del bool
        }
    }

    public void CreateIndentation(float hardness)
    {
        currentHardness = hardness;
        createMark = true;
    }
    
    void CreateMark(Vector3 position, Vector3 normal)
    {
        // Instanciar la marca en el punto exacto del hit
        GameObject mark = Instantiate(markPrefab, position, Quaternion.identity);
        mark.transform.SetParent(testPiece); // Hacer la marca hija de la pieza de prueba
        
        // Alinear la marca con la superficie
        mark.transform.up = normal;
        
        // Calcular el tamaño basado en la dureza
        // A mayor dureza, menor tamaño de la marca
        float scaleFactor = Mathf.Lerp(1.5f, 0.5f, currentHardness / 1000f);
        float newScale = baseScale * scaleFactor;
        
        mark.transform.localScale = new Vector3(newScale, heightScale, newScale);
    }
    
    private void OnDestroy()
    {
        if (microscopeView != null)
        {
            microscopeView.Release();
            Destroy(microscopeView);
        }
    }
}
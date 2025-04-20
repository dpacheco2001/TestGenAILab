using UnityEngine;
using UnityEngine.InputSystem;


public class MenuManager : MonoBehaviour
{
    [Header("Objeto del menú (con ScaleInterpolator)")]
    public GameObject menuObject;

    [Header("Distancia para posicionar el menú frente al usuario")]
    public float menuDistance = 2.0f;
    public InputActionReference menuButton; // Referencia al controlador XR
    
    public Animator targetAnimator;
    public string parameterName = "isMenuActive";
    public float activationThreshold = 0.1f;
    // Bandera para llevar el estado del menú (visible o no)
    private bool isMenuVisible = false;
    public bool vrenabled = false;
    private HandlerFlags handlerFlags;
    private SpeechAssistantControllerWS speechAssistantControllerWS;

    void Start()
    {
        isMenuVisible = targetAnimator.GetBool(parameterName);
        handlerFlags = FindFirstObjectByType<HandlerFlags>();
        speechAssistantControllerWS = FindFirstObjectByType<SpeechAssistantControllerWS>();
    }
    void Update()
    {
        
    
   if(vrenabled){
        if (menuButton.action.WasPressedThisFrame())
        {
            ToggleMenu();
            if(handlerFlags.EstadoObjeto("BOTON_MENU")){
                Debug.Log("Se ha apretado el botón del menú.");
                speechAssistantControllerWS.SendTranscriptionToWebSocket("*El usuario ha apretado el botón del menú satisfactoriamente*");
                handlerFlags.MarcarComoDesactivado("BOTON_MENU");
            }
        }
    
   }
   else{
    if (Input.GetKeyDown(KeyCode.Space)){
        ToggleMenu();
        }
    }
   }


    private void ToggleMenu()
    {
        Camera cam = Camera.main;
        bool current = targetAnimator.GetBool(parameterName);
        
        bool next = !current;
        
        if (cam == null)
        {
            Debug.LogWarning("No se encontró la cámara principal.");
            return;
        }

        if (!isMenuVisible)
        {
            Vector3 targetPosition = cam.transform.position + cam.transform.forward * menuDistance;
            menuObject.transform.position = targetPosition;
            Vector3 directionToCamera = cam.transform.position - menuObject.transform.position;
            if (directionToCamera != Vector3.zero)
            {
                menuObject.transform.rotation = Quaternion.LookRotation(directionToCamera) * Quaternion.Euler(0, 180, 0);
            }
        }

        targetAnimator.SetBool(parameterName, next);
        Debug.Log("Estado actual del menú: " + current);
        
        isMenuVisible = !isMenuVisible;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;


public class MenuManager : MonoBehaviour
{
    [Header("Objeto del menú (con ScaleInterpolator)")]
    public GameObject menuObject;

    [Header("Distancia para posicionar el menú frente al usuario")]
    public float menuDistance = 2.0f;
    public InputActionReference menuButton; // Referencia al controlador XR
    
    public float activationThreshold = 0.1f;
    // Bandera para llevar el estado del menú (visible o no)
    private bool isMenuVisible = false;
    public bool vrenabled = false;
    private HandlerFlags handlerFlags;
    private SpeechAssistantControllerWS speechAssistantControllerWS;

    void Start()
    {
       
        handlerFlags = FindFirstObjectByType<HandlerFlags>();
        speechAssistantControllerWS = FindFirstObjectByType<SpeechAssistantControllerWS>();
    }
    void Update()
    {
        // Para dispositivos Quest se usa OVRInput. En el editor usamos la barra espaciadora.
    
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
        if (cam == null)
        {
            Debug.LogWarning("No se encontró la cámara principal.");
            return;
        }

        // Si el menú se va a mostrar (popOn), reposicionarlo frente al usuario
        if (!isMenuVisible)
        {
            // Calcula la posición a 'menuDistance' unidades delante de la cámara.
            Vector3 targetPosition = cam.transform.position + cam.transform.forward * menuDistance;
            menuObject.transform.position = targetPosition;

            // Rota el menú para que siempre mire hacia la cámara.
            // Se calcula la dirección desde el menú hacia la cámara.
            Vector3 directionToCamera = cam.transform.position - menuObject.transform.position;
            if (directionToCamera != Vector3.zero)
            {
                // Se aplica una rotación extra de 180° en Y para corregir que se vea la parte frontal.
                menuObject.transform.rotation = Quaternion.LookRotation(directionToCamera) * Quaternion.Euler(0, 180, 0);
            }
        }

        // Ejecuta la animación usando el componente ScaleInterpolator del objeto de menú.
        ScaleInterpolator scaleInterp = menuObject.GetComponent<ScaleInterpolator>();
        if (scaleInterp != null)
        {
            if (isMenuVisible)
            {
                scaleInterp.PopOff();
            }
            else
            {
                scaleInterp.PopUp();
            }
        }
        else
        {
            Debug.LogWarning("El objeto del menú no tiene un componente ScaleInterpolator.");
        }

        // Alterna el estado del menú.
        isMenuVisible = !isMenuVisible;
    }
}

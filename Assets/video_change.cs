using UnityEngine;

public class video_change : MonoBehaviour
{
    public GameObject firstObject;
    public GameObject secondObject;
    
    public bool switchObjects = false;
    
    // No se hace nada en Start, la activación se hará desde otro script
    void Start()
    {
        // No hacemos nada al inicio, se controlará desde otro script
    }
    
    // Verificar cambios en cada frame
    void Update()
    {
        // Si el booleano es true, asegúrate de que el segundo objeto esté activo y el primero inactivo
        if (switchObjects)
        {
            if (firstObject != null && firstObject.activeSelf)
            {
                firstObject.SetActive(false);
            }
            
            if (secondObject != null && !secondObject.activeSelf)
            {
                secondObject.SetActive(true);
            }
        }
    }
    
    // Método simple para activar el segundo objeto y desactivar el primero
    public void ActivateSecondObject()
    {
        switchObjects = true;
        
        if (firstObject != null)
        {
            firstObject.SetActive(false);
        }
        
        if (secondObject != null)
        {
            secondObject.SetActive(true);
        }
    }
}

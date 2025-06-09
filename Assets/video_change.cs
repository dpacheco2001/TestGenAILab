using UnityEngine;

public class video_change : MonoBehaviour
{
    public GameObject firstObject;
    public GameObject secondObject;
    
    public bool switchObjects = false;
    private bool hasAlreadySwitched = false; // Nueva variable para controlar que solo se ejecute una vez
    
    // No se hace nada en Start, la activación se hará desde otro script
    void Start()
    {
        // No hacemos nada al inicio, se controlará desde otro script
    }
    
    // Ya no necesitamos Update() porque solo queremos que se ejecute una vez
    // void Update() - REMOVIDO
    
    // Método simple para activar el segundo objeto y desactivar el primero - SOLO UNA VEZ
    public void ActivateSecondObject()
    {
        // Solo ejecutar si no se ha cambiado antes
        if (!hasAlreadySwitched)
        {
            switchObjects = true;
            hasAlreadySwitched = true; // Marcar que ya se ejecutó
            
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
    
    // Método opcional para resetear el estado si necesitas volver a permitir el cambio
    public void ResetSwitch()
    {
        hasAlreadySwitched = false;
        switchObjects = false;
    }
}

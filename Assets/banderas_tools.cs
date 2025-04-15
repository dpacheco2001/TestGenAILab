using UnityEngine;

public class banderas_tools : MonoBehaviour
{
    [Header("GameObject Control")]
    public GameObject objetoADesactivar;

    public void DesactivarObjeto(GameObject objetoADesactivar)
    {
        if (objetoADesactivar != null)
        {
            objetoADesactivar.SetActive(false);
        }
    }
}

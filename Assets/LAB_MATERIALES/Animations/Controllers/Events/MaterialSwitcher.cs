using UnityEngine;

public class MaterialSwitcher : MonoBehaviour
{
    public Material newMaterial;

    public void SwitchMaterial()
    {
        Renderer rend = GetComponent<Renderer>();
        if(rend != null && newMaterial != null)
        {
            rend.material = newMaterial;
        }
    }
}

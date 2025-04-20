using UnityEngine;
using System.Collections.Generic;

public class UIButtonToggleManager : MonoBehaviour
{
    [Header("Animator")]
    public Animator targetAnimator;
    public string parameterName = "isActive";

    [Header("Objetos a togglear")]
    public List<GameObject> toggleObjects;

    // Este método lo conectas al OnClick() de tu Button
    public void ToggleEverything()
    {
        // 1) Obtiene el valor actual y lo invierte
        bool current = targetAnimator.GetBool(parameterName);
        bool next    = !current;

        // 2) Setea el parámetro en el Animator
        targetAnimator.SetBool(parameterName, next);

        // 3) Activa o desactiva todos los GameObjects de la lista
        foreach (var go in toggleObjects)
        {
            if (go != null)
                go.SetActive(!go.activeSelf);
        }
    }
}
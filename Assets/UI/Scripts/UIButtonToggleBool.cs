using System.Collections.Generic;
using UnityEngine;

public class ToggleScaleController : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator targetAnimator;
    public string    parameterName;

    [Header("UI Elements to Toggle")]
    public List<GameObject> toggleObjects;


    public void ToggleEverything()
    {

        bool current = targetAnimator.GetBool(parameterName);
        bool next    = !current;
        targetAnimator.SetBool(parameterName, next);

  
        foreach (var go in toggleObjects)
        {
            if (go == null) 
                continue;

            Vector3 scale = go.transform.localScale;

            bool isZero = scale.sqrMagnitude < 0.0001f;
            go.transform.localScale = isZero ? Vector3.one : Vector3.zero;
        }
    }
}

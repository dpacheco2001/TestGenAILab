using System.Collections;
using UnityEngine;

public class ScaleInterpolator : MonoBehaviour
{
    [Header("Duración de la animación (segundos)")]
    public float animationDuration = 1.0f;

    [Header("¿Debe iniciar visible?")]
    public bool startPopped = false;

    private Coroutine scalingCoroutine;
    private bool isPopUp = false;

    void Start()
    {
        if (startPopped)
        {
            transform.localScale = Vector3.one;
            isPopUp = true;
        }
        else
        {
            transform.localScale = Vector3.zero;
            isPopUp = false;
        }
    }

    public void PopUp()
    {
        if (Vector3.Distance(transform.localScale, Vector3.one) < 0.01f)
            return;

        if (scalingCoroutine != null)
            StopCoroutine(scalingCoroutine);

        scalingCoroutine = StartCoroutine(ScaleCoroutine(transform.localScale, Vector3.one, animationDuration));
        isPopUp = true;
    }

    public void PopOff()
    {
        if (Vector3.Distance(transform.localScale, Vector3.zero) < 0.01f)
            return;

        if (scalingCoroutine != null)
            StopCoroutine(scalingCoroutine);

        scalingCoroutine = StartCoroutine(ScaleCoroutine(transform.localScale, Vector3.zero, animationDuration));
        isPopUp = false;
    }

    public void TogglePop()
    {
        if (isPopUp)
            PopOff();
        else
            PopUp();
    }

    private IEnumerator ScaleCoroutine(Vector3 startScale, Vector3 endScale, float duration)
    {
        float elapsed = 0f;
        transform.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;
        scalingCoroutine = null;
    }
}

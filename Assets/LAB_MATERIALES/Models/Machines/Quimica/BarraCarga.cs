// LoadingBar.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using VirtualGrasp.Onboarding;

public class BarraCarga : MonoBehaviour
{
    //public ButtonInteraction buttonInteraction; // Referencia al script de interacción del botón
    public Image loadingImage; // Referencia a la imagen de la barra de carga
    public float targetWidth = 100f; // Ancho objetivo en píxeles
    public float duration = 5f; // Duración en segundos
    public bool isLoadingComplete; // Variable pública para indicar si la carga está completa

    private float originalWidth;
    private RectTransform rectTransform;
    private bool isCoroutineRunning = false;

    void Start()
    {
        rectTransform = loadingImage.GetComponent<RectTransform>();
        originalWidth = rectTransform.sizeDelta.x;
    }

    // Este método público puede ser llamado desde el botón en Unity
    public void StartLoading()
    {
        if (!isCoroutineRunning)
        {
            Debug.Log("Starting IncreaseWidth coroutine");
            StartCoroutine(IncreaseWidth());
        }
    }

    private IEnumerator IncreaseWidth()
    {
        Debug.Log("Corutina iniciada."); // Depuración
        isCoroutineRunning = true;
        isLoadingComplete = false;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newWidth = Mathf.Lerp(originalWidth, targetWidth, elapsedTime / duration);
            rectTransform.sizeDelta = new Vector2(newWidth, rectTransform.sizeDelta.y);
            Debug.Log($"Elapsed time: {elapsedTime}, New width: {newWidth}");
            yield return null;
        }

        // Al finalizar, se establece isLoadingComplete en true y luego se reinicia el contador
        isLoadingComplete = true;
        yield return new WaitForSeconds(0.5f); // Pausa para indicar que la carga está completa

        // Reinicia la barra y el estado de la carga
        rectTransform.sizeDelta = new Vector2(originalWidth, rectTransform.sizeDelta.y);
        isLoadingComplete = false;
        isCoroutineRunning = false; // Aquí aseguramos que el valor se resetee correctamente
        Debug.Log("IncreaseWidth coroutine completed and reset.");
    }
}

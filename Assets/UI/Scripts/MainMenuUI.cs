using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        
        var styleSheet = Resources.Load<StyleSheet>("UI/Styles/MainMenu");
        if (styleSheet != null)
        {
            root.styleSheets.Add(styleSheet);
        }

        
        var jugarBtn = root.Q<Button>("btn-jugar");
        var opcionesBtn = root.Q<Button>("btn-opciones");
        var salirBtn = root.Q<Button>("btn-salir");

        jugarBtn.clicked += () => SceneManager.LoadScene("NombreDeTuEscena"); 
        opcionesBtn.clicked += () => Debug.Log("Opciones pulsado"); 
        salirBtn.clicked += () => {
            Application.Quit();
            Debug.Log("Salir pulsado");
        };
    }
}

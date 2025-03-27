using UnityEngine;

public class CheckVisibleObjectsHotZone : MonoBehaviour
{

    public string tagName = "TuTag";
    public Camera cam;
    public Vector2 hotZoneCenter = new Vector2(0.5f, 0.5f);
    public float hotZoneRadius = 0.2f;
    public Texture2D hotZoneTexture;
    void Start()
    {
        if (cam == null)
            cam = Camera.main;


        if (hotZoneTexture == null)
        {
            hotZoneTexture = CreateCircleTexture(256);
        }
    }

    void Update()
    {

        GameObject[] objects = GameObject.FindGameObjectsWithTag(tagName);

        foreach (GameObject obj in objects)
        {
            
            Vector3 viewportPos = cam.WorldToViewportPoint(obj.transform.position);
            if (viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1)
            {
                float distance = Vector3.Distance(cam.transform.position, obj.transform.position);
                Debug.LogWarning(obj.name + " está a " + distance + " unidades de distancia.");
                // Calcula la distancia (cuadrática) entre la posición del objeto y el centro de la zona caliente
                float dx = viewportPos.x - hotZoneCenter.x;
                float dy = viewportPos.y - hotZoneCenter.y;
                if (dx * dx + dy * dy <= hotZoneRadius * hotZoneRadius)
                {
                    Debug.LogWarning(obj.name + " está dentro de la zona caliente.");
                    // Aquí podrías disparar la alerta o ejecutar otra lógica
                }
            }
        }
    }

    // Método para dibujar el círculo en pantalla
    void OnGUI()
    {
        if (cam == null || hotZoneTexture == null)
            return;

        // Convertimos el centro de la zona caliente (en viewport) a coordenadas de pantalla
        Vector2 screenPos = new Vector2(hotZoneCenter.x * Screen.width, (1 - hotZoneCenter.y) * Screen.height);
        // Calculamos el radio en píxeles (usamos el ancho de la pantalla como referencia)
        float radiusPixels = hotZoneRadius * Screen.width;
        // Definimos el rectángulo en el que se dibujará la textura (centrado en screenPos)
        Rect rect = new Rect(screenPos.x - radiusPixels, screenPos.y - radiusPixels, radiusPixels * 2, radiusPixels * 2);
        GUI.DrawTexture(rect, hotZoneTexture);
    }

    // Método auxiliar para crear una textura circular (círculo con borde)
    Texture2D CreateCircleTexture(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color circleColor = Color.red;
        // Inicializa la textura con píxeles transparentes
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, clear);
            }
        }
        // Dibuja el borde del círculo
        int radius = size / 2;
        int thickness = 4;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = x - radius;
                int dy = y - radius;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist >= radius - thickness && dist <= radius)
                {
                    tex.SetPixel(x, y, circleColor);
                }
            }
        }
        tex.Apply();
        return tex;
    }
}

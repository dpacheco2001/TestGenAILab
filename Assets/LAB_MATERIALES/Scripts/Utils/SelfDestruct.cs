using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [Tooltip("Tiempo en segundos antes de autodestruir el objeto.")]
    public float lifeTime = 20;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
using UnityEngine;
using System.Collections.Generic;

public class HandlerFlags : MonoBehaviour
{
    [System.Serializable]
    public class FlagObject
    {
        public string nombre;
        public GameObject objeto;
        [SerializeField]
        private bool _enabled = false;

        public bool enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    if (objeto != null)
                    {
                        objeto.SetActive(value);
                    }
                }
            }
        }

        // Este método se llama cuando se modifica el valor en el Inspector
        private void OnValidate()
        {
            if (objeto != null)
            {
                objeto.SetActive(_enabled);
            }
        }
    }

    [Header("Objetos Controlados")]
    public List<FlagObject> objetosControlados = new List<FlagObject>();

    private void Start()
    {
        // Asegurarse de que todos los objetos estén en el estado correcto al inicio
        foreach (var obj in objetosControlados)
        {
            if (obj.objeto != null)
            {
                obj.objeto.SetActive(obj.enabled);
            }
        }
    }

    private void Update()
    {
        // Verificar continuamente los estados de los objetos durante el juego
        foreach (var obj in objetosControlados)
        {
            if (obj.objeto != null)
            {
                // Si el estado del GameObject no coincide con el estado enabled, actualizarlo
                if (obj.objeto.activeInHierarchy != obj.enabled)
                {
                    obj.objeto.SetActive(obj.enabled);
                }
            }
        }
    }

    // Activa un objeto específico por nombre
    public void ActivarObjeto(string nombreObjeto)
    {
        FlagObject obj = objetosControlados.Find(x => x.nombre == nombreObjeto);
        if (obj != null && obj.objeto != null)
        {
            obj.enabled = true;
        }
    }

    // Marca un objeto como desactivado
    public void MarcarComoDesactivado(string nombreObjeto)
    {
        FlagObject obj = objetosControlados.Find(x => x.nombre == nombreObjeto);
        if (obj != null)
        {
            obj.enabled = false;
        }
    }

    // Verifica el estado de un objeto
    public bool EstadoObjeto(string nombreObjeto)
    {
        FlagObject obj = objetosControlados.Find(x => x.nombre == nombreObjeto);
        return obj != null && obj.enabled;
    }

    public void EnableAndDisableObject(string nombreObjeto, bool enable)
    {
        FlagObject obj = objetosControlados.Find(x => x.nombre == nombreObjeto);    
        if (obj != null)
        {
            obj.enabled = enable;
        }
    }
}


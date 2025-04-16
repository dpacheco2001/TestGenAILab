using UnityEngine;

public class TestObjetos : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public HandlerFlags _handlerFlags;
    public bool _enabled = true;
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
    
        
    }

    private void OnValidate()
    {
        if (_handlerFlags != null)
        {
            _handlerFlags.ActivarObjeto("CUBO"); 
        }
    }

    private void OnDisable()
    {
        if (_handlerFlags != null)
        {
            _handlerFlags.MarcarComoDesactivado("CUBO"); 
        }
    }

}

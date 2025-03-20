using System;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using BNG;
public class UnityHttpServer : MonoBehaviour
{
    // El puerto en el que escuchará el servidor
    public int port = 5000;

    // La variable que queremos exponer (puedes actualizarla en tiempo real)
    public string myVariable = "Valor inicial";

    public Marker marker;
    private HttpListener listener;
    private Thread listenerThread;

    void Start()
    {
        // Inicia el servidor en un hilo separado
        listener = new HttpListener();
        // Escuchamos en todas las IP locales en el puerto especificado
        listener.Prefixes.Add($"http://*:{port}/");
        try
        {
            listener.Start();
            Debug.Log("HTTP Server started on port " + port);
        }
        catch(Exception ex)
        {
            Debug.LogError("Error starting HTTP Server: " + ex.Message);
            return;
        }
        listenerThread = new Thread(HandleIncomingConnections);
        listenerThread.Start();
    }

    // Método que se ejecuta en un hilo separado para procesar solicitudes
    void HandleIncomingConnections()
    {
        while (listener.IsListening)
        {
            try
            {
                // Bloquea hasta que llega una solicitud
                HttpListenerContext context = listener.GetContext();
                ProcessRequest(context);
            }
            catch (Exception ex)
            {
                Debug.LogError("Error en el listener: " + ex.Message);
            }
        }
    }

    // Procesa la solicitud y envía una respuesta
    void ProcessRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;
        string responseString = "";

        // Sólo manejamos solicitudes GET
        if (request.HttpMethod == "GET")
        {
            // Por ejemplo, respondemos a la ruta "/solicitud"
            if (request.Url.AbsolutePath == "/verificar_zona_relevante")
            {
                myVariable = marker.GetEnclosedObjectsAndClearStrokes();
                responseString = myVariable;
            }
            else
            {
                responseString = "Endpoint no encontrado.";
            }
        }
        else
        {
            responseString = "Solo se soporta GET.";
        }

        byte[] buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        try
        {
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Debug.LogError("Error escribiendo la respuesta: " + ex.Message);
        }
        finally
        {
            response.OutputStream.Close();
        }
    }

    void OnDestroy()
    {
        // Detén el listener y el hilo al destruir el objeto
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
            listener.Close();
        }
        if (listenerThread != null && listenerThread.IsAlive)
        {
            listenerThread.Abort();
        }
    }
}

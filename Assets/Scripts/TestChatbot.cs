using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class TestChatbot : MonoBehaviour
{
    // URL to make requests to
    [SerializeField] private string baseUrl = "https://api.example.com";
    [SerializeField] private string apiKey;

    [SerializeField] private bool send = false;

    /// <summary>
    /// Makes a GET request to the specified endpoint
    /// </summary>
    public void MakeGetRequest(string endpoint, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(GetRequest(endpoint, onSuccess, onError));
    }

    /// <summary>
    /// Makes a POST request to the specified endpoint with JSON payload
    /// </summary>
    public void MakePostRequest(string endpoint, string jsonData, Action<string> onSuccess, Action<string> onError)
    {
        StartCoroutine(PostRequest(endpoint, jsonData, onSuccess, onError));
    }

    private IEnumerator GetRequest(string endpoint, Action<string> onSuccess, Action<string> onError)
    {
        string url = baseUrl + endpoint;
        
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Send the request and wait for response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                onError?.Invoke(webRequest.error);
            }
            else
            {
                // Request successful, get the response
                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"Received: {responseText}");
                onSuccess?.Invoke(responseText);
            }
        }
    }

    private IEnumerator PostRequest(string endpoint, string jsonData, Action<string> onSuccess, Action<string> onError)
    {
        string url = baseUrl + endpoint;
        
        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the request and wait for response
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || 
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error: {webRequest.error}");
                onError?.Invoke(webRequest.error);
            }
            else
            {
                // Request successful, get the response
                string responseText = webRequest.downloadHandler.text;
                Debug.Log($"Received: {responseText}");
                onSuccess?.Invoke(responseText);
            }
        }
    }

    // Example usage
    public void ExampleUsage()
    {
        // Example GET request
        MakeGetRequest("/users", 
            (response) => { Debug.Log("GET success: " + response); },
            (error) => { Debug.LogError("GET error: " + error); }
        );

        // Example POST request
        string jsonPayload = "{\"name\":\"John\",\"age\":30}";
        MakePostRequest("/users", jsonPayload,
            (response) => { Debug.Log("POST success: " + response); },
            (error) => { Debug.LogError("POST error: " + error); }
        );
    }

    public void Start()
    {
        ExampleUsage();
    }

    public void Update()
    {
        if(send)
        {
            send = false;
            ExampleUsage();
        }
    }
}
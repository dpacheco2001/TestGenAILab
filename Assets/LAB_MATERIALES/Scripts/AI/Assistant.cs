using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System;

public class Assistant : MonoBehaviour
{
    [SerializeField]
    private string baseUrl = "https://2483-38-253-189-83.ngrok-free.app";
    
    [SerializeField]
    private string endpoint = "/process";

    [SerializeField]
    public string message = "Hola";

    [SerializeField]
    public string code = "20190051";

    [SerializeField]
    public string response = "";
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assistant assistant = GetComponent<Assistant>();
        assistant.SendRequest(message, code, (assistantResponse, fullResponse) => {
            Debug.Log("Response received: " + fullResponse);
            assistant.response = assistantResponse; // Store only the assistant part
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendRequest(string message, string code, Action<string, string> callback = null)
    {
        StartCoroutine(SendPostRequest(message, code, callback));
    }

    private IEnumerator SendPostRequest(string message, string code, Action<string, string> callback)
    {
        string url = baseUrl + endpoint;
        string jsonBody = JsonUtility.ToJson(new RequestData
        {
            message = message,
            codigo = code
        });
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                
                try {
                    ResponseData responseData = JsonUtility.FromJson<ResponseData>(responseText);
                    string formattedResponse = FormatResponse(responseData);
                    Debug.Log("Request successful: " + formattedResponse);
                    callback?.Invoke(responseData.assistant, formattedResponse);
                }
                catch (Exception e) {
                    Debug.LogWarning("Error parsing response: " + e.Message);
                    Debug.Log("Raw response: " + responseText);
                    callback?.Invoke(responseText, responseText);
                }
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
                callback?.Invoke(null, null);
            }
        }
    }

    private string FormatResponse(ResponseData data)
    {
        if (data == null) return "No data";
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Response:");
        sb.AppendLine($"Assistant: {data.assistant}");
        sb.AppendLine($"Human: {data.human}");
        return sb.ToString();
    }

    [Serializable]
    private class RequestData
    {
        public string message;
        public string codigo;
    }
    
    [Serializable]
    private class ResponseData
    {
        public string assistant;
        public string human;
    }
}
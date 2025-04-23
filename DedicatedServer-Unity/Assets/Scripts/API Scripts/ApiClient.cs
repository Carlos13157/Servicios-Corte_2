using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ApiClient : MonoBehaviour {
    public string baseUrl = "http://localhost:5005/server";
    public event Action<int, ServerData> OnDataReceived;

    private Queue<ApiRequest> requestQueue = new Queue<ApiRequest>();
    private bool isProcessing = false;

    public int maxConcurrentRequests = 2;
    private int currentRequests = 0;

    private void Update() {
        while (currentRequests < maxConcurrentRequests && requestQueue.Count > 0) {
            StartCoroutine(ProcessNextRequest());
        }
    }

    private IEnumerator ProcessNextRequest() {
        currentRequests++;
        var request = requestQueue.Dequeue();
        yield return request.Execute(this);
        currentRequests--;
    }


    public void EnqueueGet(string gameId, string playerId) {
        requestQueue.Enqueue(new ApiGetRequest(gameId, playerId));
    }

    public void EnqueuePost<T>(string gameId, string playerId, T data) {
        requestQueue.Enqueue(new ApiPostRequest<T>(gameId, playerId, data));
    }


    public void EnqueueLog(string playerId, string action) {
        PlayerLog log = new PlayerLog {
            playerId = playerId,
            action = action,
            timestamp = DateTime.UtcNow.ToString("o") // ISO 8601
        };

        string gameId = "logs";
        requestQueue.Enqueue(new ApiPostRequest<PlayerLog>("logs", playerId, log));

        Debug.Log("Log enviado");

    }



    // Métodos reales para enviar GET y POST
    public IEnumerator PerformGet(string gameId, string playerId) {
        string url = $"{baseUrl}/{gameId}/{playerId}";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"GET Error: {webRequest.error}");
                Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            }
            else {
                /* Debug.Log($"GET Success: {webRequest.downloadHandler.text}"); */
                var data = JsonUtility.FromJson<ServerData>(webRequest.downloadHandler.text);
                OnDataReceived?.Invoke(Convert.ToInt16(playerId), data);
            }
        }
    }

    public IEnumerator PerformPost<T>(string gameId, string playerId, T data) {
        string url = $"{baseUrl}/{gameId}/{playerId}";
        string jsonData = JsonUtility.ToJson(data);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"POST Error: {webRequest.error}");
                Debug.LogError($"Response: {webRequest.downloadHandler.text}");
            }
            else {
                Debug.Log($"POST Success: {webRequest.downloadHandler.text}");
            }
        }
    }

}

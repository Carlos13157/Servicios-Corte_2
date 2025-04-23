using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum Mode {
    Local,
    Online
}


public class PlayerController : MonoBehaviour {
    public int playerId;
    public float speed = 4.5f;
    public Mode mode;
    public CharacterController CharCtrl => GetComponent<CharacterController>();
    public bool AllowInput = true;
    private Vector3 lastPosition;
    private Vector3 targetPosition;
    public float lerpSpeed = 5f; // Puedes ajustar la suavidad con este valor

    private ApiClient apiClient;

    void Start() {
        lastPosition = GetPosition();
        apiClient = FindObjectOfType<ApiClient>();

        if (mode == Mode.Online) {
            StartCoroutine(UpdateFromServer());
        }

        if (mode == Mode.Local) {
        apiClient = FindObjectOfType<ApiClient>();

        PlayerLog log = new PlayerLog {
            playerId = playerId.ToString(),
            action = "Test log desde Start()",
            timestamp = DateTime.UtcNow.ToString("o")
        };

        apiClient.EnqueuePost("logs", log.playerId, log);
    }
    }


    public void MovePlayer(Vector3 position) {
        transform.position = position;
    }

    public Vector3 GetPosition() {
        return transform.position;
    }

    void Update() {
        if (mode == Mode.Local && AllowInput) {
            playerInput();
            DetectMovementAndPost();
        }
        else if (mode == Mode.Online) {
            DetectMovementAndLog();
            SmoothMoveToTarget();
        }
    }

    void OnEnable() {
        if (mode == Mode.Online) {
            FindObjectOfType<ApiClient>().OnDataReceived += OnServerDataReceived;
        }
    }

    void OnDisable() {
        if (mode == Mode.Online) {
            FindObjectOfType<ApiClient>().OnDataReceived -= OnServerDataReceived;
        }
    }

    void OnServerDataReceived(int id, ServerData data) {
        if (id == playerId) {
            targetPosition = new Vector3(data.posX, data.posY, data.posZ);
        }
    }

    private void SmoothMoveToTarget() {
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * lerpSpeed);
    }



    private void playerInput() {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        var movement = new Vector3(moveX, 0, moveY);

        CharCtrl.Move(movement * speed * Time.deltaTime);
    }

    private void DetectMovementAndPost() {
        Vector3 currentPosition = GetPosition();

        if (Vector3.Distance(currentPosition, lastPosition) > 0.0001f) {
            lastPosition = currentPosition;

            // Solo si el ApiClient está asignado
            if (apiClient != null) {
                ServerData data = new ServerData {
                    posX = currentPosition.x,
                    posY = currentPosition.y,
                    posZ = currentPosition.z
                };
                apiClient.EnqueuePost("game1", playerId.ToString(), data);
            }
        }
    }

    private void DetectMovementAndLog() {
        Vector3 currentPosition = GetPosition();

        if (Vector3.Distance(currentPosition, lastPosition) > 0.01f) {
            lastPosition = currentPosition;

            if (apiClient != null) {
                apiClient.EnqueueLog(playerId.ToString(), $"Moved to {currentPosition}");
            }
        }
    }


    private IEnumerator UpdateFromServer() {
        while (true) {
            if (apiClient != null) {
                apiClient.EnqueueGet("game1", playerId.ToString());
            }
            yield return new WaitForSeconds(0.01f); // cada 100 ms
        }
    }
}
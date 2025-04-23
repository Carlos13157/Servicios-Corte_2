using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private ApiClient api;
    [SerializeField] private List<PlayerController> players;
    public string gameId;

    public void Start() {
        api.OnDataReceived += OnDataReceived;
    }

    void Update() {
        /* foreach (var player in players) {
            if (player.mode == Mode.Local)
                SendPlayerPosition(player.playerId);
        }

        foreach (var player in players) {
            if (player.mode == Mode.Online)
                GetPlayerData(player.playerId);
        } */
    }

    public void GetPlayerData(int playerId) {
        api.EnqueueGet(gameId, playerId.ToString());
    }

    public void OnDataReceived(int playerId, ServerData data) {
        Vector3 position = new Vector3(data.posX, data.posY, data.posZ);
        players[playerId].MovePlayer(position);
    }

    public void SendPlayerPosition(int playerId) {
        Vector3 position = players[playerId].GetPosition();
        ServerData data = new() {
            posX = position.x,
            posY = position.y,
            posZ = position.z
        };

        api.EnqueuePost(gameId, playerId.ToString(), data);
    }
}
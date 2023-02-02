using System;
using DefaultNamespace;
using UnityEngine;
using static ConnectionNotificationManager;

public class ScoreManager : MonoBehaviour {
    public static ScoreManager Singleton { get; private set; }

    [SerializeField] private RectTransform scorePanel;
    [SerializeField] private PlayerScore[] PlayerScores;

    private void Awake() {
        if (Singleton != null) {
            Debug.LogError($"There's more than one ScoreManager in the scene! {transform} - {Singleton}");
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        scorePanel.gameObject.SetActive(false);
    }

    private void Start() {
        if (ConnectionNotificationManager.Singleton == null) {
            throw new Exception($"There is no {nameof(ConnectionNotificationManager)} for the {nameof(ScoreManager)} to do stuff with! Please add a {nameof(ConnectionNotificationManager)} to the scene.");
        }
        ConnectionNotificationManager.Singleton.OnClientConnectionNotification += OnClientConnectionEvent;
    }

    private void OnClientConnectionEvent(ulong clientId, ConnectionStatus connectionStatus) {
        Debug.Log($"client {clientId} - {connectionStatus}");

        if (connectionStatus == ConnectionStatus.Connected && clientId == ConnectionNotificationManager.Singleton.GetMyClientId()) {
            scorePanel.gameObject.SetActive(true);
            //Assume first connection, check other players
            var connectedClientIds = ConnectionNotificationManager.Singleton.GetConnectedClientIds();
            foreach (var aClientId in connectedClientIds) {
                PlayerScores[Convert.ToInt32(aClientId)].gameObject.SetActive(true);
            }
        }
        var player = Convert.ToInt32(clientId);
        PlayerScores[player].gameObject.SetActive(connectionStatus == ConnectionStatus.Connected);
    }

    public void ShowScoreBoard() { }
}
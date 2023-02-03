using System;
using DefaultNamespace;
using UnityEngine;
using static managers.ConnectionNotificationManager;

namespace managers {
    public class ScoreManager : MonoBehaviour {
        public static ScoreManager Singleton { get; private set; }

        [SerializeField] private RectTransform scorePanel;
        [SerializeField] private PlayerScore[] playerScores;

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
                throw new Exception(
                    $"There is no {nameof(ConnectionNotificationManager)} for the {nameof(ScoreManager)} to do stuff with! Please add a {nameof(ConnectionNotificationManager)} to the scene.");
            }

            ConnectionNotificationManager.Singleton.OnClientConnectionNotification += OnClientConnectionEvent;
            ConnectionNotificationManager.Singleton.OnPlayerListResponse += EnableScorePanels;
        }

        private void OnClientConnectionEvent(ulong clientId, ConnectionStatus connectionStatus) {
            Debug.Log($"client {clientId} - {connectionStatus}");

            if (connectionStatus == ConnectionStatus.Connected && clientId == ConnectionNotificationManager.Singleton.GetMyClientId()) {
                scorePanel.gameObject.SetActive(true);
                //Assume first connection, check other players
                ConnectionNotificationManager.Singleton.RequestConnectedClientIds();
            }

            var player = Convert.ToInt32(clientId);
            playerScores[player].gameObject.SetActive(connectionStatus == ConnectionStatus.Connected);
        }

        private void EnableScorePanels(ulong[] players) {
            foreach (var aClientId in players) {
                playerScores[Convert.ToInt32(aClientId)].gameObject.SetActive(true);
            }
        }

        private void OnDestroy() {
            if (ConnectionNotificationManager.Singleton == null) return;
            ConnectionNotificationManager.Singleton.OnClientConnectionNotification -= OnClientConnectionEvent;
            ConnectionNotificationManager.Singleton.OnPlayerListResponse -= EnableScorePanels;
        }
    }
}
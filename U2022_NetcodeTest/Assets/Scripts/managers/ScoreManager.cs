using System;
using System.Linq;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using static managers.ConnectionNotificationManager;

namespace managers {
    public class ScoreManager : MonoBehaviour{
        public static ScoreManager Singleton { get; private set; }

        [SerializeField] private RectTransform scorePanel;
        [SerializeField] private PlayerScore[] playerScorePanels;
        
        private void Awake() {
            if (Singleton != null) {
                Debug.LogError($"There's more than one ScoreManager in the scene! {transform} - {Singleton}");
                Destroy(gameObject);
                return;
            }

            Singleton = this;
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
            var player = Convert.ToInt32(clientId);
            playerScorePanels[player].gameObject.SetActive(connectionStatus == ConnectionStatus.Connected);
            if (NetworkManager.Singleton.IsServer) {
                Debug.Log($"I AM SERVER!!!");
                ConnectionNotificationManager.Singleton.UpdateConnectedClientsClientRpc(NetworkManager.Singleton.ConnectedClientsIds.ToArray());
            }
        }

        private void EnableScorePanels(ulong[] players) {
            Debug.Log($"ENABLE PANELS({players.Length}) {players}");
            foreach (var aClientId in players) {
                playerScorePanels[Convert.ToInt32(aClientId)].gameObject.SetActive(true);
            }
        }

        private void OnDestroy() {
            if (ConnectionNotificationManager.Singleton == null) return;
            ConnectionNotificationManager.Singleton.OnClientConnectionNotification -= OnClientConnectionEvent;
            ConnectionNotificationManager.Singleton.OnPlayerListResponse -= EnableScorePanels;
        }
    }
}
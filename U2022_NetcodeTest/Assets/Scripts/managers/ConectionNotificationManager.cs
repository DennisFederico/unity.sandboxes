using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace managers {
    public class ConnectionNotificationManager : NetworkBehaviour {
        public static ConnectionNotificationManager Singleton { get; internal set; }

        public enum ConnectionStatus {
            Connected,
            Disconnected
        }

        public event Action<ulong, ConnectionStatus> OnClientConnectionNotification;
        public event Action<ulong[]> OnPlayerListResponse;

        private void Awake() {
            if (Singleton != null) {
                Debug.LogError($"There's more than one ScoreManager in the scene! {transform} - {Singleton}");
                //throw new Exception($"Detected more than one instance of {nameof(ConnectionNotificationManager)}! Do you have more than one component attached to a {nameof(GameObject)}");
                Destroy(gameObject);
            }

            Singleton = this;
        }

        private void Start() {
            if (Singleton != this) {
                return; // so things don't get even more broken if this is a duplicate >:(
            }

            if (NetworkManager.Singleton == null) {
                // Can't listen to something that doesn't exist >:(
                throw new Exception(
                    $"There is no {nameof(NetworkManager)} for the {nameof(ConnectionNotificationManager)} to do stuff with! Please add a {nameof(NetworkManager)} to the scene.");
            }

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
        }

        public override void OnDestroy() {
            if (NetworkManager.Singleton != null) {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }
            base.OnDestroy();
        }

        private void OnClientConnectedCallback(ulong clientId) {
            OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Connected);
        }

        private void OnClientDisconnectCallback(ulong clientId) {
            OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Disconnected);
        }

        public ulong GetMyClientId() {
            return NetworkManager.Singleton.IsConnectedClient ? NetworkManager.Singleton.LocalClientId : Convert.ToUInt64(null);
        }

        // public void RequestConnectedClientIds() {
        //     GetConnectedClientIdsServerRpc();
        // }

        [ServerRpc]
        private void SendConnectedClientIdsServerRpc() {
            Debug.Log("ServerRPC - GetConnectedClientIdsServerRpc");
            var clientIds = NetworkManager.Singleton.ConnectedClientsIds.ToArray();
            UpdateConnectedClientsClientRpc(clientIds);
        }

        [ClientRpc]
        public void UpdateConnectedClientsClientRpc(ulong[] clientIds) {
            Debug.Log("ClientRPC - UpdateConnectedClientsClientRpc");
            OnPlayerListResponse?.Invoke(clientIds);
        }
    }
}
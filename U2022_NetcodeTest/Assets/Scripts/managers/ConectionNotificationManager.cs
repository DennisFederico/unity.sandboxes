using System;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

namespace managers {
    public class ConnectionNotificationManager : NetworkBehaviour {
        public static ConnectionNotificationManager Singleton { get; private set; }

        public enum ConnectionStatus {
            Connected,
            Disconnected
        }

        public event Action<ulong[]> OnPlayerListUpdate;
        
        //TODO A LIST OF THE CURRENTLY CONNECTED PLAYERS?

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
        }

        public override void OnNetworkSpawn() {
            if (IsServer) {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
            }
        }

        public override void OnNetworkDespawn() {
            if (IsServer) {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
            }
        }

        //ONLY THE SERVER IS REGISTERED FOR THESE EVENTS
        private void OnClientConnectedCallback(ulong clientId) {
            UpdateConnectedClientsClientRpc(NetworkManager.Singleton.ConnectedClientsIds.ToArray());
        }

        //ONLY THE SERVER IS REGISTERED FOR THESE EVENTS
        private void OnClientDisconnectCallback(ulong clientId) {
            UpdateConnectedClientsClientRpc(NetworkManager.Singleton.ConnectedClientsIds.ToArray());
        }
        
        [ClientRpc]
        private void UpdateConnectedClientsClientRpc(ulong[] clientIds) {
            OnPlayerListUpdate?.Invoke(clientIds);
        }
    }
}
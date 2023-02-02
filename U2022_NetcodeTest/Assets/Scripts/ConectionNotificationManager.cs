using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ConnectionNotificationManager : MonoBehaviour {
    public static ConnectionNotificationManager Singleton { get; internal set; }

    public enum ConnectionStatus {
        Connected,
        Disconnected
    }

    public event Action<ulong, ConnectionStatus> OnClientConnectionNotification;

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
            throw new Exception($"There is no {nameof(NetworkManager)} for the {nameof(ConnectionNotificationManager)} to do stuff with! Please add a {nameof(NetworkManager)} to the scene.");
        }
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
    }

    private void OnDestroy() {
        // Since the NetworkManager could potentially be destroyed before this component, only 
        // remove the subscriptions if that singleton still exists.
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
    }

    private void OnClientConnectedCallback(ulong clientId) {
        OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Connected);
    }

    private void OnClientDisconnectCallback(ulong clientId) {
        OnClientConnectionNotification?.Invoke(clientId, ConnectionStatus.Disconnected);
    }

    public ulong GetMyClientId() {
        return NetworkManager.Singleton.IsConnectedClient ? NetworkManager.Singleton.LocalClientId : Convert.ToUInt64(-1);
    }

    public IReadOnlyList<ulong> GetConnectedClientIds() {
        var clientIds = NetworkManager.Singleton.ConnectedClientsIds;
        return clientIds;
    }
}
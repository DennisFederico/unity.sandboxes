using System;
using UI;
using Unity.Netcode;
using UnityEngine;

namespace managers {
    public class ScoreManager : NetworkBehaviour {
        public static ScoreManager Singleton { get; private set; }

        [SerializeField] private PlayerScore[] playerScorePanels;
        [SerializeField] private int localScore;

        public event Action<int> NewLocalScore;
        //TODO SHOULD CHANGE TO A STRUCTURE WITH CUSTOM SERIALIZATION, FROM A MAP WITH ID|SCORE INSTEAD OF USING INDEX 
        private NetworkList<int> _playerScores;

        private void Awake() {
            if (Singleton != null) {
                Debug.LogError($"There's more than one ScoreManager in the scene! {transform} - {Singleton}");
                Destroy(gameObject);
                return;
            }
            Singleton = this;
            //NetworkList can't be initialized at declaration time like NetworkVariable. It must be initialized in Awake instead.
            _playerScores = new NetworkList<int>(new[]{0, 0, 0});
        }

        public override void OnNetworkSpawn() {
            _playerScores.OnListChanged += OnScoreChanged;
        }

        private void Start() {
            if (ConnectionNotificationManager.Singleton == null) {
                throw new Exception(
                    $"There is no {nameof(ConnectionNotificationManager)} for the {nameof(ScoreManager)} to do stuff with! Please add a {nameof(ConnectionNotificationManager)} to the scene.");
            }
            ConnectionNotificationManager.Singleton.OnPlayerListUpdate += OnPlayerListUpdate;
        }

        private void OnScoreChanged(NetworkListEvent<int> changeEvent) {
            Debug.Log($"Score change type:{changeEvent.Type.ToString()}");
            //Could use changeEvent.Type and fetch the changed Index and update its value
            for (int i = 0; i < _playerScores.Count; i++) {
                playerScorePanels[i].DisplayScore(_playerScores[i]);
            }
        }

        private void OnPlayerListUpdate(ulong[] players) {
            foreach (var playerScorePanel in playerScorePanels) {
                playerScorePanel.gameObject.SetActive(false);
            }

            foreach (var aClientId in players) {
                playerScorePanels[Convert.ToInt32(aClientId)].gameObject.SetActive(true);
            }
        }

        public void IncrementLocalPlayerScore() {
            localScore += 10;
            NewLocalScore?.Invoke(localScore);
        }
        
        [ServerRpc(RequireOwnership = false)]
        public void IncrementPlayerScoreServerRpc(ulong clientId) {
            Debug.Log($"Server increase score for {clientId}");
            var player = Convert.ToInt32(clientId);
            if (player is > 2 or < 0) {
                Debug.Log("Invalid Player ID");
                return;
            }

            _playerScores[player] += 10;
        }

        public override void OnDestroy() {
            if (ConnectionNotificationManager.Singleton == null) return;
            ConnectionNotificationManager.Singleton.OnPlayerListUpdate -= OnPlayerListUpdate;
        }
    }
}
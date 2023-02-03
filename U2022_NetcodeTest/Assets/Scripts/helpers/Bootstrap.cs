using Unity.Netcode;
using UnityEngine;

namespace helpers {
    public class Bootstrap : MonoBehaviour {

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));

            var networkManager = NetworkManager.Singleton;
            if (!networkManager.IsClient && !networkManager.IsServer) {
                if (GUILayout.Button("Host")) {
                    networkManager.StartHost();
                }

                if (GUILayout.Button("Client")) {
                    networkManager.StartClient();
                }

                if (GUILayout.Button("Server")) {
                    networkManager.StartServer();
                }
            } else {
                GUILayout.Label($"Mode: {(networkManager.IsHost ? "Host" : networkManager.IsServer ? "Server" : "Client")}");
            
                if (networkManager.IsClient || networkManager.IsHost) {
                    if (GUILayout.Button("Spawn Object at Random")) {
                        Spawner.Instance.SpawnRandomObjectServerRpc();
                    }
                }
            }

            GUILayout.EndArea();
        }
    }
}
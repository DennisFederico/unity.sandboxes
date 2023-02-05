using Unity.Netcode;
using UnityEngine;

namespace helpers {
    public class TickDebugger : NetworkBehaviour {
        public override void OnNetworkSpawn() {
            NetworkManager.NetworkTickSystem.Tick += Tick;
        }

        private void Tick() {
            Debug.Log($"Tick: {NetworkManager.LocalTime.Tick}");
        }

        public override void OnNetworkDespawn() {
            NetworkManager.NetworkTickSystem.Tick -= Tick;
        }
    }
}
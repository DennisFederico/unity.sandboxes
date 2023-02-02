using Unity.Netcode;
using UnityEngine;

public class RpcTest : NetworkBehaviour {
    public override void OnNetworkSpawn() {
        if (!IsServer) {
            TestServerRpc(Random.Range(7, 17));
        }
    }

    [ClientRpc]
    void TestClientRpc(int value) {
        if (IsClient) {
            Debug.Log($"CLIENT Received the RPC #{value}");
            TestServerRpc(value + 1);
        }
    }

    [ServerRpc]
    void TestServerRpc(int value) {
        Debug.Log($"SERVER Received the RPC #{value}");
        TestClientRpc(value);
    }
}
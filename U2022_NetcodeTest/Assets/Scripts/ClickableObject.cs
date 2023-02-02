using Unity.Netcode;
using UnityEngine;

public class ClickableObject : NetworkBehaviour {
    
    // public override void OnNetworkSpawn() {
    //     base.OnNetworkSpawn();
    // }
    //
    // public override void OnNetworkDespawn() {
    //     base.OnNetworkDespawn();
    // }

    private void OnMouseDown() {
        Debug.Log($"Someone {NetworkManager.Singleton.LocalClientId} clicked On Me");
        DestroyMeServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyMeServerRPC(ServerRpcParams serverRpcParams = default) {
        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log($"SERVER - Client {clientId} clicked me, award him points!");
        GetComponent<NetworkObject>().Despawn();
    }
}
using managers;
using Unity.Netcode;
using UnityEngine;

public class ClickableObject : NetworkBehaviour {
    
    private void OnMouseDown() {
        Debug.Log($"Mr. player {NetworkManager.Singleton.LocalClientId} clicked On Me");
        ScoreManager.Singleton.IncrementPlayerScoreServerRpc(NetworkManager.Singleton.LocalClientId);
        DestroyMeServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyMeServerRPC(ServerRpcParams serverRpcParams = default) {
        var clientId = serverRpcParams.Receive.SenderClientId;
        Debug.Log($"SERVER - Client {clientId} clicked me, award him points!");
        if (GetComponent<NetworkObject>().IsSpawned) {
            GetComponent<NetworkObject>().Despawn();            
        }
    }
}
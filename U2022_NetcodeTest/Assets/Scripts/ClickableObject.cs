using managers;
using Unity.Netcode;

public class ClickableObject : NetworkBehaviour {
    
    private void OnMouseDown() {
        ScoreManager.Singleton.IncrementLocalPlayerScore();
        ScoreManager.Singleton.IncrementPlayerScoreServerRpc(NetworkManager.Singleton.LocalClientId);
        DestroyMeServerRPC();
    }

    [ServerRpc(RequireOwnership = false)]
    void DestroyMeServerRPC(ServerRpcParams serverRpcParams = default) {
        var clientId = serverRpcParams.Receive.SenderClientId;
        if (GetComponent<NetworkObject>().IsSpawned) {
            GetComponent<NetworkObject>().Despawn();            
        }
    }
}
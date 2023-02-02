using Unity.Netcode.Components;

public class PlayerAuthoritativeNetworkTransform : NetworkTransform {
    protected override bool OnIsServerAuthoritative() {
        return false;
    }
}
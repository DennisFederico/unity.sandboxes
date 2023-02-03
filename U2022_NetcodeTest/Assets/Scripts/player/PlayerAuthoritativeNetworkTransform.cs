using Unity.Netcode.Components;

namespace player {
    public class PlayerAuthoritativeNetworkTransform : NetworkTransform {
        protected override bool OnIsServerAuthoritative() {
            return false;
        }
    }
}
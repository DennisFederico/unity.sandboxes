using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class Spawner : NetworkBehaviour {
    public static Spawner Instance { get; private set; }
    [SerializeField] List<Transform> prefabs;

    private void Awake() {
        if (Instance != null) {
            Debug.LogError($"There's more than one Spawner in the scene! {transform} - {Instance}");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnRandomObjectServerRpc() {
        // Do things for this client
        var prefab = prefabs[Random.Range(0, prefabs.Count)];
        var spawn = Instantiate(prefab, RandomPosition.GetRandomPosition(), Random.rotation);
        spawn.GetComponent<NetworkObject>().Spawn();
    }
}
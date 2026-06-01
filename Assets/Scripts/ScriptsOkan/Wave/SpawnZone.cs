using UnityEngine;

/// <summary>
///     Defines a spawn area on the map.
///     Zombies are placed on the ground using a downward raycast.
/// </summary>
public class SpawnZone : MonoBehaviour {
    [Header("Spawn Settings")] public GameObject zombiePrefab;

    public int zombieCount = 5;
    public float spawnRadius = 5f;

    [Header("Ground Detection")] [SerializeField]
    private LayerMask groundLayer;

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    public void SpawnWave() {
        for (var i = 0; i < zombieCount; i++) {
            var randomOffset = Random.insideUnitCircle * spawnRadius;
            var origin = new Vector3(
                transform.position.x + randomOffset.x,
                transform.position.y + 10f,
                transform.position.z + randomOffset.y
            );

            if (Physics.Raycast(origin, Vector3.down, out var hit, 50f, groundLayer))
                Instantiate(zombiePrefab, hit.point, Quaternion.identity);
        }
    }
}
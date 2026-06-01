using UnityEngine;

/// <summary>
///     Defines a circular spawn area on the map.
///     Zombies are instantiated at random positions within the radius
///     and initialized with the zone's home position and patrol bounds.
/// </summary>
public class SpawnZone : MonoBehaviour {
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Settings")] public GameObject zombiePrefab;

    /// <summary>Number of zombies spawned per wave.</summary>
    public int zombieCount = 5;

    /// <summary>Radius of the spawn and patrol area.</summary>
    public float spawnRadius = 5f;

    [Header("Ground Detection")] [SerializeField]
    private LayerMask groundLayer;

    /// <summary>
    ///     Draws a wire sphere in the Scene View to visualize the spawn area.
    ///     Only visible when the GameObject is selected.
    /// </summary>
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    /// <summary>
    ///     Spawns a wave of zombies at random positions within the spawn radius.
    ///     Each zombie is initialized with this zone's center and radius for patrol behaviour.
    /// </summary>
    public void SpawnWave() {
        for (var i = 0; i < zombieCount; i++) {
            var randomOffset = Random.insideUnitCircle * spawnRadius;
            var spawnPos = new Vector3(
                transform.position.x + randomOffset.x,
                transform.position.y,
                transform.position.z + randomOffset.y
            );

            var go = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
            var ai = go.GetComponent<ZombieAI>();
            //if (ai != null) ai.Init(transform.position, spawnRadius, playerTransform);
            if (ai != null) ai.Init(transform.position, spawnRadius * 2f, playerTransform);
        }
    }
}
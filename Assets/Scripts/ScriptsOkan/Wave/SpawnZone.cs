using UnityEngine;

/// <summary>
///     Defines a circular spawn area on the map.
///     Zombies are instantiated at random positions within the radius
///     and initialized with the zone's home position and patrol bounds.
/// </summary>
public class SpawnZone : MonoBehaviour {
    [SerializeField] private Transform playerTransform;

    [Header("Spawn Settings")] public GameObject zombiePrefab;

    public GameObject sprinterPrefab;

    /// <summary>Number of zombies spawned per wave.</summary>
    public int zombieCount = 5;

    /// <summary>Radius of the spawn and patrol area.</summary>
    public float spawnRadius = 5f;

    [HideInInspector] public int sprinterCount;

    [HideInInspector] public int currentDay;


    [Header("Ground Detection")]
    [SerializeField]
    private LayerMask groundLayer;

    /// <summary>
    ///     Draws a wire sphere in the Scene View to visualize the spawn area.
    ///     Only visible when the GameObject is selected.
    /// </summary>
    /* private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    } */

    /// <summary>
    ///     Spawns a wave of zombies at random positions within the spawn radius.
    ///     Each zombie is initialized with this zone's center and radius for patrol behaviour.
    /// </summary>
    public void SpawnWave() {
        // Normale Zombies
        for (var i = 0; i < zombieCount; i++) SpawnZombieAt(zombiePrefab);
        // Sprinter
        for (var i = 0; i < sprinterCount; i++) SpawnZombieAt(sprinterPrefab);
    }

    private void SpawnZombieAt(GameObject prefab) {
        var randomOffset = Random.insideUnitCircle * spawnRadius;
        var spawnPos = new Vector3(
            transform.position.x + randomOffset.x,
            transform.position.y,
            transform.position.z + randomOffset.y
        );
        var go = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        var ai = go.GetComponent<ZombieAI>();
        if (ai != null) ai.Init(transform.position, spawnRadius * 2f, playerTransform);
    }

    public void ClearZombies() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Destroy(transform.GetChild(i));
        }
    }
}
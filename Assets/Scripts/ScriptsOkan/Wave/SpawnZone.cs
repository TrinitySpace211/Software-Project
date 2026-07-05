using System;
using System.Collections.Generic;
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

    // Tank-Zombie-Prefab. Im Inspector zuweisen, sobald das Prefab existiert.
    public GameObject tankPrefab;

    /// <summary>Number of zombies spawned per wave.</summary>
    public int zombieCount = 5;

    /// <summary>Radius of the spawn and patrol area.</summary>
    public float spawnRadius = 5f;

    [HideInInspector] public int sprinterCount;

    [HideInInspector] public int tankCount;

    [HideInInspector] public int currentDay;


    [Header("Ground Detection")]
    [SerializeField]
    private LayerMask groundLayer;

    private List<ZombieAI> zombies = new();
    private List<SprinterController> sprinters = new();
    private List<TankZombieController> tanks = new();

    /// <summary>
    ///     Draws a wire sphere in the Scene View to visualize the spawn area.
    ///     Only visible when the GameObject is selected.
    /// </summary>
    /* private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    } */

    private void Tank_OnDead(TankZombieController tank) {
        if (tank != null) {
            tanks.Remove(tank);
        }
    }

    private void Sprinter_OnDead(SprinterController sprinter) {
        if (sprinter != null) {
            sprinters.Remove(sprinter);
        }
    }

    private void Zombie_OnDead(ZombieAI zombie) {
        if (zombie != null) {
            zombies.Remove(zombie);
        }
    }

    /// <summary>
    ///     Spawns a wave of zombies at random positions within the spawn radius.
    ///     Each zombie is initialized with this zone's center and radius for patrol behaviour.
    /// </summary>
    public void SpawnWave() {
        // Normale Zombies
        for (var i = 0; i < zombieCount; i++) {
            ZombieAI zombieInstance = SpawnZombieAt<ZombieAI>(zombiePrefab);
            zombies.Add(zombieInstance);
        }

        // Sprinter
        for (var i = 0; i < sprinterCount; i++) {
            SprinterController sprinter = SpawnZombieAt<SprinterController>(sprinterPrefab);
            sprinters.Add(sprinter);
        }

        // Tanks
        for (var i = 0; i < tankCount; i++) {
            TankZombieController tank = SpawnZombieAt<TankZombieController>(tankPrefab);
            tanks.Add(tank);
        }

    }

    private T SpawnZombieAt<T>(GameObject prefab) {
        if (prefab == null) return default;

        var randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;
        var spawnPos = new Vector3(
            transform.position.x + randomOffset.x,
            transform.position.y,
            transform.position.z + randomOffset.y
        );
        var go = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        var ai = go.GetComponent<ZombieAI>();
        if (ai != null) ai.Init(transform.position, spawnRadius * 2f, playerTransform);

        return go.GetComponent<T>();
    }

    public void ClearZombies() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            Destroy(transform.GetChild(i));
        }
    }

    public List<ZombieAI> GetZombies() {
        return zombies;
    }

    public List<SprinterController> GetSprinters() {
        return sprinters;
    }

    public List<TankZombieController> GetTanks() {
        return tanks;
    }

    private void OnEnable() {
        ZombieAI.OnDead += Zombie_OnDead;
        SprinterController.OnDead += Sprinter_OnDead;
        TankZombieController.OnDead += Tank_OnDead;
    }

    private void OnDisable() {
        ZombieAI.OnDead -= Zombie_OnDead;
        SprinterController.OnDead -= Sprinter_OnDead;
        TankZombieController.OnDead -= Tank_OnDead;
    }
}
using UnityEngine;

/// <summary>
/// Continuously searches for the nearest enemy within a defined radius.
/// </summary>
public class FindEnemy : MonoBehaviour {

    /// <summary>
    /// Stores the found enemy. Other scripts, such as LookAtEnemy.cs and Emmitter.cs, use this value.
    /// </summary>
    public ZombieAI zombie;

    public SprinterController sprinter;

    public TankZombieController tank;

    /// <summary>
    /// The search radius of the tower.
    /// </summary>
    public float radius = 10f;

    /// <summary>
    /// To define how often a enemy will be searched, in this case in a period of 0.25 seconds.
    /// </summary>
    public float searchInterval = 0.25f;

    /// <summary>
    /// The defined Enemy Tag to find the enemies as GameObjects.
    /// </summary>
    public string enemyTag = "Enemy";


    // We need function to see which GameObjects in our radius and wich Object has the Tag "Enemy".
    // It could contains a big list of different GameObjects so that it is not recommendet to use a findEnemy function in Update(),
    // because this has perfomance reasons.
    private void OnEnable() { // If GamObject with this script activated, this method will be called

        // Calls the method FindNewEnemy() in a period of 0.25 seconds
        InvokeRepeating(nameof(FindNewEnemy), 0f, searchInterval);
    }

    // If GameObject with this script deactivated, this method will be called
    private void OnDisable() {

        // If the tower deactivated, FindNewEnemy() method should not be called anymore
        CancelInvoke(nameof(FindNewEnemy));
    }

    /// <summary>
    /// Searches for the nearest active enemy within the defined radius.
    /// </summary>
    private void FindNewEnemy() {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits) {
            ZombieAI zombie = hit.gameObject.GetComponentInParent<ZombieAI>();
            SprinterController sprinter = hit.gameObject.GetComponentInParent<SprinterController>();
            TankZombieController tank = hit.gameObject.GetComponentInParent<TankZombieController>();

            if (zombie != null) {
                if (zombie.IsDead()) {
                    continue;
                }
                if (IsBlockedByObject(zombie)) {
                    continue;
                }
                this.zombie = zombie; // global variable becomes the local variable
            } else if (sprinter != null) {
                if (sprinter.IsDead()) {
                    continue;
                }
                if (IsBlockedByObject(sprinter)) {
                    continue;
                }
                this.sprinter = sprinter;
            } else if (tank != null) {
                if (tank.IsDead()) {
                    continue;
                }
                if (IsBlockedByObject(tank)) {
                    continue;
                }
                this.tank = tank;
            }
        }
    }

    /// <summary>
    /// Checks if there is any collider between the tower and the target zombie.
    /// Works with any zombie type that implements IDamageable and is a Component.
    /// </summary>
    private bool IsBlockedByObject<T>(T targetZombie) where T : Component, IDamageable {
        if (targetZombie == null) {
            return false;
        }

        Vector3 startPosition = transform.position + Vector3.up * 1.0f;
        Vector3 targetPosition = targetZombie.transform.position + Vector3.up * 1.0f;

        Vector3 direction = targetPosition - startPosition;
        float distance = direction.magnitude;

        if (distance <= 0f) {
            return false;
        }

        RaycastHit[] hits = Physics.RaycastAll(
            startPosition,
            direction.normalized,
            distance
        );

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits) {
            // Ignore this tower itself
            if (hit.collider.transform.IsChildOf(transform)) {
                continue;
            }

            T hitZombie = hit.collider.GetComponentInParent<T>();

            if (hitZombie == null) {
                continue;
            }

            if (hitZombie == targetZombie) {
                return false;
            }

            if (hitZombie.IsDead()) {
                continue;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Gizmos = Lines or something like that are created from the developer. 
    /// This function show us the gizmo if we selected the object.
    /// </summary>
    /* private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    } */
}

/*
 * FindEnemy.cs sucht den nächsten Zombie (Dieses Skript auf crossbow parent)
        ↓
 * LookAtEnemy.cs dreht die Armbrust zum Zombie (Dieses Skript auf die Armbrust)
        ↓
 * Emmitter.cs erzeugt die Pfeile in Zeitintervallen (Dieses Skript auf Emmitter und bekommt Bullet Prefab zugewiesen im Inspector)
        ↓
 * Bullet.cs lässt Bullet-Prefab zum enemy fliegen und macht Schaden
 */
using UnityEngine;

/// <summary>
/// Continuously searches for the nearest enemy within a defined radius.
/// </summary>
public class FindEnemy : MonoBehaviour {

    /// <summary>
    /// Stores the found enemy. Other scripts, such as LookAtEnemy.cs and Emmitter.cs, use this value.
    /// </summary>
    public ZombieAI zombie;

    /// <summary>
    /// Stores the found enemy. Other scripts, such as LookAtEnemy.cs and Emmitter.cs, use this value.
    /// </summary>
    public SprinterController sprinter;

    /// <summary>
    /// Stores the found enemy. Other scripts, such as LookAtEnemy.cs and Emmitter.cs, use this value.
    /// </summary>
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
        // Get all colliders inside the tower's detection radius.
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        // Check every collider found inside the radius.
        foreach (Collider hit in hits) {
            // Try to find a normal zombie component on the hit object or one of its parents.
            ZombieAI zombie = hit.gameObject.GetComponentInParent<ZombieAI>();

            // Try to find a sprinter zombie component on the hit object or one of its parents.
            SprinterController sprinter = hit.gameObject.GetComponentInParent<SprinterController>();

            // Try to find a tank zombie component on the hit object or one of its parents.
            TankZombieController tank = hit.gameObject.GetComponentInParent<TankZombieController>();

            // Check if the detected enemy is a normal zombie.
            if (zombie != null) {
                // Ignore the zombie if it is already dead.
                if (zombie.IsDead()) {
                    continue;
                }

                // Ignore the zombie if another object blocks the line of sight.
                if (IsBlockedByObject(zombie)) {
                    continue;
                }

                // Store the found zombie as the current target.
                this.zombie = zombie; // global variable becomes the local variable
            } else if (sprinter != null) {
                // Ignore the sprinter if it is already dead.
                if (sprinter.IsDead()) {
                    continue;
                }

                // Ignore the sprinter if another object blocks the line of sight.
                if (IsBlockedByObject(sprinter)) {
                    continue;
                }

                // Store the found sprinter as the current target.
                this.sprinter = sprinter;
            } else if (tank != null) {
                // Ignore the tank zombie if it is already dead.
                if (tank.IsDead()) {
                    continue;
                }

                // Ignore the tank zombie if another object blocks the line of sight.
                if (IsBlockedByObject(tank)) {
                    continue;
                }

                // Store the found tank zombie as the current target.
                this.tank = tank;
            }
        }
    }

    /// <summary>
    /// Checks if there is any collider between the tower and the target zombie.
    /// Works with any zombie type that implements IDamageable and is a Component.
    /// </summary>
    private bool IsBlockedByObject<T>(T targetZombie) where T : Component, IDamageable {
        // If there is no target zombie, nothing can block the view.
        if (targetZombie == null) {
            return false;
        }

        // Start the ray slightly above the tower position to avoid shooting from ground level.
        Vector3 startPosition = transform.position + Vector3.up * 1.0f;

        // Aim at a point slightly above the zombie position, roughly at body height.
        Vector3 targetPosition = targetZombie.transform.position + Vector3.up * 1.0f;

        // Calculate the direction from the tower to the target zombie.
        Vector3 direction = targetPosition - startPosition;

        // Calculate the distance between the tower and the target zombie.
        float distance = direction.magnitude;

        // If the distance is zero or invalid, no raycast should be performed.
        if (distance <= 0f) {
            return false;
        }

        // Cast a ray from the tower to the target zombie and collect all hits along the way.
        RaycastHit[] hits = Physics.RaycastAll(
            startPosition,
            direction.normalized,
            distance
        );

        // Sort all hits by distance so the closest objects are checked first.
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        // Check every object hit by the raycast.
        foreach (RaycastHit hit in hits) {
            // Ignore this tower itself
            if (hit.collider.transform.IsChildOf(transform)) {
                continue;
            }

            // Try to find a zombie component on the hit object or one of its parents.
            T hitZombie = hit.collider.GetComponentInParent<T>();

            // If the hit object is not a zombie of the expected type, ignore it.
            if (hitZombie == null) {
                continue;
            }

            // If the first relevant zombie hit is the actual target, the view is not blocked.
            if (hitZombie == targetZombie) {
                return false;
            }

            // Dead zombies should not block the tower's line of sight.
            if (hitZombie.IsDead()) {
                continue;
            }

            // A different living zombie was found before the target, so the target is blocked.
            return true;
        }

        // No living zombie was found between the tower and the target.
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
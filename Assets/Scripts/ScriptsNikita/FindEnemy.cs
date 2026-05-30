using UnityEngine;

public class FindEnemy : MonoBehaviour { // This script searches continously the next zomnie in a defined radius

    // Variable to store the foundet enemy, other scripts (LookAtEnemy.cs Emmitter.cs) use this value
    public GameObject enemy; 

    // The radius of the tower
    public float radius = 10f;

    // To define how often a enemy will be serched, in this case in a period of 0.25 seconds
    public float searchInterval = 0.25f;

    // The defined Enemy Tag to find the enemies as GameObjects
    public string enemyTag = "Enemy"; 


    // We need function to see which GameObjects in our radius and wich Object has the Tag "Enemy".
    // It could contains a big list of different GameObjects so that it is not recommendet to use a findEnemy function in Update(),
    // because this has perfomance reasons.
    private void OnEnable() { // If GamObject with this script activated, this method will be called

        // Calls the method FindNewEnemy() in a period of 0.25 seconds
        InvokeRepeating(nameof(FindNewEnemy), 0f, searchInterval); 
    }

    private void OnDisable() { // If GameObject with this script deactivated, this method will be called

        // If the tower deactivated, FindNewEnemy() method should not be called anymore
        CancelInvoke(nameof(FindNewEnemy));
    }

    private void FindNewEnemy() {

        // Variable to store GameObejcts with the "Enemy" Tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag); 

        GameObject nearestEnemy = null; // To store the nearest Enemy to the tower

        // The biggest allowd distance between enemy and tower.
        // Later we use sqrMagnitude and this is square of the distance and performs better than Vector3.Distance
        float nearestDistance = radius * radius;

        foreach (GameObject possibleEnemy in enemies) {
            if (!possibleEnemy.activeInHierarchy)
                continue; // Deactivated Enemies will be ignored

            // Calculate the distance between enemy and tower
            // sqrMagnitude -> The square of the magnitude of this vector
            float distance = (possibleEnemy.transform.position - transform.position).sqrMagnitude; 

            // Stores the new nearest Enemy
            if (distance < nearestDistance) {
                nearestDistance = distance;
                nearestEnemy = possibleEnemy;
            }
        }

        enemy = nearestEnemy;
    }

    /// <summary>
    /// Gizmos = Lines or something like that are created from the developer. This function show us the gizmo if we selected the object
    /// </summary>
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

/*
 * FindEnemy sucht den nächsten Zombie
        ↓
 * LookAtEnemy dreht die Armbrust zum Zombie
        ↓
 * Emmitter schießt in Intervallen ein Bullet
        ↓
 * Bullet fliegt zum Zombie
        ↓
 * Bullet trifft und ruft TakeDamage auf
 */

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

        foreach(Collider hit in hits) {
            ZombieAI zombie = hit.gameObject.GetComponentInParent<ZombieAI>();
            if (zombie != null) {
                if(zombie.IsDead()) {
                    return;
                }
                this.zombie = zombie; // global variable becomes the local variable
            }
        }
    }

    /// <summary>
    /// Gizmos = Lines or something like that are created from the developer. 
    /// This function show us the gizmo if we selected the object.
    /// </summary>
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
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
using UnityEngine;

/// <summary>
/// Creates and shoots arrows at regular intervals.
/// </summary>
public class Emmitter : MonoBehaviour 
{
    /// <summary>
    /// The bullet-Prefab which should be spawn.
    /// </summary>
    public GameObject bullet;

    /// <summary>
    /// The Time between 2 arrows.
    /// </summary>
    public float timeInterval = 0.4f;

    /// <summary>
    /// Timer for the countdown until next shot.
    /// </summary>
    private float currentTime;

    /// <summary>
    /// // Reference to the FindEnemy script that stores the currently found enemy.
    /// </summary>
    public FindEnemy findEnemy;

    /// <summary>
    /// Initializes the shooting timer and gets the FindEnemy component from the parent GameObject.
    /// </summary>
    private void Start()
    {
        // To set the Timer from the inspector (timeInterval is global and public)
        currentTime = timeInterval;

        // Searches in the parent GameObejct the FindEnemy component
        findEnemy = GetComponentInParent<FindEnemy>(); 
    }

    /// <summary>
    /// Counts down the timer and shoots at the current enemy when the timer reaches zero.
    /// </summary>
    private void Update()
    {
        if (findEnemy == null || findEnemy.zombie == null)

            // return if enemies not found, the tower doesnt shot in this case
            return;

        // Timer counts down
        currentTime -= Time.deltaTime;

        // If timer reaches 0, the shoot() method can be called
        if (currentTime <= 0f && !findEnemy.zombie.IsDead()) {
            Shoot(findEnemy.zombie);
            currentTime = timeInterval; // to reset the timer
        }
    }

    /// <summary>
    /// Spawns a bullet (arrow) and assigns the given target to it.
    /// </summary>
    private void Shoot(ZombieAI target) {

        // Information for the developer
        if (bullet == null) {
            Debug.LogError("Bullet Prefab fehlt beim Emmitter!");
            return;
        }

        // Creates a copy of bullet (the arrow)
        GameObject bulletInstance = Instantiate(
            bullet, // Bullet from the inspector to copy
            transform.position,
            transform.rotation
        );

        // Look for the Bullet script on the root object of the newly spawned bullet.
        Bullet bulletScript = bulletInstance.GetComponent<Bullet>();

        // If no Bullet Script was found in the root directory, then Unity will also search the child objects.
        if (bulletScript == null) {
            bulletScript = bulletInstance.GetComponentInChildren<Bullet>();
        }

        if (bulletScript != null) {

            // Give the enemy to the bullet script
            bulletScript.target = target;
        }

        ExplosiveBullet explosiveBulletScript = bulletInstance.GetComponent<ExplosiveBullet>();

        if (explosiveBulletScript == null) {
            explosiveBulletScript = bulletInstance.GetComponentInChildren<ExplosiveBullet>();
        }

        if (explosiveBulletScript != null) {
            explosiveBulletScript.target = target;
        }
    }
}

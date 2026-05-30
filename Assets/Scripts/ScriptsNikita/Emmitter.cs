using UnityEngine;

public class Emmitter : MonoBehaviour // Script to creates the arrows at regular intervals
{
    // The bullet-Prefab which should be spawn
    public GameObject bullet;

    // The Time between 2 arrows
    public float timeInterval = 0.4f; 
  
    private float currentTime; // Timer for the countdown until next shot
    public FindEnemy enemy; // Variable to have a reference to the FindEnemy script to story the found enemy

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // To set the Timer from the inspector (timeInterval is global and public)
        currentTime = timeInterval;

        // Searches in the parent GameObejct the FindEnemy component
        enemy = GetComponentInParent<FindEnemy>(); 
    }

    // Update is called once per frame
    private void Update()
    {
        if (enemy == null || enemy.enemy == null)

            // return if enemies not found, the tower doesnt shot in this case
            return;

        // Timer counts down
        currentTime -= Time.deltaTime;

        // If timer reaches 0, the shoot() method can be called
        if (currentTime <= 0f) {
            Shoot(enemy.enemy);
            currentTime = timeInterval; // to reset the timer
        }
    }

    private void Shoot(GameObject target) {

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
    }
}

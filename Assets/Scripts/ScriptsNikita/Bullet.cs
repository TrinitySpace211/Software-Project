using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class Bullet : MonoBehaviour // Script to move the arrow and to make damage to the enemies
{
    // The founded enemy which cames from Emmitter script
    public GameObject target; 

    // Speed of the arrow    
    public float speed = 5f;

    // To determine the distance at which an arrow is considered to have hit the target
    // If the arrow is within 0.25 units of the enemy, HitTarget() is executed
    public float hitDistance = 0.25f; 

    // Damage of the arrow
    public int damage = 10;

    // Additional rotation for the arrow
    public Vector3 rotationOffset;


    // Update is called once per frame
    private void Update()
    {
        // If enemy not exists, the arrow should be destroyed
        if(target == null) {
            Destroy(this.gameObject);
            return;
        }

        // Calculates the target position (enemy)
        Vector3 targetPosition = target.transform.position + Vector3.up * 1f;

        // Calculates the distance from enemy and the arrow
        Vector3 direction = targetPosition - transform.position;

        // Calculates how far the arrow should move in this frame.
        float moveDistance = speed * Time.deltaTime;

        // Check if the arrow is near enough to the enemy
        // And to avoid that arrows can go through out enemies
        if (direction.magnitude <= hitDistance || direction.magnitude <= moveDistance) {
            HitTarget(); // Called if a arrow hit can be analyzed
            return;
        }

        // Moves the arrow to the enemy
        // direction.normalized = Lenght of 1
        transform.position += direction.normalized * moveDistance;

        // Calculates a rotation in which the forward axis of the arrow points in the direction of flight.
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);

        // Rotate the arrow to point in the direction of flight
        transform.rotation = lookRotation * Quaternion.Euler(rotationOffset);
    }

    private void HitTarget() {

        // Calls a method named TakeDamage on the target.
        // SendMessageOptions.DontRequireReceiver = If the enemy doest have a TakeDamage method, no error occurs.
        target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        // Destroys the arrow after a hit
        Destroy(gameObject);
    }
}

using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

/// <summary>
/// Moves the arrow towards its target and applies damage when it hits an enemy.
/// </summary>
public class Bullet : MonoBehaviour {
    /// <summary>
    /// Stores the transform of the current target.
    /// </summary>
    private Transform targetTransform;

    /// <summary>
    /// Stores the damageable interface of the current target.
    /// </summary>
    private IDamageable targetDamageable;

    /// <summary>
    /// Speed of the arrow.   
    /// </summary>
    public float speed = 5f;

    /// <summary>
    /// To determine the distance at which an arrow is considered to have hit the target.
    /// If the arrow is within 0.25 units of the enemy, HitTarget() is executed.
    /// </summary>
    public float hitDistance = 0.25f;

    /// <summary>
    /// Damage of the arrow.
    /// </summary>
    public int damage = 10;

    /// <summary>
    /// Additional rotation offset for the arrow.
    /// </summary>
    public Vector3 rotationOffset;


    /// <summary>
    /// Sets the current target for this object.
    /// </summary>
    /// <typeparam name="T">
    /// The target type, which must be a Component and implement IDamageable.
    /// </typeparam>
    /// <param name="target">
    /// The target component that should be tracked and damaged.
    /// </param>
    public void SetTarget<T>(T target) where T : Component, IDamageable {
        targetTransform = target.transform;
        targetDamageable = target;
    }

    /// <summary>
    /// Moves the arrow towards its target every frame and checks if it has hit.
    /// </summary>
    private void Update() {
        if (targetTransform == null || targetDamageable == null || targetDamageable.IsDead()) {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = targetTransform.position + Vector3.up * 1f;

        // Calculates the distance from enemy and the arrow
        Vector3 direction = targetPosition - transform.position;

        // Calculates how far the arrow should move in this frame.
        float moveDistance = speed * Time.deltaTime;

        // Check if the arrow is close enough to the enemy.
        // And to avoid that arrows can go through out enemies
        if (direction.magnitude <= hitDistance || direction.magnitude <= moveDistance) {
            HitTarget();
            return;
        }

        // Move the arrow towards the enemy
        // direction.normalized has a length of 1
        transform.position += direction.normalized * moveDistance;

        // Calculates a rotation in which the forward axis of the arrow points in the direction of flight direction.
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);

        // Rotate the arrow to point in the flight direction
        transform.rotation = lookRotation * Quaternion.Euler(rotationOffset);
    }

    /// <summary>
    /// Applies damage to the target and destroys the arrow after hitting.
    /// </summary>
    private void HitTarget() {
        if (targetDamageable != null && !targetDamageable.IsDead()) {
            targetDamageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}

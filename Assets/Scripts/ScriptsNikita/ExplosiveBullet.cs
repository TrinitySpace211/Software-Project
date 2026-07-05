using UnityEngine;

/// <summary>
/// Moves an explosive projectile towards a target and deals area damage on impact.
/// This script is used for upgraded tower projectiles, such as cannon balls or rockets.
/// </summary>
public class ExplosiveBullet : MonoBehaviour {
    private Transform targetTransform;
    private IDamageable targetDamageable;

    /// <summary>
    /// Movement speed of the projectile.
    /// </summary>
    public float speed = 10f;

    /// <summary>
    /// Distance at which the projectile counts as hitting the target.
    /// </summary>
    public float hitDistance = 0.35f;

    /// <summary>
    /// Damage dealt to the directly hit target.
    /// </summary>
    public int directDamage = 40;

    /// <summary>
    /// Damage dealt to all other enemies inside the explosion radius.
    /// </summary>
    public int areaDamage = 20;

    /// <summary>
    /// Radius of the explosion area.
    /// </summary>
    public float explosionRadius = 3f;

    /// <summary>
    /// Optional rotation offset for the projectile model.
    /// Use this if the projectile visually points in the wrong direction.
    /// </summary>
    public Vector3 rotationOffset;

    /// <summary>
    /// Layer mask for enemies that can receive explosion damage.
    /// </summary>
    public LayerMask enemyLayerMask;

    /// <summary>
    /// Visual effect that is spawned when the projectile explodes.
    /// </summary>
    public ParticleSystem explosionEffect;

    /// <summary>
    /// Sets the target for this explosive projectile.
    /// The target must be a Unity Component and implement the IDamageable interface.
    /// </summary>
    /// <typeparam name="T">
    /// The target type, which must inherit from Component and implement IDamageable.
    /// </typeparam>
    /// <param name="target">
    /// The target that the projectile should follow and damage.
    /// </param>
    public void SetTarget<T>(T target) where T : Component, IDamageable {
        // Store the target transform so the projectile can follow its position.
        targetTransform = target.transform;

        // Store the IDamageable reference so damage can be applied later.
        targetDamageable = target;
    }

    /// <summary>
    /// Moves the projectile towards the target every frame.
    /// If the projectile reaches the target, it explodes.
    /// </summary>
    private void Update() {
        // Destroy the projectile if the target is missing or already dead.
        if (targetTransform == null || targetDamageable == null || targetDamageable.IsDead()) {
            Destroy(gameObject);
            return;
        }

        // Aim slightly above the target position to hit the body instead of the ground.
        Vector3 targetPosition = targetTransform.position + Vector3.up * 1f;

        // Calculate the direction from the projectile to the target.
        Vector3 direction = targetPosition - transform.position;

        // Calculate how far the projectile can move during this frame.
        float moveDistance = speed * Time.deltaTime;

        // If the projectile is close enough to the target, trigger the explosion.
        if (direction.magnitude <= hitDistance || direction.magnitude <= moveDistance) {
            Explode();
            return;
        }

        // Move the projectile towards the target.
        transform.position += direction.normalized * moveDistance;

        // Rotate the projectile so it visually points towards the target.
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);

        // Apply the rotation together with the optional model rotation offset.
        transform.rotation = lookRotation * Quaternion.Euler(rotationOffset);
    }

    /// <summary>
    /// Deals direct damage to the main target and area damage to nearby enemies.
    /// </summary>
    private void Explode() {

        // Spawn the explosion visual effect if one has been assigned.
        if (explosionEffect != null) {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Apply direct damage to the main target if it is still alive.
        if (targetDamageable != null && !targetDamageable.IsDead()) {
            targetDamageable.TakeDamage(directDamage);
        }

        // Find all colliders inside the explosion radius that are on the enemy layer.
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayerMask);

        // Check every collider found inside the explosion radius.
        foreach (Collider hit in hits) {
            // Try to get a component from the hit object or one of its parents.
            var nearbyComponent = hit.GetComponentInParent<Component>();

            // Ignore objects that do not implement the IDamageable interface.
            if (nearbyComponent is not IDamageable nearbyDamageable)
                continue;

            // Ignore enemies that are already dead.
            if (nearbyDamageable.IsDead())
                continue;

            // Ignore the main target because it already received direct damage.
            if (nearbyDamageable == targetDamageable)
                continue;

            // Apply area damage to nearby enemies.
            nearbyDamageable.TakeDamage(areaDamage);
        }

        // Destroy the projectile after the explosion is finished.
        Destroy(gameObject);
    }

    /* private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    } */
}
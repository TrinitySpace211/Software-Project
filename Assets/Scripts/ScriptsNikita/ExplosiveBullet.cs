using UnityEngine;

/// <summary>
/// Moves an explosive projectile towards a target and deals area damage on impact.
/// This script is used for upgraded tower projectiles, such as cannon balls or rockets.
/// </summary>
public class ExplosiveBullet : MonoBehaviour {
    /// <summary>
    /// The current target of the projectile.
    /// This value is assigned by the Emmitter script.
    /// </summary>
    public ZombieAI target;

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

    private void Update() {
        if (target == null) {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition = target.transform.position + Vector3.up * 1f;
        Vector3 direction = targetPosition - transform.position;

        float moveDistance = speed * Time.deltaTime;

        if (direction.magnitude <= hitDistance || direction.magnitude <= moveDistance) {
            Explode();
            return;
        }

        transform.position += direction.normalized * moveDistance;

        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = lookRotation * Quaternion.Euler(rotationOffset);
    }

    /// <summary>
    /// Deals direct damage to the main target and area damage to nearby enemies.
    /// </summary>
    private void Explode() {

        if (explosionEffect != null) {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        ZombieAI directTargetZombie = null;

        if (target != null) {
            directTargetZombie = target.GetComponentInParent<ZombieAI>();

            if (directTargetZombie != null && !directTargetZombie.IsDead()) {
                directTargetZombie.TakeDamage(directDamage);
            }
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayerMask);

        foreach (Collider hit in hits) {
            ZombieAI nearbyZombie = hit.GetComponentInParent<ZombieAI>();

            if (nearbyZombie == null)
                continue;

            if (nearbyZombie.IsDead())
                continue;

            if (nearbyZombie == directTargetZombie)
                continue;

            nearbyZombie.TakeDamage(areaDamage);
        }

        Destroy(gameObject);
    }

    /* private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    } */
}
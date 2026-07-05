using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Grenade class with a rigidbody for the throw
/// </summary>
public class Grenade : MonoBehaviour {

    [SerializeField] private WeaponSlot weaponSlot;
    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private ParticleSystem explosionParticle;
    [SerializeField] private int damage = 80;
    [SerializeField] private float explosionRadius = 10f;
    private Rigidbody rb;
    private bool isThrown = false;
    private bool exploded = false;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
    }

    private void Update() {
        GrenadeTickToExplosion();
    }

    /// <summary>
    /// Spawns the Grenade on the Parent which is most likely the Players hand
    /// </summary>
    /// <param name="parent">The Transform to where it should be spawned at</param>
    /// <returns>the spawned instance</returns>
    public Grenade Spawn(Transform parent) {
        Grenade grenadeInstance = Instantiate(this, parent, false);
        grenadeInstance.transform.localPosition = Vector3.zero;
        grenadeInstance.transform.localRotation = Quaternion.identity;
        grenadeInstance.transform.localScale = Vector3.one * 12f;
        grenadeInstance.gameObject.SetActive(true);

        Rigidbody grenadeRb = grenadeInstance.GetComponent<Rigidbody>();
        if (grenadeRb != null) {
            grenadeRb.linearVelocity = Vector3.zero;
            grenadeRb.useGravity = false;
            grenadeRb.isKinematic = true;
        }

        return grenadeInstance;
    }

    /// <summary>
    /// Sets the Parent null, sets the rigidbody so that grenade gets thrown into a certain Direction
    /// </summary>
    /// <param name="direction">The Direction and Force</param>
    public void Throw(Vector3 direction) {
        transform.SetParent(null);

        if (rb == null) {
            rb = GetComponent<Rigidbody>();
        }

        if (rb != null) {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = direction;
            isThrown = true;
        }
    }

    /// <summary>
    /// When the Grenade is thrown if will explode with an Paricle Effect 
    /// after the set explosion Delay and then destroyed
    /// </summary>
    private void GrenadeTickToExplosion() {
        if (isThrown && !exploded) {
            explosionDelay -= Time.deltaTime;
            if (explosionDelay <= 0) {
                exploded = true;

                Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

                HashSet<IDamageable> zombies = new();
                foreach (Collider hit in hits) {
                    IDamageable zombie = hit.GetComponentInParent<IDamageable>();

                    if (zombie != null && !zombie.IsDead() && zombies.Add(zombie)) {
                        zombie.TakeDamage(damage);
                    }
                }

                SoundManager.Instance.Grenade_ExplosionSound(transform.position);
                Instantiate(explosionParticle, transform.position, Quaternion.identity);

                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Getter for the Weapon Slot it is currently set
    /// </summary>
    /// <returns>The Weapon Slot</returns>
    public WeaponSlot GetWeaponSlot() {
        return weaponSlot;
    }

    /// <summary>
    /// Getter for if the Grenade was thrown
    /// </summary>
    /// <returns>true if the grenades velocity got set false otherwise</returns>
    public bool GetIsThrown() {
        return isThrown;
    }

    /* private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, explosionRadius);

        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    } */
}

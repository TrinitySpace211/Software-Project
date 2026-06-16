using System.Collections.Generic;
using UnityEngine;

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
        }

        return grenadeInstance;
    }

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

    private void GrenadeTickToExplosion() {
        if (isThrown && !exploded) {
            explosionDelay -= Time.deltaTime;
            if (explosionDelay <= 0) {
                exploded = true;

                Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

                HashSet<ZombieAI> zombies = new();
                foreach (Collider hit in hits) {
                    ZombieAI zombie = hit.GetComponentInParent<ZombieAI>();

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

    public WeaponSlot GetWeaponSlot() {
        return weaponSlot;
    }

    public bool GetIsThrown() {
        return isThrown;
    }

    /* private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, explosionRadius);

        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    } */
}

using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunSO : ScriptableObject {

    public ImpactType impactType;
    public GunType type;
    public WeaponSlot weaponSlot;
    public string gunName;
    public GameObject modelPrefab;
    public Vector3 spawnPoint;
    public Vector3 spawnRotation;

    public ShootConfigSO shootConfigSO;
    public TrailConfigSO trailConfigSO;

    private MonoBehaviour activeMonoBehaviour;
    private GameObject model;
    private float lastShootTime;
    private ParticleSystem shootSystem;
    private ObjectPool<TrailRenderer> trailPool;
    private GameObject poolParent;

    public void SpawnModel(Transform parent, MonoBehaviour activeMonoBehaviour) {
        if (model == null) {
            this.activeMonoBehaviour = activeMonoBehaviour;
            lastShootTime = 0f;
            trailPool = new ObjectPool<TrailRenderer>(CreateTrail);
            poolParent = new GameObject("PoolParent");
            model = Instantiate(modelPrefab);
            model.transform.SetParent(parent, false);
            model.transform.localPosition = spawnPoint;
            model.transform.localRotation = Quaternion.Euler(spawnRotation);
        }

        shootSystem = model.GetComponentInChildren<ParticleSystem>();
    }

    public void Shoot() {
        if (Time.time > shootConfigSO.fireRate + lastShootTime) {
            lastShootTime = Time.time;

            shootSystem.Play();
            ShootByWeaponType(shootSystem.transform.position);

            for (int i = 0; i < shootConfigSO.bulletsPerShoot; i++) {
                Vector3 shootDirection = shootSystem.transform.forward
                    + new Vector3(
                            Random.Range(-shootConfigSO.spread.x, shootConfigSO.spread.x),
                            Random.Range(-shootConfigSO.spread.y, shootConfigSO.spread.y),
                            Random.Range(-shootConfigSO.spread.z, shootConfigSO.spread.z)
                        );
                shootDirection.Normalize();

                if (Physics.Raycast(shootSystem.transform.position, shootDirection, out RaycastHit hit, 100f, shootConfigSO.hitMask)) {
                    activeMonoBehaviour.StartCoroutine(PlayTrail(shootSystem.transform.position, hit.point, hit));
                } else {
                    activeMonoBehaviour.StartCoroutine(PlayTrail(shootSystem.transform.position, shootSystem.transform.position + (shootDirection * trailConfigSO.missDistance), new RaycastHit()));
                }
            }

        }
    }

    /// <summary>
    /// Plays the path of the "bullets" and if it hits and object it plays the corresponding texture impact particle effect
    /// </summary>
    /// <param name="startPoint">The starting position of the "bullets"</param>
    /// <param name="endPoint">The end position of the "bullets"</param>
    /// <param name="hit">The hit object</param>
    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit) {
        TrailRenderer instance = trailPool.Get();
        instance.transform.SetParent(poolParent.transform, false);
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;
        yield return null; // zum korrektem Verarbeiten der Position und Rendering-Initialisierung vor der Bewegung

        instance.emitting = true;

        float distance = Vector3.Distance(startPoint, endPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0f) {
            instance.transform.position = Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1 - (remainingDistance / distance)));
            remainingDistance -= trailConfigSO.simulationSpeed * Time.deltaTime;

            yield return null; //damit die Schleife nicht in einem Frame durchläuft, sondern pro Frame
        }

        instance.transform.position = endPoint;

        if (hit.collider != null) {
            SurfaceManager.Instance.HandleImpact(hit.transform.gameObject, endPoint, hit.normal, impactType, hit.triangleIndex);

            HitEnemy(hit);
        }

        yield return new WaitForSeconds(trailConfigSO.duration);
        yield return null; // der Trail soll noch für einen weiteren Frame sichtbar sein

        instance.emitting = false;
        instance.gameObject.SetActive(false);
        trailPool.Release(instance);
    }

    /// <summary>
    /// Creates the bullet path trail
    /// </summary>
    /// <returns>the rendered trail</returns>
    private TrailRenderer CreateTrail() {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = trailConfigSO.color;
        trail.material = trailConfigSO.material;
        trail.widthCurve = trailConfigSO.widthCurve;
        trail.time = trailConfigSO.duration;
        trail.minVertexDistance = trailConfigSO.minVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }

    public void Despawn() {
        // We do a bunch of other stuff on the same frame, so we really want it to be immediately destroyed, not at Unity's convenience.
        model.SetActive(false);

        shootSystem = null;
    }

    private void HitEnemy(RaycastHit hit) {
        ZombieAI zombie = hit.transform.GetComponentInParent<ZombieAI>();

        if (zombie != null) {
            if (!zombie.IsDead()) {
                zombie.TakeDamage(shootConfigSO.damage);
            }
        }
    }

    public void Spawn(Transform parent, MonoBehaviour activeMonoBehaviour) {
        if (model == null) {
            SpawnModel(parent, activeMonoBehaviour);
        } else {
            model.SetActive(true);

            shootSystem = model.GetComponentInChildren<ParticleSystem>();
        }
    }

    private void ShootByWeaponType(Vector3 position) {
        switch (type) {
            case GunType.AssaultRifle:
                WeaponSoundManager.Instance.AssaultRilfe_ShootSound(position, shootConfigSO.shootVolume);
                break;
            case GunType.Pistol:
                WeaponSoundManager.Instance.Pistol_ShootSound(position, shootConfigSO.shootVolume);
                break;
            case GunType.Shotgun:
                WeaponSoundManager.Instance.Shotgun_ShootSound(position, shootConfigSO.shootVolume);
                break;
            case GunType.Sniper:
                WeaponSoundManager.Instance.Sniper_ShootSound(position, shootConfigSO.shootVolume);
                break;
        }

    }
}

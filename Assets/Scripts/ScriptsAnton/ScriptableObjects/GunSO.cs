using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Creates a Sciptable Object for the Guns with Logic so every Weapon is the same
/// </summary>
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

    public AudioClip[] shootSoundClips;

    private MonoBehaviour activeMonoBehaviour;
    private GameObject model;
    private float lastShootTime;
    private ParticleSystem shootSystem;
    private AudioSource shootSound;
    private ObjectPool<TrailRenderer> trailPool;
    private GameObject poolParent;

    public int currentAmmo { get; private set; }
    private int savedAmmo;
    private bool emptyMagazine = false;

    /// <summary>
    /// Instantiates the Model of the Gun first then it will just turn it of and on
    /// </summary>
    /// <param name="parent">The Position of the Parent where it should spawn</param>
    /// <param name="activeMonoBehaviour">The Instance which spawned the Weapon so that a Coroutine can be used</param>
    public void Spawn(Transform parent, MonoBehaviour activeMonoBehaviour) {
        if (model == null) {
            this.activeMonoBehaviour = activeMonoBehaviour;
            lastShootTime = 0f;
            trailPool = new ObjectPool<TrailRenderer>(CreateTrail);
            poolParent = new GameObject("PoolParent");
            model = Instantiate(modelPrefab);
            model.transform.SetParent(parent, false);
            model.transform.localPosition = spawnPoint;
            model.transform.localRotation = Quaternion.Euler(spawnRotation);

            currentAmmo = shootConfigSO.maxAmmo;
        } else {
            model.gameObject.SetActive(true);

            currentAmmo = savedAmmo;
        }
        shootSound = model.GetComponentInChildren<AudioSource>();
        shootSystem = model.GetComponentInChildren<ParticleSystem>();
    }

    /// <summary>
    /// The Shoot Function which is played depending of the firerate.
    /// It plays a Sound and Particle Effect at the Muzzle of the gun
    /// When the Weapon shoots then it will Shoot a Raycast and if it hits then it will play the bullet trail normally,
    /// otherwise the trail will be rendered until it reaches a certain duration
    /// </summary>
    public void Shoot() {
        if (Time.time > shootConfigSO.fireRate + lastShootTime && currentAmmo > 0) {
            emptyMagazine = false;
            lastShootTime = Time.time;

            shootSystem.Play();

            shootSound.clip = shootSoundClips[UnityEngine.Random.Range(0, shootSoundClips.Length)];
            shootSound.volume = shootConfigSO.shootVolume;
            shootSound.Play();

            for (int i = 0; i < shootConfigSO.bulletsPerShoot; i++) {
                Vector3 shootDirection = shootSystem.transform.forward
                    + new Vector3(
                            UnityEngine.Random.Range(-shootConfigSO.spread.x, shootConfigSO.spread.x),
                            UnityEngine.Random.Range(-shootConfigSO.spread.y, shootConfigSO.spread.y),
                            UnityEngine.Random.Range(-shootConfigSO.spread.z, shootConfigSO.spread.z)
                        );
                shootDirection.Normalize();

                if (Physics.Raycast(shootSystem.transform.position, shootDirection, out RaycastHit hit, 100f, shootConfigSO.hitMask)) {
                    activeMonoBehaviour.StartCoroutine(PlayTrail(shootSystem.transform.position, hit.point, hit));
                } else {
                    activeMonoBehaviour.StartCoroutine(PlayTrail(shootSystem.transform.position, shootSystem.transform.position + (shootDirection * trailConfigSO.missDistance), new RaycastHit()));
                }
            }
            currentAmmo--;
        } else if (currentAmmo == 0) {
            emptyMagazine = true;
        }

    }

    #region Trail Creation and Trail Update
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
    #endregion

    /// <summary>
    /// Deactivates the Weapon and sets the shoot Particle to null
    /// </summary>
    public void Despawn() {
        savedAmmo = currentAmmo;
        model.gameObject.SetActive(false);

        shootSystem = null;
    }

    /// <summary>
    /// The Damage logic so Zombies can be damaged
    /// </summary>
    /// <param name="hit">The hit info of the Raycast</param>
    private void HitEnemy(RaycastHit hit) {
        ZombieAI zombie = hit.transform.GetComponentInParent<ZombieAI>();

        if (zombie != null) {
            if (!zombie.IsDead()) {
                zombie.TakeDamage(shootConfigSO.damage);
            }
        }
    }

    public void SetFullMagazine() {
        emptyMagazine = false;
        currentAmmo = shootConfigSO.maxAmmo;
    }

    public bool GetEmptyMagazine() {
        return emptyMagazine;
    }

    public bool MagazineIsFull() {
        return currentAmmo == shootConfigSO.maxAmmo;
    }

}

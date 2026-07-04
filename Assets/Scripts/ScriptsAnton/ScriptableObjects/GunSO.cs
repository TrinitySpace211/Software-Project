using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
///     Creates a Sciptable Object for the Guns with Logic so every Weapon is the same
/// </summary>
[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunSO : ScriptableObject {

    public ImpactType impactType;
    public GunType type;
    public AmmunitionType ammunitionType;
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
    private AudioSource shootSound;
    private GameObject poolParent;
    private GunData gunData;

    public int currentAmmo { get; private set; } = 0;
    private bool emptyMagazine;
    private int savedAmmo;
    private ParticleSystem shootSystem;
    private ObjectPool<TrailRenderer> trailPool;

    private const int maxUpgradeCount = 2;

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

            //Debugging
            /* if (gunData != null) {
                Debug.Log("Spawn1: CurrentAmmo: " + gunData.currentAmmo + ", MaxAmmo: " + gunData.effectiveMaxAmmo + ", Damage: " + gunData.effectiveDamage);
            } */

            if (gunData == null || gunData.effectiveDamage == 0f && gunData.effectiveMaxAmmo == 0f) {
                gunData = new GunData();
                gunData.UpdateStats(0, shootConfigSO.maxAmmo, shootConfigSO.damage);
            }

            currentAmmo = gunData.currentAmmo;
        } else {
            model.SetActive(true);

            currentAmmo = savedAmmo;
        }

        //Debug.Log($"Waffe gespawnt. Munition: {currentAmmo}, Schaden: {gunData.effectiveDamage}, MaxAmmo: {gunData.effectiveMaxAmmo}");

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
        } else if (currentAmmo <= 0) {
            emptyMagazine = true;
        }
    }

    #region Trail Rendering
    /// <summary>
    /// Deactivates the Weapon and sets the shoot Particle to null
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
    /// Cleanup after closing the game or changing the scene
    /// </summary>
    public void DestroyAll() {
        if (model != null) {
            Destroy(model);
            model = null;
        }

        if (poolParent != null) {
            Destroy(poolParent);
            poolParent = null;
        }

        activeMonoBehaviour = null;
        trailPool = null;
        shootSystem = null;
        gunData = null;
    }

    /// <summary>
    ///     The Damage logic so Zombies can be damaged
    /// </summary>
    /// <param name="hit">The hit info of the Raycast</param>
    private void HitEnemy(RaycastHit hit) {
        var damageable = hit.transform.GetComponentInParent<IDamageable>();
        if (damageable != null && !damageable.IsDead())
            damageable.TakeDamage(gunData.effectiveDamage);
    }

    public void SetAmmoAmount(int amount) {
        emptyMagazine = false;
        currentAmmo += amount;
    }

    public bool GetEmptyMagazine() {
        return emptyMagazine;
    }

    public bool MagazineIsFull() {
        return currentAmmo == gunData.effectiveMaxAmmo;
    }

    public int GetMaxAmmo() {
        return gunData.effectiveMaxAmmo;
    }

    public GunData GetGunData() {
        return gunData;
    }

    public void SaveGunData() {
        if (gunData == null) {
            gunData = new GunData();
        }

        gunData.UpdateStats(currentAmmo, gunData.effectiveMaxAmmo, gunData.effectiveDamage);
    }

    public void LoadGunData(GunData gunData) {
        this.gunData = gunData;
    }

    /// <summary>
    /// Increases the effective damage of this gun.
    /// </summary>
    /// <param name="amount">The amount of damage added to the gun.</param>
    public void UpgradeDamage(int amount) {
        if (gunData == null && gunData.effectiveDamage == 0f && gunData.effectiveMaxAmmo == 0f) {
            gunData = new GunData();
            gunData.UpdateStats(0, shootConfigSO.maxAmmo, shootConfigSO.damage);
        }
        gunData.UpdateStats(currentAmmo, gunData.effectiveMaxAmmo, gunData.effectiveDamage + amount);

        gunData.damageUpgradeCount++;
    }

    /// <summary>
    /// Increases the effective maximum ammo capacity of this gun.
    /// </summary>
    /// <param name="amount">The amount of ammo capacity added to the gun.</param>
    public void UpgradeMaxAmmo(int amount) {
        if (gunData == null && gunData.effectiveDamage == 0f && gunData.effectiveMaxAmmo == 0f) {
            gunData = new GunData();
            gunData.UpdateStats(0, shootConfigSO.maxAmmo, shootConfigSO.damage);
        }

        int newMaxAmmo = gunData.effectiveMaxAmmo + amount;
        int newCurrentAmmo = currentAmmo + amount;

        if (newCurrentAmmo > newMaxAmmo) {
            newCurrentAmmo = newMaxAmmo;
        }

        currentAmmo = newCurrentAmmo;

        gunData.UpdateStats(currentAmmo, newMaxAmmo, gunData.effectiveDamage);

        gunData.ammoUpgradeCount++;
    }

    /// <summary>
    /// Resets all runtime data of this GunSO when the ScriptableObject is loaded.
    /// This is important because ScriptableObjects can keep changed runtime values
    /// after Play Mode in the Unity Editor.
    /// </summary>
    private void OnEnable() {
        gunData = null;
        if (model != null) {
            Destroy(model);
            model = null;
        }
        poolParent = null;
        shootSystem = null;
        shootSound = null;
        trailPool = null;
        currentAmmo = 0;
        emptyMagazine = false;
    }

    /// <summary>
    /// Returns true if the damage upgrade can still be bought for this gun.
    /// </summary>
    public bool CanUpgradeDamage() {
        if (gunData == null && gunData.effectiveDamage == 0f && gunData.effectiveMaxAmmo == 0f) {
            gunData = new GunData();
            gunData.UpdateStats(0, shootConfigSO.maxAmmo, shootConfigSO.damage);
        }

        return gunData.damageUpgradeCount < maxUpgradeCount;
    }

    /// <summary>
    /// Returns true if the ammo upgrade can still be bought for this gun.
    /// </summary>
    public bool CanUpgradeMaxAmmo() {
        if (gunData == null && gunData.effectiveDamage == 0f && gunData.effectiveMaxAmmo == 0f) {
            gunData = new GunData();
            gunData.UpdateStats(0, shootConfigSO.maxAmmo, shootConfigSO.damage);
        }

        return gunData.ammoUpgradeCount < maxUpgradeCount;
    }

    /// <summary>
    /// Returns the current effective damage value of this gun.
    /// </summary>
    public int GetDamage() {
        if (gunData == null && gunData.effectiveDamage == 0f && gunData.effectiveMaxAmmo == 0f) {
            gunData = new GunData();
            gunData.UpdateStats(0, shootConfigSO.maxAmmo, shootConfigSO.damage);
        }

        return gunData.effectiveDamage;
    }

    [Serializable]
    public class GunData {
        public int currentAmmo;
        public int effectiveMaxAmmo;
        public int effectiveDamage;
        public int damageUpgradeCount;
        public int ammoUpgradeCount;

        public void UpdateStats(int currentAmmo, int effectiveMaxAmmo, int effectiveDamage) {
            this.currentAmmo = currentAmmo;
            this.effectiveMaxAmmo = effectiveMaxAmmo;
            this.effectiveDamage = effectiveDamage;
        }
    }
}

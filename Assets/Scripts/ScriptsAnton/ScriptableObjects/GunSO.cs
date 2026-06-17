using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering;

/// <summary>
///     Creates a Sciptable Object for the Guns with Logic so every Weapon is the same
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

    private MonoBehaviour activeMonoBehaviour;

    private int currentAmmo;
    private bool emptyMagazine;
    private float lastShootTime;
    private GameObject model;
    private GameObject poolParent;
    private int savedAmmo;
    private ParticleSystem shootSystem;
    private ObjectPool<TrailRenderer> trailPool;

    /// <summary>
    ///     Instantiates the Model of the Gun first then it will just turn it of and on
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
            model.SetActive(true);

            currentAmmo = savedAmmo;
        }

        shootSystem = model.GetComponentInChildren<ParticleSystem>();
    }

    /// <summary>
    ///     The Shoot Function which is played depending of the firerate.
    ///     It plays a Sound and Particle Effect at the Muzzle of the gun
    ///     When the Weapon shoots then it will Shoot a Raycast and if it hits then it will play the bullet trail normally,
    ///     otherwise the trail will be rendered until it reaches a certain duration
    /// </summary>
    public void Shoot() {
        if (Time.time > shootConfigSO.fireRate + lastShootTime && currentAmmo > 0) {
            emptyMagazine = false;
            lastShootTime = Time.time;

            shootSystem.Play();
            ShootByWeaponType(shootSystem.transform.position);

            for (var i = 0; i < shootConfigSO.bulletsPerShoot; i++) {
                var shootDirection = shootSystem.transform.forward
                                     + new Vector3(
                                         Random.Range(-shootConfigSO.spread.x, shootConfigSO.spread.x),
                                         Random.Range(-shootConfigSO.spread.y, shootConfigSO.spread.y),
                                         Random.Range(-shootConfigSO.spread.z, shootConfigSO.spread.z)
                                     );
                shootDirection.Normalize();

                if (Physics.Raycast(shootSystem.transform.position, shootDirection, out var hit, 100f,
                        shootConfigSO.hitMask))
                    activeMonoBehaviour.StartCoroutine(PlayTrail(shootSystem.transform.position, hit.point, hit));
                else
                    activeMonoBehaviour.StartCoroutine(PlayTrail(shootSystem.transform.position,
                        shootSystem.transform.position + shootDirection * trailConfigSO.missDistance,
                        new RaycastHit()));
            }

            currentAmmo--;
        } else if (currentAmmo == 0) {
            emptyMagazine = true;
        }
    }

    /// <summary>
    ///     Deactivates the Weapon and sets the shoot Particle to null
    /// </summary>
    public void Despawn() {
        savedAmmo = currentAmmo;
        model.SetActive(false);

        shootSystem = null;
    }

    /// <summary>
    ///     The Damage logic so Zombies can be damaged
    /// </summary>
    /// <param name="hit">The hit info of the Raycast</param>
    private void HitEnemy(RaycastHit hit) {
        var damageable = hit.transform.GetComponentInParent<IDamageable>();
        if (damageable != null && !damageable.IsDead())
            damageable.TakeDamage(shootConfigSO.damage);
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

    public int GetCurrentAmmo() {
        return currentAmmo;
    }

    public int GetMaxAmmo() {
        return shootConfigSO != null ? shootConfigSO.maxAmmo : 0;
    }

    /// <summary>
    ///     Plays the shoot Sound Effect depending on Weapon Type
    /// </summary>
    /// <param name="position">The Position it should be played at</param>
    private void ShootByWeaponType(Vector3 position) {
        switch (type) {
            case GunType.AssaultRifle:
                SoundManager.Instance.AssaultRilfe_ShootSound(position, shootConfigSO.shootVolume);
                break;
            case GunType.Pistol:
                SoundManager.Instance.Pistol_ShootSound(position, shootConfigSO.shootVolume);
                break;
            case GunType.Shotgun:
                SoundManager.Instance.Shotgun_ShootSound(position, shootConfigSO.shootVolume);
                break;
            case GunType.Sniper:
                SoundManager.Instance.Sniper_ShootSound(position, shootConfigSO.shootVolume);
                break;
        }
    }

    #region Trail Creation and Trail Update

    /// <summary>
    ///     Plays the path of the "bullets" and if it hits and object it plays the corresponding texture impact particle effect
    /// </summary>
    /// <param name="startPoint">The starting position of the "bullets"</param>
    /// <param name="endPoint">The end position of the "bullets"</param>
    /// <param name="hit">The hit object</param>
    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit) {
        var instance = trailPool.Get();
        instance.transform.SetParent(poolParent.transform, false);
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;
        yield return null; // zum korrektem Verarbeiten der Position und Rendering-Initialisierung vor der Bewegung

        instance.emitting = true;

        var distance = Vector3.Distance(startPoint, endPoint);
        var remainingDistance = distance;
        while (remainingDistance > 0f) {
            instance.transform.position =
                Vector3.Lerp(startPoint, endPoint, Mathf.Clamp01(1 - remainingDistance / distance));
            remainingDistance -= trailConfigSO.simulationSpeed * Time.deltaTime;

            yield return null; //damit die Schleife nicht in einem Frame durchläuft, sondern pro Frame
        }

        instance.transform.position = endPoint;

        if (hit.collider != null) {
            SurfaceManager.Instance.HandleImpact(hit.transform.gameObject, endPoint, hit.normal, impactType,
                hit.triangleIndex);

            HitEnemy(hit);
        }

        yield return new WaitForSeconds(trailConfigSO.duration);
        yield return null; // der Trail soll noch für einen weiteren Frame sichtbar sein

        instance.emitting = false;
        instance.gameObject.SetActive(false);
        trailPool.Release(instance);
    }

    /// <summary>
    ///     Creates the bullet path trail
    /// </summary>
    /// <returns>the rendered trail</returns>
    private TrailRenderer CreateTrail() {
        var instance = new GameObject("Bullet Trail");
        var trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = trailConfigSO.color;
        trail.material = trailConfigSO.material;
        trail.widthCurve = trailConfigSO.widthCurve;
        trail.time = trailConfigSO.duration;
        trail.minVertexDistance = trailConfigSO.minVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = ShadowCastingMode.Off;

        return trail;
    }

    #endregion
}
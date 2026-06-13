using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Creates a Sciptable Object for the Melee Weapons with Logic so every Weapon is the same
/// </summary>
[CreateAssetMenu(fileName = "Melee", menuName = "Melees/Melee", order = 0)]
public class MeleeSO : ScriptableObject {
    public ImpactType impactType;
    public MeleeType type;
    public string meleeName;
    public GameObject modelPrefab;
    public WeaponSlot weaponSlot;
    public float attackSpeed;
    public int damage;
    public float swingVolume;
    public ParticleSystem hitParticle;

    private Melee model;
    private float lastHitTime;

    /// <summary>
    /// Instantiates the Model of the Melee Weapon like the GunSO and activates it the rest of the time
    /// </summary>
    /// <param name="parent">The Parent of the Melee Weapon to be placed at</param>
    public void Spawn(Transform parent) {
        if (model == null) {
            lastHitTime = 0f;
            model = Instantiate(modelPrefab).GetComponent<Melee>();
            model.transform.SetParent(parent, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            model.InitMeleeSO(this);
        } else {
            model.gameObject.SetActive(true);
        }

        hitParticle = model.GetComponentInChildren<ParticleSystem>();
    }

    /// <summary>
    /// Plays the hit Particle Effect at a certain position
    /// </summary>
    /// <param name="hitPoint">The Position of the hit</param>

    public IEnumerator PlayHitEffect(Vector3 hitPoint) {
        if (hitParticle == null) {
            yield break;
        }

        ParticleSystem instance = Instantiate(hitParticle.gameObject, hitPoint, Quaternion.identity).GetComponent<ParticleSystem>();
        instance.Play();

        yield return new WaitUntil(() => !instance.IsAlive());

        Destroy(instance.gameObject);
    }

    /// <summary>
    /// A Cooldown of the attack which depends on the attackspeed.
    /// </summary>
    /// <returns>If the time to swing again has past</returns>
    public bool CanSwing() {
        return Time.time > 1 / attackSpeed + lastHitTime;
    }

    /// <summary>
    /// Reset of the Swing Cooldown
    /// </summary>
    public void RecordSwing() {
        lastHitTime = Time.time;
    }

    /// <summary>
    /// Deactivates the Weapon
    /// </summary>
    public void Despawn() {
        model.gameObject.SetActive(false);
    }

    /// <summary>
    /// Setter to change the Attack Speed value
    /// </summary>
    /// <param name="value"></param>
    public void SetAttackSpeed(float value) {
        attackSpeed = value;
    }

    /// <summary>
    /// Is the Player ready to Attack with this Weapon or did he just finished?
    /// If Yes it will play a Melee Swing Sound Effect.
    /// </summary>
    /// <param name="state">Is the Player ready to hit (true) or is he finished (false)</param>
    public void SetMeleeModelAttacking(bool state) {
        model.SetAttacking(state);

        if (state) {
            SoundManager.Instance.Melee_Swing(model.transform.position, swingVolume);
        }
    }

}

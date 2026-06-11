using UnityEngine;

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

    private Melee model;
    private float lastHitTime;
    private ParticleSystem hitParticle;

    public void SpawnModel(Transform parent) {
        if (model == null) {
            lastHitTime = 0f;
            model = Instantiate(modelPrefab).GetComponent<Melee>();
            model.transform.SetParent(parent, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            model.InitMeleeSO(this, hitParticle);
        }

        hitParticle = model.GetComponentInChildren<ParticleSystem>();
    }

    public bool CanSwing() {
        return Time.time > 1 / attackSpeed + lastHitTime;
    }

    public void RecordSwing() {
        lastHitTime = Time.time;
    }

    public void Despawn() {
        model.gameObject.SetActive(false);

        hitParticle = null;
    }

    public void SetAttackSpeed(float value) {
        attackSpeed = value;
    }

    public void SetMeleeModelAttacking(bool state) {
        model.SetAttacking(state);

        if (state) {
            WeaponSoundManager.Instance.Melee_Swing(model.transform.position, swingVolume);
        }
    }

    public void Spawn(Transform parent) {
        if (model == null) {
            SpawnModel(parent);
        } else {
            model.gameObject.SetActive(true);

            hitParticle = model.GetComponentInChildren<ParticleSystem>();
        }
    }

}

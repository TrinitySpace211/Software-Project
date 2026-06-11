using UnityEngine;

public class Melee : MonoBehaviour {

    private bool isAttacking = false;
    private MeleeSO meleeSO;
    private ParticleSystem hitParticle;

    public void InitMeleeSO(MeleeSO meleeSO, ParticleSystem hitParticle) {
        this.meleeSO = meleeSO;
        this.hitParticle = hitParticle;
    }

    private void OnTriggerEnter(Collider other) {
        if (meleeSO.CanSwing() && isAttacking) {
            ZombieAI zombie = other.transform.GetComponentInParent<ZombieAI>();

            //Debug.Log(other);

            if (zombie != null) {
                if (!zombie.IsDead()) {
                    zombie.TakeDamage(meleeSO.damage);
                    //hitParticle.Play();
                }
            }
        }
    }

    public void SetAttacking(bool isAttacking) {
        this.isAttacking = isAttacking;
    }
}

using System;
using UnityEngine;

public class Melee : MonoBehaviour {

    private bool isAttacking = false;
    private MeleeSO meleeSO;

    public void InitMeleeSO(MeleeSO meleeSO) {
        this.meleeSO = meleeSO;
    }

    private void Update() {
        if (meleeSO.CanSwing() && isAttacking) {
            Collider[] hits = Physics.OverlapSphere(transform.position, 0.7f);

            foreach (Collider hit in hits) {
                ZombieAI zombie = hit.GetComponentInParent<ZombieAI>();
                if (zombie != null) {
                    if (!zombie.IsDead()) {
                        zombie.TakeDamage(meleeSO.damage);

                        StartCoroutine(meleeSO.PlayHitEffect(hit.transform.position));
                        meleeSO.RecordSwing();
                        break;
                    }
                }
            }
        }
    }

    public void SetAttacking(bool isAttacking) {
        this.isAttacking = isAttacking;
    }

}

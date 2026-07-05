using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Melee class so that the Update Function can be used
/// </summary>
public class Melee : MonoBehaviour {

    private bool isAttacking = false;
    private MeleeSO meleeSO;

    private readonly HashSet<IDamageable> hitTargets = new();

    /// <summary>
    /// Initializes the MeleeSO that is currently hold by a scriptable Object
    /// </summary>
    /// <param name="meleeSO">The Melee SO</param>
    public void InitMeleeSO(MeleeSO meleeSO) {
        this.meleeSO = meleeSO;
    }

    /// <summary>
    /// Checks if the melee can be swung and attack
    /// </summary>
    public void Update() {
        if (!isAttacking || !meleeSO.CanSwing())
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, 1.2f);

        foreach (Collider hit in hits) {
            if (hit.GetComponentInParent<Player>()) continue;

            IDamageable zombie = hit.GetComponentInParent<IDamageable>();

            if (zombie == null) continue;
            if (hitTargets.Contains(zombie)) continue;
            if (zombie.IsDead()) continue;

            hitTargets.Add(zombie);

            zombie.TakeDamage(meleeSO.damage);
            StartCoroutine(meleeSO.PlayHitEffect(hit.transform.position));
            meleeSO.RecordSwing();
        }

    }

    /// <summary>
    /// Setter for if the player is attacking with the Melee right now
    /// </summary>
    /// <param name="isAttacking">the state of the boolean</param>
    public void SetAttacking(bool isAttacking) {
        this.isAttacking = isAttacking;

        if (isAttacking) {
            hitTargets.Clear();
        }
    }

}

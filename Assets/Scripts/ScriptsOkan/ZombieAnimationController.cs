using UnityEngine;

/// <summary>
///     Setzt Animator-Parameter des Zombies.
///     Wird von ZombieAI aufgerufen, kennt selbst keine Spiellogik.
/// </summary>
public class ZombieAnimationController : MonoBehaviour {
    private const string IS_WALKING = "isWalking";
    private const string IS_ATTACKING = "isAttacking";
    private const string ATTACK_TRIGGER = "Attack";
    private Animator _animator;

    private void Start() {
        _animator = GetComponent<Animator>();
    }

    public void TriggerAttack() {
        if (_animator is not null)
            _animator.SetTrigger(ATTACK_TRIGGER);
    }

    public void SetWalking(bool value) {
        if (_animator is not null)
            _animator.SetBool(IS_WALKING, value);
    }

    public void SetAttacking(bool value) {
        if (_animator is not null)
            _animator.SetBool(IS_ATTACKING, value);
    }
}
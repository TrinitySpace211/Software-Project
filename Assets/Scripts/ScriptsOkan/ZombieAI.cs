using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Steuert das Zombie-Verhalten: Verfolgen, Stoppen, Angreifen.
/// </summary>
public class ZombieAI : MonoBehaviour {
    public int health = 100;

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    /// <summary>Transform des Ziels (Player). Im Inspector setzen.</summary>
    [SerializeField] private Transform target;

    [SerializeField] private GameObject[] joints;

    /// <summary>Stats-ScriptableObject mit Speed, Damage, AttackRange etc.</summary>
    public EnemyStatsSO enemyStatsSO;

    private readonly Color hitColor = Color.red;

    private NavMeshAgent _agent;
    private ZombieAnimationController _animController;

    private float _attackTimer;
    private bool _isAggro;
    private bool _isAttacking;
    private PlayerHealth _targetHealth;
    private bool isDead;
    private Color originalColor;

    private Material zombieMaterial;


    private void Start() {
        _agent = GetComponent<NavMeshAgent>();
        _animController = GetComponent<ZombieAnimationController>();

        if (skinnedMeshRenderer != null) {
            zombieMaterial = skinnedMeshRenderer.material;
            originalColor = zombieMaterial.color;
        }


        _agent.speed = enemyStatsSO.moveSpeed;

        _targetHealth = target.GetComponentInChildren<PlayerHealth>();

        if (enemyStatsSO != null) _agent.speed = enemyStatsSO.moveSpeed;
        if (target != null) _targetHealth = target.GetComponentInChildren<PlayerHealth>();

        //(added some code into scripts so the healthbar display the zombie damage properly)
    }

    /// <summary>
    ///     Handles the zombie's behaviour each frame.
    ///     Transitions between three states based on distance to the player:
    ///     Idle (out of detection range), Chase (within detection range),
    ///     and Attack (within attack range).
    /// </summary>
    private void Update() {
        if (target is null || !_agent.isOnNavMesh) return;

        _attackTimer -= Time.deltaTime;

        // Freeze movement during attack animation
        if (_isAttacking) return;

        if (isDead) {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            return;
        }

        var targetPos = target.position;
        var sqrDist = (transform.position - targetPos).sqrMagnitude;
        var inAttackRange = enemyStatsSO is not null && sqrDist <= enemyStatsSO.attackRange * enemyStatsSO.attackRange;
        var inDetectionRange = enemyStatsSO is not null &&
                               sqrDist <= enemyStatsSO.detectionRange * enemyStatsSO.detectionRange;
        var leashRange = enemyStatsSO is not null ? enemyStatsSO.detectionRange * 1.5f : 0f;
        var beyondLeash = sqrDist > leashRange * leashRange;

        // Aggro: activated when player enters detection range, lost when beyond leash range
        if (inDetectionRange) _isAggro = true;
        if (beyondLeash) _isAggro = false;

        if (inAttackRange) {
            // Player is within melee range — stop and attack
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();
            _animController?.SetWalking(false);
            HandleAttack();
        } else if (_isAggro) {
            // Player detected or aggro'd — chase
            _agent.isStopped = false;
            _agent.SetDestination(targetPos);
            _animController?.SetWalking(_agent.velocity.magnitude > 0.1f);
        } else {
            // Player out of detection range — idle
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _animController?.SetWalking(false);
        }
    }

    private void HandleAttack() {
        if (_attackTimer <= 0f && !_isAttacking) {
            if (enemyStatsSO == null || _targetHealth == null) return;

            _isAttacking = true;
            _animController?.TriggerAttack();
            _targetHealth.TakeDamage(enemyStatsSO.damage);
            _attackTimer = enemyStatsSO.attackCooldown;
        }
    }

    /// <summary>
    ///     Als Animation Event am Ende der Attack-Animation aufrufen.
    ///     Gibt den Zombie für den nächsten Angriff frei.
    /// </summary>
    public void OnAttackAnimationEnd() {
        _isAttacking = false;
    }

    public void TakeDamage(int damage) {
        health -= damage;

        StopAllCoroutines();
        StartCoroutine(HitFeedback());

        if (health <= 0) {
            isDead = true;
            _animController?.SetDead(isDead);

            //Ragdoll
            _animController.enabled = false;

            foreach (var joint in joints) joint.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    private IEnumerator HitFeedback() {
        zombieMaterial.color = hitColor;

        yield return new WaitForSeconds(0.1f);

        zombieMaterial.color = originalColor;
    }

    public bool IsDead() {
        return isDead;
    }
}
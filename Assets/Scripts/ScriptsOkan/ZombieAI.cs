using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Steuert das Zombie-Verhalten: Verfolgen, Stoppen, Angreifen.
/// </summary>
public class ZombieAI : MonoBehaviour {
    public Transform target;
    public EnemyStatsSO stats;

    private NavMeshAgent _agent;
    private ZombieAnimationController _animController;
    private float _attackTimer;

    private bool _isAttacking;
    private PlayerHealth _targetHealth;

    private void Start() {
        _agent = GetComponent<NavMeshAgent>();
        _animController = GetComponent<ZombieAnimationController>();

        if (stats != null)
            _agent.speed = stats.moveSpeed;

        // Schaden-Referenz am Ziel holen
        if (target != null)
            _targetHealth = target.GetComponent<PlayerHealth>();
    }

    private void Update() {
        if (target is null || !_agent.isOnNavMesh) return;

        _attackTimer -= Time.deltaTime;

        // Während Attack-Animation einfrieren
        if (_isAttacking) return;

        var targetPos = target.position;
        var sqrDist = (transform.position - targetPos).sqrMagnitude;
        var inAttackRange = stats is not null && sqrDist <= stats.attackRange * stats.attackRange;

        if (inAttackRange) {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();
            _animController?.SetWalking(false);
            HandleAttack();
        } else {
            _agent.isStopped = false;
            _agent.SetDestination(targetPos);
            _animController?.SetWalking(_agent.velocity.magnitude > 0.1f);
        }
    }

    private void HandleAttack() {
        if (_attackTimer <= 0f && !_isAttacking) {
            _isAttacking = true;
            _animController?.TriggerAttack();
            _targetHealth?.TakeDamage(stats.damage);
            _attackTimer = stats.attackCooldown;
        }
    }

// Diese Methode als Animation Event am Ende der Attack-Animation aufrufen
    public void OnAttackAnimationEnd() {
        _isAttacking = false;
    }
}
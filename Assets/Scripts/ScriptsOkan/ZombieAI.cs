using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Controls zombie behaviour: patrolling, chasing, and attacking the player.
///     Transitions between three states based on distance to the target:
///     Patrol (out of detection range), Chase (within detection range),
///     and Attack (within attack range).
/// </summary>
public class ZombieAI : MonoBehaviour {
    public int health = 100;

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    /// <summary>Transform of the target (player). Assign via Inspector or Init().</summary>
    [SerializeField] public Transform target;

    [SerializeField] private GameObject[] joints;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    /// <summary>ScriptableObject containing speed, damage, attack range, and detection range.</summary>
    public EnemyStatsSO enemyStatsSO;

    private readonly Color hitColor = Color.red;

    private NavMeshAgent _agent;
    private ZombieAnimationController _animController;

    private float _attackTimer;
    private Vector3 _currentPatrolTarget;
    private Vector3 _homePosition;


    private bool _initialized;
    private bool _isAggro;
    private bool _isAttacking;
    private int _patrolIndex;
    private float _patrolRadius;
    private float _patrolWaitTimer;
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

        if (enemyStatsSO != null) _agent.speed = enemyStatsSO.moveSpeed;

        if (target != null)
            _targetHealth = target.GetComponentInChildren<PlayerHealth>();
    }

    /// <summary>
    ///     Evaluated every frame. Manages state transitions between
    ///     Patrol, Chase, and Attack based on distance to the player.
    /// </summary>
    private void Update() {
        if (!_initialized || target is null || !_agent.isOnNavMesh) return;

        _attackTimer -= Time.deltaTime;

        if (_isAttacking) return;

        if (isDead) {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            return;
        }

        var targetPos = target.position;
        var flatSelf = new Vector3(transform.position.x, 0f, transform.position.z);
        var flatTarget = new Vector3(targetPos.x, 0f, targetPos.z);
        var sqrDist = (flatSelf - flatTarget).sqrMagnitude;
        var inAttackRange = enemyStatsSO is not null && sqrDist <= enemyStatsSO.attackRange * enemyStatsSO.attackRange;
        var inDetectionRange = enemyStatsSO is not null &&
                               sqrDist <= enemyStatsSO.detectionRange * enemyStatsSO.detectionRange;
        var leashRange = enemyStatsSO is not null ? enemyStatsSO.detectionRange * 1.5f : 0f;
        var beyondLeash = sqrDist > leashRange * leashRange;

        if (inDetectionRange) _isAggro = true;

        if (beyondLeash) _isAggro = false;

        if (inAttackRange) {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();
            _animController?.SetWalking(false);
            HandleAttack();
        } else if (_isAggro) {
            _agent.isStopped = false;
            _agent.SetDestination(new Vector3(targetPos.x, transform.position.y, targetPos.z));
            _animController?.SetWalking(_agent.velocity.magnitude > 0.1f);
        } else {
            Patrol();
        }
    }

    /// <summary>
    ///     Deals damage to the player if the attack cooldown has elapsed
    ///     and no attack animation is currently playing.
    /// </summary>
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
    ///     Moves the zombie to random points within its patrol radius.
    ///     Waits at each point for <see cref="patrolWaitTime" /> seconds before moving on.
    ///     Does nothing if no patrol radius has been set via <see cref="Init" />.
    /// </summary>
    private void Patrol() {
        if (_patrolRadius <= 0f) {
            _agent.isStopped = true;
            _animController?.SetWalking(false);
            return;
        }

        if (_agent.remainingDistance < 0.5f) {
            _patrolWaitTimer -= Time.deltaTime;
            _animController?.SetWalking(false);

            if (_patrolWaitTimer <= 0f) {
                _currentPatrolTarget = GetRandomPatrolPoint();
                _agent.SetDestination(_currentPatrolTarget);
                _patrolWaitTimer = patrolWaitTime;
            }
        } else {
            _agent.isStopped = false;
            _animController?.SetWalking(_agent.velocity.magnitude > 0.1f);
        }
    }

    /// <summary>
    ///     Called by <see cref="SpawnZone" /> after instantiation.
    ///     Sets the home position, patrol radius, and player target for this zombie.
    /// </summary>
    /// <param name="homePosition">Center point of the patrol area.</param>
    /// <param name="patrolRadius">Maximum distance from home the zombie will wander.</param>
    /// <param name="player">Transform of the player to track and attack.</param>
    public void Init(Vector3 homePosition, float patrolRadius, Transform player) {
        _homePosition = homePosition;
        _patrolRadius = patrolRadius;
        _currentPatrolTarget = GetRandomPatrolPoint();
        _patrolWaitTimer = patrolWaitTime;
        target = player;
        _targetHealth = player?.GetComponentInChildren<PlayerHealth>();
        _initialized = true;
    }

    /// <summary>
    ///     Returns a random NavMesh-reachable point within the patrol radius around the home position.
    /// </summary>
    private Vector3 GetRandomPatrolPoint() {
        var randomOffset = Random.insideUnitCircle * _patrolRadius;
        return new Vector3(
            _homePosition.x + randomOffset.x,
            _homePosition.y,
            _homePosition.z + randomOffset.y
        );
    }

    /// <summary>
    ///     Called as an Animation Event at the end of the attack animation.
    ///     Releases the zombie for its next attack.
    /// </summary>
    public void OnAttackAnimationEnd() {
        _isAttacking = false;
    }

    /// <summary>
    ///     Applies damage to the zombie. Triggers hit feedback and ragdoll on death.
    /// </summary>
    /// <param name="damage">Amount of health points to subtract.</param>
    public void TakeDamage(int damage) {
        health -= damage;

        StopAllCoroutines();
        StartCoroutine(HitFeedback());

        if (health <= 0) {
            isDead = true;
            _animController?.SetDead(isDead);

            _animController.enabled = false;
            foreach (var joint in joints) joint.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    /// <summary>
    ///     Briefly flashes the zombie red to indicate it has been hit.
    /// </summary>
    private IEnumerator HitFeedback() {
        zombieMaterial.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        zombieMaterial.color = originalColor;
    }

    /// <summary>
    ///     Returns true if the zombie has died.
    /// </summary>
    public bool IsDead() {
        return isDead;
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

/// <summary>
///     Controls zombie behaviour: patrolling, chasing, and attacking the player or objective.
///     Transitions between states based on distance to the current target.
/// </summary>
public class ZombieAI : MonoBehaviour {
    public static Action<Vector3> OnTakeDamage;
    public int health = 100;

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    /// <summary>Transform of the current target. Assign via Inspector or Init().</summary>
    [SerializeField] public Transform target;

    [SerializeField] private Rigidbody[] joints;
    [SerializeField] private GameObject attackPoint;
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
    private bool _isTargetingObjective;
    private float _patrolRadius;
    private float _patrolWaitTimer;
    private PlayerHealth _playerHealthRef;

    private Transform _playerTransform;
    private PlayerHealth _targetHealth;
    private GasTankHealth _targetObjectiveHealth;

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

        if (target != null) {
            _targetHealth = target.GetComponentInChildren<PlayerHealth>();
            _playerHealthRef = _targetHealth;
            _playerTransform = target;
        }

        foreach (var joint in joints) joint.isKinematic = true;
    }

    private void Update() {
        if (!_initialized || target is null || !_agent.isOnNavMesh) return;

        _attackTimer -= Time.deltaTime;

        if (isDead) {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            return;
        }

        if (_isAttacking) return;

        if (_isTargetingObjective)
            HandleObjectiveMode();
        else
            HandlePlayerMode();
    }

    /// <summary>
    ///     Objective mode: moves towards the assigned objective.
    ///     If the player enters detection range, temporarily prioritises the player.
    /// </summary>
    private void HandleObjectiveMode() {
        if (_playerTransform != null) {
            var sqrToPlayer = SqrDistFlat(transform.position, _playerTransform.position);
            var detection = enemyStatsSO.detectionRange * enemyStatsSO.detectionRange;

            if (sqrToPlayer <= detection) {
                MoveTo(_playerTransform.position);
                if (sqrToPlayer <= enemyStatsSO.attackRange * enemyStatsSO.attackRange)
                    StopAndAttack(_playerHealthRef, null);
                return;
            }
        }

        var sqrToObj = SqrDistFlat(transform.position, target.position);
        if (sqrToObj <= enemyStatsSO.attackRange * enemyStatsSO.attackRange)
            StopAndAttack(null, _targetObjectiveHealth);
        else
            MoveTo(target.position);
    }

    /// <summary>
    ///     Player mode: manages Patrol → Chase → Attack transitions.
    /// </summary>
    private void HandlePlayerMode() {
        var targetPos = target.position;
        var sqrDist = SqrDistFlat(transform.position, targetPos);
        var inAttackRange = enemyStatsSO != null && sqrDist <= enemyStatsSO.attackRange * enemyStatsSO.attackRange;
        var inDetectionRange =
            enemyStatsSO != null && sqrDist <= enemyStatsSO.detectionRange * enemyStatsSO.detectionRange;
        var leashRange = enemyStatsSO != null ? enemyStatsSO.detectionRange * 1.5f : 0f;
        var beyondLeash = sqrDist > leashRange * leashRange;

        if (inDetectionRange) _isAggro = true;
        if (beyondLeash) _isAggro = false;

        if (inAttackRange)
            StopAndAttack(_targetHealth, null);
        else if (_isAggro)
            MoveTo(targetPos);
        else
            Patrol();
    }

    private float SqrDistFlat(Vector3 a, Vector3 b) {
        var dx = a.x - b.x;
        var dz = a.z - b.z;
        return dx * dx + dz * dz;
    }

    private void MoveTo(Vector3 pos) {
        _agent.isStopped = false;
        _agent.SetDestination(new Vector3(pos.x, transform.position.y, pos.z));
        _animController?.SetWalking(_agent.velocity.magnitude > 0.1f);
    }

    private void StopAndAttack(PlayerHealth ph, GasTankHealth oh) {
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
        _agent.ResetPath();
        _animController?.SetWalking(false);
        _targetHealth = ph;
        _targetObjectiveHealth = oh;
        HandleAttack();
    }

    /// <summary>
    ///     Deals damage via OverlapSphere to player, or directly to objective.
    /// </summary>
    private void HandleAttack() {
        if (_attackTimer > 0f || _isAttacking) return;
        if (enemyStatsSO == null) return;
        if (_targetHealth == null && _targetObjectiveHealth == null) return;

        _isAttacking = true;
        _animController?.TriggerAttack();
        _attackTimer = enemyStatsSO.attackCooldown;

        if (_targetObjectiveHealth != null) {
            _targetObjectiveHealth.TakeDamage(enemyStatsSO.damage);
        } else {
            var hits = Physics.OverlapSphere(attackPoint.transform.position, enemyStatsSO.attackRange);
            foreach (var hit in hits)
                if (hit.GetComponent<Player>() != null) {
                    _targetHealth.TakeDamage(enemyStatsSO.damage);
                    break;
                }
        }
    }

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

    public void Init(Vector3 homePosition, float patrolRadius, Transform player) {
        _homePosition = homePosition;
        _patrolRadius = patrolRadius;
        _currentPatrolTarget = GetRandomPatrolPoint();
        _patrolWaitTimer = patrolWaitTime;
        target = player;
        _playerTransform = player;
        _targetHealth = player?.GetComponentInChildren<PlayerHealth>();
        _playerHealthRef = _targetHealth;
        _initialized = true;
    }

    /// <summary>
    ///     Called by ObjectiveManager to switch target between player and GasTank.
    /// </summary>
    public void SetTarget(Transform newTarget, bool isObjective = false) {
        target = newTarget;
        _isTargetingObjective = isObjective;
        _isAggro = true;

        if (isObjective) {
            _targetHealth = null;
            _targetObjectiveHealth = newTarget?.GetComponent<GasTankHealth>();
        } else {
            _targetObjectiveHealth = null;
            _targetHealth = newTarget?.GetComponentInChildren<PlayerHealth>();
        }
    }

    private Vector3 GetRandomPatrolPoint() {
        var randomOffset = Random.insideUnitCircle * _patrolRadius;
        return new Vector3(
            _homePosition.x + randomOffset.x,
            _homePosition.y,
            _homePosition.z + randomOffset.y
        );
    }

    public void OnAttackAnimationEnd() {
        _isAttacking = false;
    }

    public void TakeDamage(int damage) {
        if (isDead) return;

        health -= damage;
        OnTakeDamage?.Invoke(transform.position);

        if (health <= 0) {
            Die();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(HitFeedback());
    }

    private void Die() {
        isDead = true;
        _animController?.SetDead(true);
        _animController.enabled = false;
        foreach (var joint in joints) joint.isKinematic = false;
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
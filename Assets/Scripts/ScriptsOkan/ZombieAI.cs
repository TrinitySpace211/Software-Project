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
    private bool _isTargetingObjective;
    private int _patrolIndex;
    private float _patrolRadius;
    private float _patrolWaitTimer;
    private PlayerHealth _playerHealthRef;

    private Transform _playerTransform;
    private PlayerHealth _targetHealth;

    private ObjectiveHealth _targetObjectiveHealth;
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
            _initialized = true;
        }
    }

    /// <summary>
    ///     Evaluated every frame. Routes behaviour to either objective or player mode
    ///     depending on the current night state.
    /// </summary>
    private void Update() {
        if (!_initialized || !_agent.isOnNavMesh) return;

        _attackTimer -= Time.deltaTime;
        if (_isAttacking || isDead) {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            return;
        }

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

        MoveTo(target.position);
    }

    /// <summary>
    ///     Player mode: manages Patrol → Chase → Attack transitions
    ///     based on detection and leash range.
    /// </summary>
    private void HandlePlayerMode() {
        var sqrToTarget = SqrDistFlat(transform.position, target.position);
        var detection = enemyStatsSO.detectionRange * enemyStatsSO.detectionRange;
        var leash = enemyStatsSO.detectionRange * 1.5f;

        if (sqrToTarget <= detection) _isAggro = true;
        if (sqrToTarget > leash * leash) _isAggro = false;

        if (sqrToTarget <= enemyStatsSO.attackRange * enemyStatsSO.attackRange)
            StopAndAttack(_targetHealth, _targetObjectiveHealth);
        else if (_isAggro)
            MoveTo(target.position);
        else
            Patrol();
    }

    /// <summary>
    ///     Returns the squared flat (XZ) distance between two points.
    ///     Avoids a square root for cheaper distance comparisons.
    /// </summary>
    private float SqrDistFlat(Vector3 a, Vector3 b) {
        var dx = a.x - b.x;
        var dz = a.z - b.z;
        return dx * dx + dz * dz;
    }

    /// <summary>
    ///     Moves the zombie towards a target position on the XZ plane.
    ///     Updates the walking animation based on actual agent velocity.
    /// </summary>
    private void MoveTo(Vector3 pos) {
        _agent.isStopped = false;
        _agent.SetDestination(new Vector3(pos.x, transform.position.y, pos.z));
        _animController?.SetWalking(_agent.velocity.magnitude > 0.1f);
    }

    /// <summary>
    ///     Stops the agent and triggers an attack against the given target.
    ///     Pass <c>null</c> for whichever target type is not relevant.
    /// </summary>
    private void StopAndAttack(PlayerHealth ph, ObjectiveHealth oh) {
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
        _agent.ResetPath();
        _animController?.SetWalking(false);
        _targetHealth = ph;
        _targetObjectiveHealth = oh;
        HandleAttack();
    }

    /// <summary>
    ///     Deals damage to the current target (player or objective) if the attack cooldown
    ///     has elapsed and no attack animation is currently playing.
    /// </summary>
    private void HandleAttack() {
        if (_attackTimer <= 0f && !_isAttacking) {
            if (enemyStatsSO == null) return;
            if (_targetHealth == null && _targetObjectiveHealth == null) return;

            _isAttacking = true;
            _animController?.TriggerAttack();

            if (_targetObjectiveHealth != null)
                _targetObjectiveHealth.TakeDamage(enemyStatsSO.damage);
            else if (_targetHealth != null)
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
        _playerTransform = player; // <-- neu
        _targetHealth = player?.GetComponentInChildren<PlayerHealth>();
        _playerHealthRef = _targetHealth; // <-- neu
        _initialized = true;
    }

    /// <summary>
    ///     Sets a new target for this zombie.
    ///     Used by ObjectiveManager to switch between player and objectives at night.
    /// </summary>
    public void SetTarget(Transform newTarget, bool isObjective = false) {
        target = newTarget;
        _isTargetingObjective = isObjective;
        _isAggro = true;

        if (isObjective) {
            _targetHealth = null;
            _targetObjectiveHealth = newTarget.GetComponent<ObjectiveHealth>();
        } else {
            _targetObjectiveHealth = null;
            _targetHealth = newTarget?.GetComponentInChildren<PlayerHealth>();
        }
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
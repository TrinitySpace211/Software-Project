using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

/// <summary>
///     Controls zombie behaviour: patrolling, chasing, and attacking the player or objective.
///     Transitions between states based on distance to the current target.
/// </summary>
public class ZombieAI : MonoBehaviour, IDamageable {

    public int health = 100;

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private Material rageEyes;
    [SerializeField] private ParticleSystem dissolveParticle;

    /// <summary>Transform of the current target. Assign via Inspector or Init().</summary>
    [SerializeField] public Transform target;

    [SerializeField] private Rigidbody[] joints;
    [SerializeField] private CapsuleCollider[] capsuleCollider;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float patrolWaitTime = 2f;

    /// <summary>ScriptableObject containing speed, damage, attack range, and detection range.</summary>
    public EnemyStatsSO enemyStatsSO;

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
    private Material originalEyes;
    private readonly Color hitColor = Color.red;

    //Dissolve
    private bool dissolveEnemy = false;
    private float dissolveMeterMin;
    private float dissolveMeterMax;
    private float dissolveMeter;
    private float dissolveSpeed = 1f;

    //Event
    public static Action<Vector3> OnTakeDamage;
    public static event Action<ZombieAI> OnDead;

    private void Start() {
        _agent = GetComponent<NavMeshAgent>();
        _animController = GetComponent<ZombieAnimationController>();

        if (skinnedMeshRenderer != null) {
            zombieMaterial = skinnedMeshRenderer.material;
            originalColor = zombieMaterial.color;

            //The Normal Zombie has 2 Materials (one for the eyes and one for the rest)
            if (skinnedMeshRenderer.materials.Length > 1) {
                originalEyes = skinnedMeshRenderer.materials[1];
            }
        }

        Shader shader = zombieMaterial.shader;
        int propertyIndex = shader.FindPropertyIndex("_DissolveMeter");
        dissolveMeterMin = zombieMaterial.shader.GetPropertyRangeLimits(propertyIndex).x;
        dissolveMeterMax = zombieMaterial.shader.GetPropertyRangeLimits(propertyIndex).y;
        dissolveMeter = dissolveMeterMax;

        ApplyEnemyStats();

        if (target != null) {
            _targetHealth = target.GetComponentInChildren<PlayerHealth>();
            _playerHealthRef = _targetHealth;
            _playerTransform = target;
        }

        foreach (var joint in joints) joint.isKinematic = true;
        foreach (var collider in capsuleCollider) collider.isTrigger = true;
    }

    private void Update() {
        if (dissolveEnemy) {
            dissolveMeter -= Time.deltaTime * dissolveSpeed;
            if (dissolveMeter > dissolveMeterMin) {
                zombieMaterial.SetFloat("_DissolveMeter", dissolveMeter);
            } else {
                dissolveEnemy = false;
                Destroy(gameObject);
            }
        }

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

    private void ApplyEnemyStats() {
        if (_agent == null) return;

        if (enemyStatsSO != null) {
            _agent.speed = enemyStatsSO.moveSpeed;
            _agent.stoppingDistance = Mathf.Max(0.1f, enemyStatsSO.attackRange - 0.05f);
        } else {
            _agent.stoppingDistance = 0.1f;
        }
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


    }

    /// <summary>
    /// An Animation Event plays this function to check if the arm catches the player
    /// </summary>
    public void TryDamageObject() {
        if (isDead) return;

        if (_targetObjectiveHealth != null) {
            _targetObjectiveHealth.TakeDamage(enemyStatsSO.damage);
        } else {
            var hits = Physics.OverlapSphere(attackPoint.position, enemyStatsSO.attackRange);
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

        if (_agent.pathPending) return;

        // "Angekommen" heisst auch: Weg unvollstaendig/ungueltig (Punkt liegt z.B.
        // auf einer nicht erreichbaren NavMesh-Insel wie einem Dach). Sonst bleibt
        // der Zombie fuer immer am letzten erreichbaren Punkt stehen, weil
        // remainingDistance nie unter den Schwellwert faellt.
        // Wichtig: stoppingDistance einrechnen - der Agent haelt regulaer schon
        // stoppingDistance vor dem Ziel an, remainingDistance faellt also nie
        // darunter und eine fixe 0.5er-Schwelle wuerde nie erreicht.
        var arrivedOrBlocked = !_agent.hasPath
            || _agent.remainingDistance <= _agent.stoppingDistance + 0.5f
            || _agent.pathStatus != NavMeshPathStatus.PathComplete;

        if (arrivedOrBlocked) {
            _patrolWaitTimer -= Time.deltaTime;
            _animController?.SetWalking(false);

            if (_patrolWaitTimer <= 0f) {
                _currentPatrolTarget = GetRandomPatrolPoint();
                // isStopped kann nach StopAndAttack noch true sein -> explizit loslaufen.
                _agent.isStopped = false;
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
        ApplyEnemyStats();
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
            _targetObjectiveHealth = newTarget != null ? newTarget.GetComponentInChildren<GasTankHealth>() : null;
        } else {
            _targetObjectiveHealth = null;
            _targetHealth = newTarget != null ? newTarget.GetComponentInChildren<PlayerHealth>() : null;
        }
    }

    private Vector3 GetRandomPatrolPoint() {
        // XZ zufaellig um die Zone, Hoehe vom Zombie selbst (der steht sicher auf
        // dem NavMesh) - die SpawnZone kann beliebig ueber/unter dem Boden liegen.
        var randomOffset = Random.insideUnitCircle * _patrolRadius;
        var candidate = new Vector3(
            _homePosition.x + randomOffset.x,
            transform.position.y,
            _homePosition.z + randomOffset.y
        );

        // Kandidat aufs NavMesh projizieren (wie beim Sprinter). Grosszuegiger
        // Radius, damit auch auf huegeligem Gelaende ein Punkt gefunden wird -
        // unerreichbare Punkte (z.B. Daecher) faengt der PathStatus-Check ab.
        if (NavMesh.SamplePosition(candidate, out var hit, Mathf.Max(_patrolRadius, 4f), NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    public void OnAttackAnimationEnd() {
        _isAttacking = false;
    }

    /// <summary>
    /// This Enemy Instance is going to take Damage (the health gets reduced).
    /// Every time it takes damage it will blink red to give the Player a feedback, that the Enemy got damaged.
    /// If this Enemy goes below 0 Health then it will die.
    /// </summary>
    /// <param name="damage">the amount of damage this Enemy should take</param>
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

    /// <summary>
    /// If the Enemy dies the Animator will be deactivated and 
    /// the Rigidbodys on all the limbs will be set to isKinematic = false (so that the Ragdoll takes Effect),
    /// the Layers of the object and its children will be set to "Ignore Raycast", 
    /// so that the Player is not aiming at the Enemy anymore and a Timer starts.
    /// When the Timer reaches finishes, the Enemy gets dissolved and destroyed afterwards.
    /// </summary>
    private void Die() {
        isDead = true;
        _animController?.SetDead(true);
        _animController.DeactivateAnimator();

        LayerMask layer = LayerMask.NameToLayer("Ignore Raycast");
        SetLayerRecursively(gameObject, layer);

        foreach (var joint in joints) joint.isKinematic = false;
        foreach (var collider in capsuleCollider) collider.isTrigger = false;

        OnDead?.Invoke(this);

        StartCoroutine(DissolveEnemy(3f));
    }

    /// <summary>
    /// Sets the current objects Layer and every Layer of its children
    /// to the one set in the parameter
    /// </summary>
    /// <param name="obj">The GameObject of which the Layer gets changed</param>
    /// <param name="newLayer">The new Layer of the GameObject</param>
    private void SetLayerRecursively(GameObject obj, int newLayer) {
        if (obj == null) return;

        // Ändere den Layer des aktuellen Objekts
        obj.layer = newLayer;

        // Gehe durch alle Kind-Objekte und ändere auch deren Layer
        foreach (Transform child in obj.transform) {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    private IEnumerator DissolveEnemy(float secondsToWait) {
        yield return new WaitForSeconds(secondsToWait);

        if (joints.Length > 0)
            Instantiate(dissolveParticle, joints[0].transform.position, Quaternion.identity);

        dissolveEnemy = true;
    }

    /// <summary>
    /// Paints the Enemy Instance red for a short time and
    /// sets it back to the original Color.
    /// </summary>
    private IEnumerator HitFeedback() {
        zombieMaterial.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        zombieMaterial.color = originalColor;
    }

    public void SetRageEyes() {
        if (rageEyes != null && skinnedMeshRenderer.materials.Length > 1) {
            Material[] materials = skinnedMeshRenderer.materials;
            materials[1] = rageEyes;
            skinnedMeshRenderer.materials = materials;
        }
    }

    public void SetDefaultEyes() {
        if (originalEyes != null && skinnedMeshRenderer.materials.Length > 1) {
            Material[] materials = skinnedMeshRenderer.materials;
            materials[1] = originalEyes;
            skinnedMeshRenderer.materials = materials;
        }
    }

    public bool IsDead() {
        return isDead;
    }

    /* private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        //Gizmos.DrawSphere(attackPoint.position, enemyStatsSO.detectionRange);

        Gizmos.DrawWireSphere(attackPoint.position, enemyStatsSO.attackRange);
    } */
}
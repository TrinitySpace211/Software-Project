using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Tank-Zombie: langsamer, aber deutlich mehr HP und mehr Schaden pro Treffer.
///     Bewusst strukturell identisch zum <see cref="SprinterController" /> aufgebaut
///     (gleiche KI: Roam -> Spieler erkennen -> verfolgen -> angreifen, plus Objective-Mode),
///     damit dieselbe Animator-Einrichtung wiederverwendet werden kann.
///
///     Animator-Parameter (Bool), die dieses Script setzt - im Animator Controller anlegen:
///       isWalking, isSprinting, isAttacking, isDead
///     Der Treffer-Schaden wird ueber ein Animation-Event ausgeloest, das DealDamage() aufruft
///     (im Attack-Clip auf den Treffer-Frame legen - genau wie beim Sprinter).
///
///     Unterschied zum Sprinter = nur die Werte unten:
///       - viel mehr HP (haelt deutlich mehr aus)
///       - mehr Schaden pro Treffer
///       - alle Geschwindigkeiten niedriger (auch der "Sprint" ist langsam)
///       - langsamerer, dafuer haerterer Angriff (groesserer attackCooldown)
///
///     HINWEIS "Tank erscheint erst ab Welle 6": das ist Spawn-/Wellen-Logik und gehoert in den
///     WaveManager (Spawn-Tabelle pro Welle), NICHT in dieses Verhaltensskript.
/// </summary>
public class TankZombieController : MonoBehaviour, IDamageable {
    [Header("Detection")] public float roamRadius = 10f;
    public float detectionRange = 15f;
    public float sprintDistance = 12f;

    [Header("Attack")] public float attackDistance = 2f;
    public float attackCooldown = 2.5f; // langsamer als Sprinter (1.5) -> seltener, aber haerter
    public int attackDamage = 40;       // deutlich mehr als Sprinter (20)

    [Header("Speed")] public float roamSpeed = 1f;
    public float walkSpeed = 1.4f;   // langsamer als Sprinter (2)
    public float sprintSpeed = 2.8f; // "Sprint" des Tanks ist langsamer als der WALK des Sprinters

    [Header("Animation")] public float animationSpeed = 1f;
    public float sprintAnimationSpeed = 0.9f;

    [Header("Roam Wait")] public float roamWaitMin = 1f;
    public float roamWaitMax = 3f;

    [Header("Health")] public int health = 250; // viel mehr als Sprinter (60)
    public bool isDead;

    private NavMeshAgent agent;
    private Animator animator;
    private float attackTimer;
    private BoxCollider boxCollider;

    private Transform currentTarget;
    private bool isAttacking;
    private bool isTargetingObjective;
    private bool isWaiting;
    private Transform player;

    private bool playerDetected;
    private PlayerHealth playerHealth;
    private Vector3 roamTarget;
    private GasTankHealth targetObjectiveHealth;
    private float waitTimer;

    private void Start() {
        boxCollider = GetComponent<BoxCollider>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        currentTarget = player;
        animator.speed = animationSpeed;
        SetNewRoamTarget();
    }

    private void Update() {
        if (isDead) return;

        if (attackTimer > 0f)
            attackTimer -= Time.deltaTime;

        if (isAttacking) return;

        if (!isTargetingObjective) {
            var nearestObjective = ObjectiveManager.Instance?.GetNearestActiveObjective(transform.position);
            if (nearestObjective != null) {
                var distToObjective = Vector3.Distance(transform.position, nearestObjective.position);
                if (distToObjective <= detectionRange)
                    SetTarget(nearestObjective, true);
            }
        }

        if (isTargetingObjective) {
            HandleObjectiveMode();
            return;
        }

        var distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detectionRange)
            playerDetected = true;

        if (!playerDetected) {
            Roam();
            return;
        }

        if (distance <= attackDistance) {
            TryStartAttack();
        } else if (distance <= sprintDistance) {
            Sprint();
            agent.isStopped = false;
            agent.SetDestination(player.position);
        } else {
            Walk();
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
    }

    public void TakeDamage(int damage) {
        if (isDead) return;
        health -= damage;
        ZombieAI.OnTakeDamage?.Invoke(transform.position);
        if (health <= 0)
            Die();
    }

    public bool IsDead() {
        return isDead;
    }

    private void TryStartAttack() {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        LookAtTarget(player.position);

        animator.SetBool("isWalking", false);
        animator.SetBool("isSprinting", false);

        if (attackTimer > 0f) {
            animator.SetBool("isAttacking", false);
            return;
        }

        isAttacking = true;
        attackTimer = attackCooldown;
        animator.SetBool("isAttacking", true);
    }

    private void HandleObjectiveMode() {
        if (player != null) {
            var distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer <= attackDistance) {
                TryStartAttack();
                return;
            }
        }

        var targetPos = currentTarget.position;
        var distToObjective = Vector3.Distance(transform.position, targetPos);

        if (distToObjective <= attackDistance) {
            TryStartAttack();
        } else if (distToObjective <= sprintDistance) {
            Sprint();
            agent.isStopped = false;
            agent.SetDestination(targetPos);
        } else {
            Walk();
            agent.isStopped = false;
            agent.SetDestination(targetPos);
        }
    }

    private void Roam() {
        agent.speed = roamSpeed;
        animator.speed = animationSpeed;
        animator.SetBool("isSprinting", false);
        animator.SetBool("isAttacking", false);

        if (isWaiting) {
            waitTimer -= Time.deltaTime;
            agent.isStopped = true;
            animator.SetBool("isWalking", false);

            if (waitTimer <= 0f) {
                isWaiting = false;
                agent.isStopped = false;
                SetNewRoamTarget();
            }

            return;
        }

        animator.SetBool("isWalking", true);

        if (agent.remainingDistance < 1f && !agent.pathPending) {
            isWaiting = true;
            waitTimer = Random.Range(roamWaitMin, roamWaitMax);
        }
    }

    private void Walk() {
        agent.isStopped = false;
        agent.speed = walkSpeed;
        animator.speed = animationSpeed;
        animator.SetBool("isWalking", true);
        animator.SetBool("isSprinting", false);
        animator.SetBool("isAttacking", false);
    }

    private void Sprint() {
        agent.isStopped = false;
        agent.speed = sprintSpeed;
        animator.speed = sprintAnimationSpeed;
        animator.SetBool("isWalking", false);
        animator.SetBool("isSprinting", true);
        animator.SetBool("isAttacking", false);
    }

    private void SetNewRoamTarget() {
        var randomDirection = Random.insideUnitSphere * roamRadius + transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, 1)) {
            roamTarget = hit.position;
            agent.SetDestination(roamTarget);
        }
    }

    private void LookAtTarget(Vector3 targetPos) {
        var direction = targetPos - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(direction.normalized),
                Time.deltaTime * 10f
            );
    }

    /// <summary>
    ///     Wird per Animation-Event im Attack-Clip aufgerufen (Treffer-Frame) und teilt
    ///     den Schaden aus - entweder ans Objective (Objective-Mode) oder an den Spieler.
    /// </summary>
    public void DealDamage() {
        if (isDead) return;

        if (isTargetingObjective) {
            if (targetObjectiveHealth != null)
                targetObjectiveHealth.TakeDamage(attackDamage);
        } else {
            if (playerHealth != null && !playerHealth.GetIsDead()) {
                var dist = Vector3.Distance(transform.position, player.position);
                if (dist <= attackDistance + 0.5f)
                    playerHealth.TakeDamage(attackDamage);
            }
        }

        isAttacking = false;
        animator.SetBool("isAttacking", false);
        agent.isStopped = false;
    }

    /// <summary>
    ///     Setzt das Ziel. Wird u.a. vom ObjectiveManager aufgerufen (Objective-Mode nachts).
    /// </summary>
    public void SetTarget(Transform newTarget, bool isObjective = false) {
        currentTarget = newTarget;
        isTargetingObjective = isObjective;
        playerDetected = true;

        if (isObjective)
            targetObjectiveHealth = newTarget != null ? newTarget.GetComponent<GasTankHealth>() : null;
        else
            targetObjectiveHealth = null;
    }

    public void Die() {
        if (isDead) return;

        isDead = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        animator.SetBool("isDead", true);

        if (boxCollider != null)
            boxCollider.enabled = false;
    }
}

using UnityEngine;
using UnityEngine.AI;

/// <summary>
///     Controls the Sprinter Zombie enemy type.
///     Behaviour states: Roam → Detect Player/Objective → Walk → Sprint → Attack → Die
///     The Sprinter detects the player earlier and starts sprinting from further
///     away compared to the normal zombie.
///     Supports switching target to an objective via SetTarget().
/// </summary>
public class SprinterController : MonoBehaviour, IDamageable {
    [Header("Detection")] public float roamRadius = 10f;
    public float detectionRange = 15f;
    public float sprintDistance = 12f;

    [Header("Attack")] public float attackDistance = 1.5f;
    public float objectiveAttackDistance = 3f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 20;

    [Header("Speed")] public float roamSpeed = 1.5f;
    public float walkSpeed = 2f;
    public float sprintSpeed = 7f;

    [Header("Animation")] public float animationSpeed = 1.3f;
    public float sprintAnimationSpeed = 0.9f;

    [Header("Roam Wait")] public float roamWaitMin = 1f;
    public float roamWaitMax = 3f;

    [Header("Health")] public int health = 60;
    public bool isDead;

    private NavMeshAgent agent;
    private Animator animator;
    private float attackTimer;
    private Transform currentTarget;
    private bool isAttacking;
    private bool isSprinting;
    private bool isTargetingObjective;
    private bool isWaiting;
    private Transform player;
    private bool playerDetected;
    private PlayerHealth playerHealth;
    private Vector3 roamTarget;
    private GasTankHealth targetObjectiveHealth;
    private float waitTimer;

    private void Start() {
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

        attackTimer -= Time.deltaTime;

        if (isAttacking && attackTimer <= 0f) {
            isAttacking = false;
            animator.SetBool("isAttacking", false);
        }

        if (isAttacking) return;

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

        if (distance <= attackDistance)
            Attack(playerHealth, null);
        else if (distance <= sprintDistance)
            Sprint();
        else
            Walk();

        agent.SetDestination(player.position);
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

    /// <summary>
    ///     Objective mode: moves toward the objective.
    ///     Attacks the player only if directly in attack range, otherwise focuses objective.
    /// </summary>
    private void HandleObjectiveMode() {
        if (isAttacking) return;

        if (player != null) {
            var distToPlayer = Vector3.Distance(transform.position, player.position);
            if (distToPlayer <= attackDistance) {
                Attack(playerHealth, null);
                return;
            }
        }

        var distToObj = Vector3.Distance(transform.position, currentTarget.position);
        if (distToObj <= objectiveAttackDistance + 0.5f) // ← kleiner Buffer
            Attack(null, targetObjectiveHealth);
        else if (distToObj <= sprintDistance)
            Sprint();
        else
            Walk();

        MoveTowardsObjective();
    }

    /// <summary>
    ///     Sets destination to a point outside the objective so the sprinter stops at the edge.
    /// </summary>
    private void MoveTowardsObjective() {
        var dirToObj = (currentTarget.position - transform.position).normalized;
        var stoppingPos = currentTarget.position - dirToObj * objectiveAttackDistance;
        agent.SetDestination(stoppingPos);
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

    private void SetNewRoamTarget() {
        var randomDirection = Random.insideUnitSphere * roamRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, roamRadius, 1)) {
            roamTarget = hit.position;
            agent.SetDestination(roamTarget);
        }
    }

    private void Walk() {
        agent.isStopped = false;
        isSprinting = false;
        agent.speed = walkSpeed;
        animator.speed = animationSpeed;
        animator.SetBool("isWalking", true);
        animator.SetBool("isSprinting", false);
        animator.SetBool("isAttacking", false);
    }

    private void Sprint() {
        agent.isStopped = false;
        isSprinting = true;
        agent.speed = sprintSpeed;
        animator.speed = sprintAnimationSpeed;
        animator.SetBool("isWalking", false);
        animator.SetBool("isSprinting", true);
        animator.SetBool("isAttacking", false);
    }

    private void Attack(PlayerHealth ph, GasTankHealth oh) {
        isSprinting = false;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        var direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(direction), Time.deltaTime * 5f);

        animator.SetBool("isWalking", false);
        animator.SetBool("isSprinting", false);

        if (attackTimer <= 0f) {
            isAttacking = true;
            attackTimer = attackCooldown;
            animator.SetBool("isAttacking", true);

            if (oh != null)
                oh.TakeDamage(attackDamage);
            else if (ph != null && !ph.GetIsDead())
                ph.TakeDamage(attackDamage);
        }
    }

    /// <summary>
    ///     Switches the sprinter's target between the player and an objective.
    ///     Called by ObjectiveManager — mirrors ZombieAI.SetTarget().
    /// </summary>
    public void SetTarget(Transform newTarget, bool isObjective = false) {
        currentTarget = newTarget;
        isTargetingObjective = isObjective;
        playerDetected = true;

        if (isObjective)
            targetObjectiveHealth = newTarget?.GetComponent<GasTankHealth>();
        else
            targetObjectiveHealth = null;
    }

    public void Die() {
        if (isDead) return;
        isDead = true;
        agent.enabled = false;

        if (isSprinting)
            animator.SetTrigger("deadFly");
        else
            animator.SetTrigger("deadNormal");
    }
}
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class SprinterController : MonoBehaviour, IDamageable {
    [Header("Detection")] public float roamRadius = 10f;
    public float detectionRange = 15f;
    public float sprintDistance = 12f;

    [Header("Attack")] public float attackDistance = 1.5f;
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

    [Header("Material & Dissolve")]
    [SerializeField] private ParticleSystem dissolveParticle;
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private Transform particlePoint;

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

    //Material
    private Color originalColor;
    private Material zombieMaterial;
    private readonly Color hitColor = Color.red;

    //Dissolve Parameters
    private bool dissolveEnemy = false;
    private float dissolveMeterMin;
    private float dissolveMeterMax;
    private float dissolveMeter;
    private float dissolveSpeed = 1f;

    //Event
    public static event Action<SprinterController> OnDead;

    private void Start() {
        boxCollider = GetComponent<BoxCollider>();
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player").transform;
        playerHealth = player.GetComponent<PlayerHealth>();
        currentTarget = player;
        animator.speed = animationSpeed;
        SetNewRoamTarget();

        if (skinnedMeshRenderer != null) {
            zombieMaterial = skinnedMeshRenderer.material;
            originalColor = zombieMaterial.color;
        }

        Shader shader = zombieMaterial.shader;
        int propertyIndex = shader.FindPropertyIndex("_DissolveMeter");
        dissolveMeterMin = zombieMaterial.shader.GetPropertyRangeLimits(propertyIndex).x;
        dissolveMeterMax = zombieMaterial.shader.GetPropertyRangeLimits(propertyIndex).y;
        dissolveMeter = dissolveMeterMax;
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
        if (health <= 0) {
            Die();
            return;
        }

        StopAllCoroutines();
        StartCoroutine(HitFeedback());
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
        //animator.SetBool("isWalking", false);
        //animator.SetBool("isSprinting", false);
        // animator.SetBool("isAttacking", false);

        if (boxCollider != null)
            boxCollider.enabled = false;

        OnDead?.Invoke(this);

        StartCoroutine(DissolveEnemy(3f));
    }

    private IEnumerator HitFeedback() {
        zombieMaterial.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        zombieMaterial.color = originalColor;
    }

    private IEnumerator DissolveEnemy(float secondsToWait) {
        yield return new WaitForSeconds(secondsToWait);

        Instantiate(dissolveParticle, particlePoint.position, Quaternion.identity, particlePoint);

        dissolveEnemy = true;
    }
}
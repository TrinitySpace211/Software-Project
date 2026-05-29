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

    private PlayerHealth _targetHealth;
    private NavMeshAgent _agent;
    private ZombieAnimationController _animController;

    private Material zombieMaterial;
    private Color originalColor;
    private Color hitColor = Color.red;

    private float _attackTimer;
    private bool _isAttacking;
    private bool isDead = false;


    private void Start() {
        _agent = GetComponent<NavMeshAgent>();
        _animController = GetComponent<ZombieAnimationController>();

        zombieMaterial = skinnedMeshRenderer.material;
        originalColor = zombieMaterial.color;

        if (enemyStatsSO != null)
            _agent.speed = enemyStatsSO.moveSpeed;

        if (target != null)
            _targetHealth = target.GetComponent<PlayerHealth>();

        foreach (var joint in joints) {
            joint.GetComponent<Rigidbody>().isKinematic = true;
        }
    }

    private void Update() {
        if (target is null || !_agent.isOnNavMesh) return;

        _attackTimer -= Time.deltaTime;

        // Während Attack-Animation einfrieren
        if (_isAttacking) return;

        var targetPos = target.position;
        var sqrDist = (transform.position - targetPos).sqrMagnitude;
        var inAttackRange = enemyStatsSO is not null && sqrDist <= enemyStatsSO.attackRange * enemyStatsSO.attackRange;

        if (isDead) {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
        } else if (!inAttackRange) {
            _agent.isStopped = false;
            _agent.SetDestination(targetPos);
            _animController?.SetWalking(_agent.velocity.magnitude > 0.1f);
        } else {
            _agent.isStopped = true;
            _agent.velocity = Vector3.zero;
            _agent.ResetPath();
            _animController?.SetWalking(false);
            HandleAttack();
        }
    }

    private void HandleAttack() {
        if (_attackTimer <= 0f && !_isAttacking) {
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

            foreach (var joint in joints) {
                joint.GetComponent<Rigidbody>().isKinematic = false;
            }
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
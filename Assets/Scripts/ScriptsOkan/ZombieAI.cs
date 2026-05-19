using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Steuert das Zombie-verhalten.
/// </summary>
public class ZombieAI : MonoBehaviour {
    private const string IS_DEAD = "isDead";

    public int health = 100;

    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;

    public Transform target;

    private NavMeshAgent agent;
    private Animator animator;
    private Material zombieMaterial;
    private Color originalColor;
    private Color hitColor = Color.red;

    private bool isDead = false;


    /// <summary>
    /// Holt sich die benötigten Komponenten vom Zombie.
    /// </summary>
    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        zombieMaterial = skinnedMeshRenderer.material;
        originalColor = zombieMaterial.color;
    }
    /// <summary>
    /// Lässt den Zombie dem Ziel folgen und setzt die Lauf-Animation.
    /// </summary>
    private void Update() {
        if (target != null && agent.isOnNavMesh && !isDead) {
            agent.SetDestination(target.position);
        }

        if (isDead) {
            agent.velocity = Vector3.zero;
        }

        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (animator != null) {
            animator.SetBool("isWalking", isMoving);
        }
    }

    public void TakeDamage(int damage) {
        health -= damage;

        StopAllCoroutines();
        StartCoroutine(HitFeedback());

        if (health <= 0) {
            isDead = true;
            animator.SetBool(IS_DEAD, isDead);
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
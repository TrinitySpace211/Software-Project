using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Steuert das Zombie-verhalten.
/// </summary>
public class ZombieAI : MonoBehaviour
{
    public Transform target;
    
    private NavMeshAgent agent;
    private Animator animator;
    
    /// <summary>
    /// Holt sich die benötigten Komponenten vom Zombie.
    /// </summary>
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }
    /// <summary>
    /// Lässt den Zombie dem Ziel folgen und setzt die Lauf-Animation.
    /// </summary>
    void Update()
    {
        //
        if (target != null && agent.isOnNavMesh)
        {
            agent.SetDestination(target.position);
        }
        
        bool isMoving = agent.velocity.magnitude > 0.1f;

        if (animator != null) {
            animator.SetBool("isWalking", isMoving);
        }
    }
}
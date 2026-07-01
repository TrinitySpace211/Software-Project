using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Updates the NPC animator based on the current NavMeshAgent movement speed.
/// </summary>
public class NPCAnimationController : MonoBehaviour {
    [Header("References")]
    public NavMeshAgent agent;
    public Animator animator;

    [Header("Animator Parameters")]
    public string speedParameterName = "Speed";

    private void Awake() {
        if (agent == null) {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Update() {
        if (agent == null || animator == null)
            return;

        // Get the current movement speed of the NavMeshAgent.
        float currentSpeed = agent.velocity.magnitude;

        // Send the current speed to the Animator.
        animator.SetFloat(speedParameterName, currentSpeed);
    }
}
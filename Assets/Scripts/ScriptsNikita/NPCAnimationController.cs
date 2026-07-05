using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Updates the NPC animator based on the current NavMeshAgent movement speed.
/// </summary>
public class NPCAnimationController : MonoBehaviour {
    [Header("References")]

    /// <summary>
    /// Reference to the NavMeshAgent that moves the NPC.
    /// </summary>
    public NavMeshAgent agent;

    /// <summary>
    /// Reference to the Animator that controls the NPC animations.
    /// </summary>
    public Animator animator;

    [Header("Animator Parameters")]

    /// <summary>
    /// Name of the Animator float parameter used to control the movement animation.
    /// </summary>
    public string speedParameterName = "Speed";

    /// <summary>
    /// Gets the NavMeshAgent component if no agent was assigned in the Inspector.
    /// </summary>
    private void Awake() {
        // If no NavMeshAgent was assigned manually, try to get it from this GameObject.
        if (agent == null) {
            agent = GetComponent<NavMeshAgent>();
        }
    }

    /// <summary>
    /// Updates the Animator with the current movement speed of the NPC.
    /// </summary>
    private void Update() {
        // Stop if the NavMeshAgent or Animator reference is missing.
        if (agent == null || animator == null)
            return;

        // Get the current movement speed of the NavMeshAgent.
        float currentSpeed = agent.velocity.magnitude;

        // Send the current speed to the Animator.
        animator.SetFloat(speedParameterName, currentSpeed);
    }
}
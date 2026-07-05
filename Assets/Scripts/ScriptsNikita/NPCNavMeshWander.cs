using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Moves the NPC randomly on the NavMesh within a limited area.
/// The NPC walks around, waits sometimes and stays inside its defined wander radius.
/// </summary>
public class NPCNavMeshWander : MonoBehaviour {
    [Header("Movement")]

    /// <summary>
    /// Maximum distance from the start position where the NPC can wander.
    /// </summary>
    public float wanderRadius = 6f;

    /// <summary>
    /// Minimum time the NPC waits before choosing a new destination.
    /// </summary>
    public float minWaitTime = 2f;

    /// <summary>
    /// Maximum time the NPC waits before choosing a new destination.
    /// </summary>
    public float maxWaitTime = 5f;

    [Header("Timing")]

    /// <summary>
    /// Time interval between checks to see if the NPC has reached its destination.
    /// </summary>
    public float destinationCheckInterval = 0.3f;

    /// <summary>
    /// Reference to the NavMeshAgent used to move the NPC.
    /// </summary>
    private NavMeshAgent agent;

    /// <summary>
    /// Stores the NPC's initial position.
    /// The NPC will wander around this position.
    /// </summary>
    private Vector3 startPosition;

    /// <summary>
    /// Timer used while the NPC is waiting.
    /// </summary>
    private float waitTimer;

    /// <summary>
    /// Timer used to control how often the destination is checked.
    /// </summary>
    private float checkTimer;

    /// <summary>
    /// Stores whether the NPC is currently waiting.
    /// </summary>
    private bool isWaiting;

    /// <summary>
    /// Gets the NavMeshAgent component and stores the NPC's start position.
    /// </summary>
    private void Awake() {
        // Get the NavMeshAgent component attached to this GameObject.
        agent = GetComponent<NavMeshAgent>();

        // Store the initial position as the center point for wandering.
        startPosition = transform.position;
    }

    /// <summary>
    /// Selects the first destination when the game starts.
    /// </summary>
    private void Start() {
        SelectNewDestination();
    }

    /// <summary>
    /// Handles the NPC wandering and waiting logic every frame.
    /// </summary>
    private void Update() {
        // Stop the update logic if there is no agent or if the agent is disabled.
        if (agent == null || !agent.enabled)
            return;

        // If the NPC is currently waiting, count down the wait timer.
        if (isWaiting) {
            waitTimer -= Time.deltaTime;

            // When the wait time is over, allow movement again and select a new destination.
            if (waitTimer <= 0f) {
                isWaiting = false;
                SelectNewDestination();
            }

            return;
        }

        // Count down the timer for checking the current destination.
        checkTimer -= Time.deltaTime;

        // Only check the destination after the defined interval.
        if (checkTimer <= 0f) {
            checkTimer = destinationCheckInterval;

            // If the path is ready and the NPC has reached its destination, start waiting.
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance) {
                StartWaiting();
            }
        }
    }

    /// <summary>
    /// Selects a new random destination inside the wander radius.
    /// The destination must be on the NavMesh.
    /// </summary>
    private void SelectNewDestination() {
        // Create a random direction inside a sphere and scale it by the wander radius.
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;

        // Move the random position around the NPC's original start position.
        randomDirection += startPosition;

        // Keep the destination on the same height as the start position.
        randomDirection.y = startPosition.y;

        // Stores the closest valid position found on the NavMesh.
        NavMeshHit hit;

        // Try to find a valid position on the NavMesh near the random position.
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas)) {
            // Move the NPC to the valid NavMesh position.
            agent.SetDestination(hit.position);
        } else {
            // If no valid position was found, let the NPC wait before trying again.
            StartWaiting();
        }
    }

    /// <summary>
    /// Makes the NPC wait for a random amount of time.
    /// </summary>
    private void StartWaiting() {
        // Set the NPC into waiting mode.
        isWaiting = true;

        // Choose a random wait time between the minimum and maximum wait values.
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    /// <summary>
    /// Stops the NPC movement.
    /// This can be used while the player is talking to the NPC.
    /// </summary>
    public void StopMovement() {
        // Stop if there is no NavMeshAgent reference.
        if (agent == null)
            return;

        // Pause the movement of the NavMeshAgent.
        agent.isStopped = true;
    }

    /// <summary>
    /// Allows the NPC to move again.
    /// </summary>
    public void ResumeMovement() {
        // Stop if there is no NavMeshAgent reference.
        if (agent == null)
            return;

        // Resume the movement of the NavMeshAgent.
        agent.isStopped = false;

        // If the NPC is not currently waiting, immediately select a new destination.
        if (!isWaiting) {
            SelectNewDestination();
        }
    }
}
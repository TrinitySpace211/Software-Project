using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Moves the NPC randomly on the NavMesh within a limited area.
/// The NPC walks around, waits sometimes and stays inside its defined wander radius.
/// </summary>
public class NPCNavMeshWander : MonoBehaviour {
    [Header("Movement")]
    public float wanderRadius = 6f;
    public float minWaitTime = 2f;
    public float maxWaitTime = 5f;

    [Header("Timing")]
    public float destinationCheckInterval = 0.3f;

    private NavMeshAgent agent;
    private Vector3 startPosition;
    private float waitTimer;
    private float checkTimer;
    private bool isWaiting;

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();
        startPosition = transform.position;
    }

    private void Start() {
        SelectNewDestination();
    }

    private void Update() {
        if (agent == null || !agent.enabled)
            return;

        if (isWaiting) {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f) {
                isWaiting = false;
                SelectNewDestination();
            }

            return;
        }

        checkTimer -= Time.deltaTime;

        if (checkTimer <= 0f) {
            checkTimer = destinationCheckInterval;

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
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
        randomDirection += startPosition;
        randomDirection.y = startPosition.y;

        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas)) {
            agent.SetDestination(hit.position);
        } else {
            StartWaiting();
        }
    }

    /// <summary>
    /// Makes the NPC wait for a random amount of time.
    /// </summary>
    private void StartWaiting() {
        isWaiting = true;
        waitTimer = Random.Range(minWaitTime, maxWaitTime);
    }

    /// <summary>
    /// Stops the NPC movement.
    /// This can be used while the player is talking to the NPC.
    /// </summary>
    public void StopMovement() {
        if (agent == null)
            return;

        agent.isStopped = true;
    }

    /// <summary>
    /// Allows the NPC to move again.
    /// </summary>
    public void ResumeMovement() {
        if (agent == null)
            return;

        agent.isStopped = false;

        if (!isWaiting) {
            SelectNewDestination();
        }
    }
}
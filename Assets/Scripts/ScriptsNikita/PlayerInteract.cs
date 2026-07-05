using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player interaction by checking for nearby interactable NPCs.
/// </summary>
public class PlayerInteract : MonoBehaviour {

    /// <summary>
    /// Reference to the player input handler, used to read and process player input.
    /// </summary>
    private PlayerInputHandler playerInputHandler;

    /// <summary>
    /// Reference to the player component, used to access player-related data and functionality.
    /// </summary>
    private Player player;

    private void Start() {
        // Get the Player component attached to this GameObject.
        player = GetComponent<Player>();

        // Get the PlayerInputHandler from the Player component.
        playerInputHandler = player.GetPlayerInputHandler();
    }

    /// <summary>
    /// Checks each frame whether the player presses the interaction key.
    /// If the Interact key is pressed, it searches for nearby interactable NPCs.
    /// </summary>
    void Update() {
        if (playerInputHandler.InteractTriggered) {
            float interactRange = 1.7f;

            // Find all colliders within the interaction range
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);

            // Check each collider for an interactable NPC component
            foreach (Collider collider in colliderArray) {
                if (collider.TryGetComponent(out NPCInteractable nInteractable)) {

                    // Trigger the interaction with the NPC
                    nInteractable.Interact();
                }
            }
        }
    }
}

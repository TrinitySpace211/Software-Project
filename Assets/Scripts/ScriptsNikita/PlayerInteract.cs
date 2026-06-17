using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player interaction by checking for nearby interactable NPCs.
/// </summary>
public class PlayerInteract : MonoBehaviour
{
    /// <summary>
    /// Checks each frame whether the player presses the interaction key.
    /// If the E key is pressed, it searches for nearby interactable NPCs.
    /// </summary>
    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame) {
            float interactRange = 1.7f;

            // Find all colliders within the interaction range
            Collider[] colliderArray = Physics.OverlapSphere(transform.position, interactRange);

            // Check each collider for an interactable NPC component
            foreach (Collider collider in colliderArray) {
                if(collider.TryGetComponent(out NPCInteractable nInteractable)) {

                    // Trigger the interaction with the NPC
                    nInteractable.Interact();
                }
            }
        }
    }
}

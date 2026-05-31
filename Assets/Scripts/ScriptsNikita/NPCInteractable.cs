using UnityEngine;

/// <summary>
/// Makes this NPC interactable and opens its dialog when interacted with.
/// </summary>
public class NPCInteractable : MonoBehaviour
{
    /// <summary>
    /// Reference to the NPC dialog that should be opened.
    /// </summary>
    public NPCDialog npcDialog;

    /// <summary>
    /// Handles the interaction with this NPC.
    /// </summary>
    public void Interact() {
        Debug.Log("Interact");

        // Open the assigned NPC dialog
        npcDialog.OpenDialog();
    }
}

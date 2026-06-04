using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the NPC dialog UI, function menu, tower spawning, and weapon upgrade.
/// </summary>
public class NPCDialog : MonoBehaviour {

    /// <summary>
    /// The main dialog panel shown when the player interacts with the NPC.
    /// </summary>
    public GameObject dialogPanel;

    /// <summary>
    /// The panel that contains additional NPC functions or options.
    /// </summary>
    public GameObject functionsPanel;

    /// <summary>
    /// The tower prefab that will be spawned.
    /// </summary>
    public GameObject towerPrefab;

    /// <summary>
    /// The position and rotation where the tower should be spawned.
    /// </summary>
    public Transform towerSpawnPoint;

    /// <summary>
    /// True while the NPC dialog is open.
    /// Can be read by other scripts, but can only be changed inside NPCDialog.
    /// </summary>
    public bool IsDialogOpen { get; private set; }

    /// <summary>
    /// Prepares the UI panels by briefly activating them and then disabling them again.
    /// This helps avoid visual stuttering when switching between panels.
    /// </summary>
    public void start() {

        // Briefly activate both panels and then deactivate them again to avoid stuttering when switching panels
        dialogPanel.SetActive(true);
        functionsPanel.SetActive(true);

        // Force Unity to update the canvas layout immediately
        Canvas.ForceUpdateCanvases();

        // Hide both panels after the canvas has been updated
        dialogPanel.SetActive(false);
        functionsPanel.SetActive(false);
    }

    /// <summary>
    /// Opens the NPC dialog panel and unlocks the cursor for UI interaction.
    /// </summary>
    public void OpenDialog() {
        IsDialogOpen = true;

        // Show the dialog panel and hide the functions panel
        dialogPanel.SetActive(true);
        functionsPanel.SetActive(false);

        // Unlock and show the cursor so the player can interact with the UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Clear the currently selected UI element.
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Closes both NPC UI panels and keeps the cursor visible.
    /// </summary>
    public void CloseDialog() {
        IsDialogOpen = false;

        // Hide both panels
        dialogPanel.SetActive(false);
        functionsPanel.SetActive(false);

        // Unlock and show the cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Clear the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Returns from the functions/upgrade panel back to the main dialog panel.
    /// </summary>
    public void Back() {
        // Hide the functions panel and show the dialog panel again
        functionsPanel.SetActive(false);
        dialogPanel.SetActive(true);

        // Clear the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Handles the upgrade option selected by the player.
    /// </summary>
    public void Upgrade() {
        Debug.Log("Upgrade gewählt");
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Handles the functions option selected by the player.
    /// </summary>
    public void Functions() {
        // Hide the dialog panel and show the functions panel
        dialogPanel.SetActive(false);
        functionsPanel.SetActive(true);

        // Clear the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null);
    }

    /// <summary>
    /// Spawns a tower at the assigned spawn point.
    /// </summary>
    public void SpawnTower() {
        Debug.Log("Tower ausgewählt");

        // Create a tower at the spawn point position and rotation
        Instantiate(towerPrefab, towerSpawnPoint.position, towerSpawnPoint.rotation);

        // Clear the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null);
    }
}

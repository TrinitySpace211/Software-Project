using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles the NPC dialog UI, function menu, tower spawning, and weapon upgrade.
/// </summary>
public class NPCDialog : MonoBehaviour {

    /// <summary>
    /// The main dialog panel shown when the player interacts with the NPC.
    /// </summary>
    public CanvasGroup dialogPanel;

    /// <summary>
    /// The panel that contains additional NPC functions or options.
    /// </summary>
    public CanvasGroup functionsPanel;

    /// <summary>
    /// CanvasGroup for the tower information panel.
    /// This panel shows details about the tower.
    /// </summary>
    public CanvasGroup towerInfoPanel;

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

        HidePanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

        // Force Unity to update the canvas layout immediately
        Canvas.ForceUpdateCanvases();

        
    }

    /// <summary>
    /// Opens the NPC dialog panel and unlocks the cursor for UI interaction.
    /// </summary>
    public void OpenDialog() {
        IsDialogOpen = true;

        ShowPanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

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

        HidePanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

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
        ShowPanel(dialogPanel);
        HidePanel(functionsPanel);
        HidePanel(towerInfoPanel);

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
        HidePanel(dialogPanel);
        ShowPanel(functionsPanel);
        HidePanel(towerInfoPanel);

        // Clear the currently selected UI element
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OpenTowerInfo() {
        ShowPanel(functionsPanel);
        ShowPanel(towerInfoPanel);
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

    /// <summary>
    /// Makes a panel visible and allows the player to interact with it.
    /// This is used instead of SetActive(true), so the UI does not need to be rebuilt every time.
    /// </summary>
    private void ShowPanel(CanvasGroup panel) {
        // If no panel was assigned in the Inspector, stop the function.
        if (panel == null)
            return;

        // Makes the panel visible.
        panel.alpha = 1f;

        // Allows buttons and other UI elements inside the panel to be clicked.
        panel.interactable = true;

        // Allows the panel to receive mouse / UI raycasts.
        panel.blocksRaycasts = true;
    }

    /// <summary>
    /// Makes a panel invisible and disables interaction with it.
    /// The GameObject stays active, but the player cannot see or click it.
    /// </summary>
    private void HidePanel(CanvasGroup panel) {
        // If no panel was assigned in the Inspector, stop the function.
        if (panel == null)
            return;

        // Makes the panel invisible.
        panel.alpha = 0f;

        // Prevents buttons and other UI elements inside the panel from being clicked.
        panel.interactable = false;

        // Prevents the invisible panel from blocking clicks on other UI elements.
        panel.blocksRaycasts = false;
    }
}



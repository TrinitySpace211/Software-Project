using UnityEngine;

/// <summary>
/// Represents a button that selects a specific game mode.
/// </summary>
public class GameModeButton : MonoBehaviour {
    [SerializeField] private GameModeType mode;
    [SerializeField] private MenuManager menuManager;

    /// <summary>
    /// Notifies the menu manager to start the selected game mode.
    /// </summary>
    public void SelectMode() {
        menuManager.StartGameMode(mode);
    }
}
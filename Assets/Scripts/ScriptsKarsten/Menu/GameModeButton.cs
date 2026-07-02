using UnityEngine;

public class GameModeButton : MonoBehaviour {
    [SerializeField] private GameModeType mode;
    [SerializeField] private MenuManager menuManager;

    public void SelectMode() {
        menuManager.StartGameMode(mode);
    }
}
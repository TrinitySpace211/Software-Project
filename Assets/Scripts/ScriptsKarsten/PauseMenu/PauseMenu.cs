using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    void Update() {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame) {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Resume() {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void LoadMainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettings() {
        Debug.Log("Settings öffnen");
        // Hier später dein Settings-Menü einblenden.
    }

    public void ExitGame() {
        Debug.Log("Spiel beendet.");
        Application.Quit();
    }
}
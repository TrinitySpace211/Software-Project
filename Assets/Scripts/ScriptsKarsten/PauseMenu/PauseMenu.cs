using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour {
    public GameObject pauseMenuUI;
    [SerializeField] private Inventory inventory;
    [SerializeField] private NPCDialog npcDialog;
    [SerializeField] private GameObject crosshair;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

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
        crosshair.SetActive(true);

        StartCoroutine(ResumeRoutine());
    }

    private IEnumerator ResumeRoutine() {
        yield return null;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (Mouse.current != null) {
            var pos = Mouse.current.position.ReadValue();
            Mouse.current.WarpCursorPosition(pos);
            InputSystem.QueueStateEvent(Mouse.current, new MouseState { position = pos });
            InputSystem.Update();
        }
    }

    void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        crosshair.SetActive(false);

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
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
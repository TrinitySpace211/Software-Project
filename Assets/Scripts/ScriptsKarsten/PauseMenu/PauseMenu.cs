using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls pause menu behavior, including pause state, UI visibility,
/// cursor handling, and menu sounds.
/// </summary>
public class PauseMenu : MonoBehaviour {
    /// <summary>
    /// Root object of the pause menu UI.
    /// </summary>
    public GameObject pauseMenuUI;

    [SerializeField] private Inventory inventory;
    [SerializeField] private NPCDialog npcDialog;

    /// <summary>
    /// Gameplay crosshair shown when the game is active.
    /// </summary>
    [SerializeField] private GameObject crosshair;

    /// <summary>
    /// Scrap HUD that is hidden while the game is paused.
    /// </summary>
    [SerializeField] private ScrapHudDisplay scrapHudDisplay;

    [SerializeField] private AmmunitionHudDisplay ammunitionHudDisplay;

    /// <summary>
    /// Button that gets selected when the pause menu opens.
    /// </summary>
    [SerializeField] private GameObject firstSelectedButton;

    /// <summary>
    /// Click sound used for pause menu actions.
    /// </summary>
    [SerializeField] private AudioClip clickSound;

    private AudioSource audioSource;
    private bool isPaused = false;

    /// <summary>
    /// Returns whether the game is currently paused.
    /// </summary>
    public bool IsPaused => isPaused;

    /// <summary>
    /// Sets up the audio source and hides the pause menu at startup.
    /// </summary>
    private void Awake() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    /// <summary>
    /// Sets the initial button selection once the scene is ready.
    /// </summary>
    private void Start() {
        if (firstSelectedButton != null && EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    /// <summary>
    /// Toggles pause with the Escape key.
    /// </summary>
    private void Update() {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame && !OptionsMenu.IsOpen) {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    /// <summary>
    /// Resumes gameplay and restores UI state.
    /// </summary>
    public void Resume() {
        PlayClick();

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        if (scrapHudDisplay != null)
            scrapHudDisplay.SetVisible(true);

        if (ammunitionHudDisplay != null)
            ammunitionHudDisplay.SetVisible(true);

        if (crosshair != null)
            crosshair.SetActive(true);

        ResetUISelection(null);
        StartCoroutine(ResumeRoutine());
    }

    /// <summary>
    /// Restores mouse cursor state after resuming.
    /// </summary>
    private IEnumerator ResumeRoutine() {
        yield return null;

        Cursor.visible = false;

        if (Mouse.current != null) {
            var pos = Mouse.current.position.ReadValue();
            Mouse.current.WarpCursorPosition(pos);
            InputSystem.QueueStateEvent(Mouse.current, new MouseState { position = pos });
            InputSystem.Update();
        }
    }

    /// <summary>
    /// Pauses gameplay and shows the pause menu.
    /// </summary>
    private void Pause() {
        PlayClick();

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        if (scrapHudDisplay != null)
            scrapHudDisplay.SetVisible(false);

        if (ammunitionHudDisplay != null)
            ammunitionHudDisplay.SetVisible(false);

        if (crosshair != null)
            crosshair.SetActive(false);

        Cursor.visible = true;

        ResetUISelection(firstSelectedButton);
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void LoadMainMenu() {
        PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Exits the game or stops play mode in the editor.
    /// </summary>
    public void ExitGame() {
        PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// Plays the configured click sound.
    /// </summary>
    private void PlayClick() {
        if (clickSound == null)
            return;

        audioSource.PlayOneShot(clickSound);
    }

    /// <summary>
    /// Clears the current UI selection and assigns a new one on the next frame.
    /// </summary>
    private void ResetUISelection(GameObject target) {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(SelectNextFrame(target));
    }

    /// <summary>
    /// Selects the requested UI object on the next frame.
    /// </summary>
    private IEnumerator SelectNextFrame(GameObject target) {
        yield return null;

        if (EventSystem.current != null && target != null)
            EventSystem.current.SetSelectedGameObject(target);
    }
}
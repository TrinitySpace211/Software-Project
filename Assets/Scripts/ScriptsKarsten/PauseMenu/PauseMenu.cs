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
    [SerializeField] private ScrapHudDisplay scrapHudDisplay;

    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private AudioClip clickSound;

    private AudioSource audioSource;
    private bool isPaused = false;
    public bool IsPaused => isPaused;

    private void Awake() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    private void Start() {
        if (firstSelectedButton != null && EventSystem.current != null) {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

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
        PlayClick();

        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        if (scrapHudDisplay != null)
            scrapHudDisplay.SetVisible(true);

        if (crosshair != null)
            crosshair.SetActive(true);

        ResetUISelection(null);
        StartCoroutine(ResumeRoutine());
    }

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

    void Pause() {
        PlayClick();

        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        if (scrapHudDisplay != null)
            scrapHudDisplay.SetVisible(false);

        if (crosshair != null)
            crosshair.SetActive(false);

        Cursor.visible = true;

        ResetUISelection(firstSelectedButton);
    }

    public void LoadMainMenu() {
        PlayClick();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void OpenSettings() {
        PlayClick();
        Debug.Log("Settings öffnen");
    }

    public void ExitGame() {
        PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayClick() {
        if (clickSound == null)
            return;

        audioSource.PlayOneShot(clickSound);
    }

    private void ResetUISelection(GameObject target) {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(SelectNextFrame(target));
    }

    private IEnumerator SelectNextFrame(GameObject target) {
        yield return null;

        if (EventSystem.current != null && target != null)
            EventSystem.current.SetSelectedGameObject(target);
    }
}
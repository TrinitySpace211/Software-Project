using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using TMPro;

/// <summary>
/// Handles main menu navigation, settings panels, game mode selection,
/// audio setup, fade transitions, and scene loading.
/// </summary>
public class MenuManager : MonoBehaviour {

    [Header("Scene Names")]
    [SerializeField] private Loader.Scene tutorialScene = Loader.Scene.TutorialScene;
    [SerializeField] private Loader.Scene mainScene = Loader.Scene.MainScene;

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameModePanel;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip buttonSelect;
    [SerializeField] private AudioSource musicSource;

    [Header("Buttons")]
    [SerializeField] private Image continueButtonImage;
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;

    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    [SerializeField] private GameObject audioSection;
    [SerializeField] private GameObject controlsSection;

    [Header("Selection")]
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private GameObject firstGameModeButton;

    private FullScreenMode fullScreenMode;

    private AudioSource audioSource;
    private bool isTransitioning;

    private string savePath;
    private string tutorialPath;
    private bool hasTutorial;

    /// <summary>
    /// Creates audio sources, initializes fade settings, and prepares the menu state.
    /// </summary>
    private void Awake() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.2f;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.clip = musicClip;
        musicSource.loop = true;
        musicSource.volume = 0.2f;
        musicSource.playOnAwake = false;

        if (musicClip != null)
            musicSource.Play();

        // Initialize fade overlay (fully transparent and non-blocking)
        if (fadeGroup != null) {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        Time.timeScale = 1f;
    }

    /// <summary>
    /// Initializes menu panels, save paths, button availability,
    /// and default UI selection.
    /// </summary>
    private void Start() {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (audioSection != null) audioSection.SetActive(false);
        if (controlsSection != null) controlsSection.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        tutorialPath = Path.Combine(Application.persistentDataPath, $"{TutorialManager.ID}.json");
        hasTutorial = File.Exists(tutorialPath);

        if (continueButton != null && continueButtonImage != null && continueButtonText != null) {
            if (!hasTutorial) {
                continueButton.interactable = false;
                continueButtonImage.color = new Color32(100, 100, 100, 255);
                continueButtonText.color = new Color32(100, 100, 100, 255);
            } else {
                continueButton.interactable = true;
                continueButtonImage.color = new Color32(255, 255, 255, 255);
                continueButtonText.color = new Color32(255, 255, 255, 255);
            }
        }

        // Ensure audio and controls starts hidden
        if (audioSection != null)
            audioSection.SetActive(false);

        if (controlsSection != null)
            controlsSection.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

        ApplyVolumeSettings();

        if (musicSource != null && musicClip != null) {
            musicSource.clip = musicClip;
            musicSource.loop = true;
            musicSource.Play();
        }

        ResetUISelection(firstSelectedButton);
    }

    /// <summary>
    /// Opens the game mode selection screen from the main menu.
    /// </summary>
    public void NewGame() {
        if (isTransitioning) return;

        PlayClick();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (audioSection != null) audioSection.SetActive(false);
        if (controlsSection != null) controlsSection.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(true);

        ResetUISelection(firstGameModeButton);
    }

    /// <summary>
    /// Selects a game mode, clears old save data, and starts the tutorial scene.
    /// </summary>
    public void StartGameMode(GameModeType mode) {
        if (isTransitioning) return;

        GameMode.Selected = mode;

        if (gameModePanel != null)
            gameModePanel.SetActive(false);

        if (File.Exists(tutorialPath)) {
            File.Delete(tutorialPath);
        }
        if (File.Exists(savePath)) {
            File.Delete(savePath);
        }

        StartCoroutine(FadeAndLoadScene(tutorialScene));
    }

    /// <summary>
    /// Closes the game mode panel and returns to the main menu.
    /// </summary>
    public void OnCloseGameModeClicked() {
        PlayClick();

        if (gameModePanel != null)
            gameModePanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);
    }

    /// <summary>
    /// Cancels game mode selection and returns to the main menu.
    /// </summary>
    public void CancelGameModeSelection() {
        PlayClick();

        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        ResetUISelection(firstSelectedButton);
    }

    /// <summary>
    /// Opens the settings menu.
    /// </summary>
    public void OnSettingsPressed() {
        PlayClick();

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        ResetUISelection(null);
    }

    /// <summary>
    /// Closes the settings menu and returns to the main menu.
    /// </summary>
    public void OnCloseSettingsPressed() {
        PlayClick();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        ResetUISelection(firstSelectedButton);
    }

    /// <summary>
    /// Opens the audio settings section.
    /// </summary>
    public void OnSoundPressed() {
        PlayClick();

        if (audioSection != null) audioSection.SetActive(true);
        if (controlsSection != null) controlsSection.SetActive(false);
    }

    /// <summary>
    /// Opens the controls settings section.
    /// </summary>
    public void OnControlsPressed() {
        PlayClick();

        if (controlsSection != null) controlsSection.SetActive(true);
        if (audioSection != null) audioSection.SetActive(false);
    }

    /// <summary>
    /// Toggles fullscreen mode.
    /// </summary>
    public void ToggleFullscreen() {
        PlayClick();

        Screen.fullScreen = !Screen.fullScreen;
    }

    /// <summary>
    /// Switches between windowed and fullscreen window mode.
    /// </summary>
    public void SetWindowed() {
        PlayClick();
        if (fullScreenMode == FullScreenMode.FullScreenWindow) {
            fullScreenMode = FullScreenMode.Windowed;
            Screen.SetResolution(1280, 720, fullScreenMode);
        } else if (fullScreenMode == FullScreenMode.Windowed) {
            Resolution res = Screen.currentResolution;

            fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.SetResolution(res.width, res.height, fullScreenMode);
        }
    }

    /// <summary>
    /// Exits the application or stops play mode in the Unity Editor.
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
    /// Fades the screen and loads the given scene.
    /// </summary>
    private IEnumerator FadeAndLoadScene(Loader.Scene scene) {
        isTransitioning = true;
        PlayClick();

        if (fadeGroup != null) {
            fadeGroup.blocksRaycasts = true;
            yield return Fade(0f, 1f);
        }

        yield return new WaitForSeconds(0.1f);
        ResetUISelection(firstSelectedButton);
        Loader.Load(scene);
    }

    /// <summary>
    /// Fades the overlay canvas group between two alpha values.
    /// </summary>
    private IEnumerator Fade(float from, float to) {
        float time = 0f;

        while (time < fadeDuration) {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / fadeDuration);
            fadeGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }

        fadeGroup.alpha = to;
    }

    /// <summary>
    /// Continues the game by loading the appropriate scene based on save data.
    /// </summary>
    public void ContinueGame() {
        if (hasTutorial) {
            TutorialManager.TutorialData tutorialData = SaveManager.Instance.LoadData<TutorialManager.TutorialData>(TutorialManager.ID);

            bool tutorialFinished = tutorialData.tutorialFinished;
            if (tutorialFinished) {
                StartCoroutine(FadeAndLoadScene(mainScene));
            } else {
                StartCoroutine(FadeAndLoadScene(tutorialScene));
            }
        }
    }

    /// <summary>
    /// Plays the click sound if one is assigned.
    /// </summary>
    private void PlayClick() {
        ApplyVolumeSettings();

        if (clickSound == null) return;
        audioSource.PlayOneShot(clickSound);
    }

    /// <summary>
    /// Applies global master, music, and SFX volume values.
    /// </summary>
    public void ApplyVolumeSettings() {
        float master = OptionsMenu.MasterVolume;
        float music = OptionsMenu.MusicVolume;
        float sfx = OptionsMenu.SfxVolume;

        AudioListener.volume = master;

        if (musicSource != null)
            musicSource.volume = music * master;

        if (audioSource != null)
            audioSource.volume = sfx * master;
    }

    /// <summary>
    /// Resets the current UI selection and schedules a new target for the next frame.
    /// </summary>
    private void ResetUISelection(GameObject target) {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(SelectNextFrame(target));
    }

    /// <summary>
    /// Selects the given UI object on the next frame.
    /// </summary>
    private IEnumerator SelectNextFrame(GameObject target) {
        yield return null;

        if (EventSystem.current != null && target != null)
            EventSystem.current.SetSelectedGameObject(target);
    }
}
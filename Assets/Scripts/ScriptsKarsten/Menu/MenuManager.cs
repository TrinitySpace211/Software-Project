using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuManager : MonoBehaviour {
    [Header("Scene")]
    [SerializeField] private string gameSceneName = "TutorialScene";

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject gameModePanel;
    [SerializeField] private GameObject audioSection;
    [SerializeField] private GameObject controlsSection;

    [Header("Selection")]
    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private GameObject firstGameModeButton;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip buttonSelect;
    [SerializeField] private AudioSource musicSource;

    private AudioSource audioSource;
    private bool isTransitioning;

    private void Awake() {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (fadeGroup != null) {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }
    }

    private void Start() {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (audioSection != null) audioSection.SetActive(false);
        if (controlsSection != null) controlsSection.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

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

    public void StartGameMode(GameModeType mode) {
        if (isTransitioning) return;

        GameMode.Selected = mode;

        if (gameModePanel != null)
            gameModePanel.SetActive(false);

        StartCoroutine(FadeAndLoadScene());
    }

    public void OnCloseGameModeClicked() {
        PlayClick();

        if (gameModePanel != null)
            gameModePanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        ResetUISelection(firstSelectedButton);
    }

    public void CancelGameModeSelection() {
        PlayClick();

        if (gameModePanel != null) gameModePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);

        ResetUISelection(firstSelectedButton);
    }

    public void OnSettingsPressed() {
        PlayClick();

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        ResetUISelection(null);
    }

    public void OnCloseSettingsPressed() {
        PlayClick();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        ResetUISelection(firstSelectedButton);
    }

    public void OnSoundPressed() {
        PlayClick();

        if (audioSection != null) audioSection.SetActive(true);
        if (controlsSection != null) controlsSection.SetActive(false);
    }

    public void OnControlsPressed() {
        PlayClick();

        if (controlsSection != null) controlsSection.SetActive(true);
        if (audioSection != null) audioSection.SetActive(false);
    }

    public void SetFullscreen() {
        PlayClick();
        Resolution res = Screen.currentResolution;
        Screen.SetResolution(res.width, res.height, FullScreenMode.FullScreenWindow);
    }

    public void SetWindowed() {
        PlayClick();
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
    }

    public void ContinueGame() {
        PlayClick();
    }

    public void ExitGame() {
        PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator FadeAndLoadScene() {
        isTransitioning = true;
        PlayClick();

        if (fadeGroup != null) {
            fadeGroup.blocksRaycasts = true;
            yield return Fade(0f, 1f);
        }

        yield return new WaitForSeconds(0.1f);
        SceneManager.LoadScene(gameSceneName);
    }

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

    private void PlayClick() {
        ApplyVolumeSettings();

        if (clickSound == null) return;
        audioSource.PlayOneShot(clickSound);
    }

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
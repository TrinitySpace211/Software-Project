using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using TMPro;

/// <summary>
/// Manages the main menu logic including scene loading, settings UI,
/// audio playback, UI selection handling, and fade transitions.
/// </summary>
public class MenuManager : MonoBehaviour
{

    [Header("Scene Names")]
    [SerializeField] private Loader.Scene tutorialScene = Loader.Scene.TutorialScene;
    [SerializeField] private Loader.Scene mainScene = Loader.Scene.MainScene;

    [SerializeField] private GameObject settingsPanel;

    [Header("Audio")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip musicClip;
    [SerializeField] private AudioClip buttonSelect;

    [Header("Buttons")]
    [SerializeField] private Image continueButtonImage;
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;

    [SerializeField] private GameObject firstSelectedButton;
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1f;

    [SerializeField] private GameObject audioSection;
    [SerializeField] private GameObject controlsSection;

    [SerializeField] private AudioSource musicSource;

    private AudioSource audioSource;
    private bool isTransitioning;

    //Save Paths
    private string savePath;
    private string tutorialPath;
    private bool hasTutorial = false;

    /// <summary>
    /// Initializes audio sources, starts background music,
    /// and prepares the fade overlay at startup.
    /// </summary>
    private void Awake()
    {
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
        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
        }

        Time.timeScale = 1f;
    }

    /// <summary>
    /// Sets up the initial UI state when the menu scene starts,
    /// including selected button and hidden settings panel.
    /// </summary>
    private void Start()
    {
        // Ensure settings panel starts hidden
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
        tutorialPath = Path.Combine(Application.persistentDataPath, $"{TutorialManager.ID}.json");
        hasTutorial = File.Exists(tutorialPath);

        if (continueButton != null && continueButtonImage != null && continueButtonText != null)
        {
            if (!hasTutorial)
            {
                continueButton.interactable = false;
                continueButtonImage.color = new Color32(100, 100, 100, 255);
                continueButtonText.color = new Color32(100, 100, 100, 255);
            }
            else
            {
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
    }


    /// <summary>
    /// Starts a new game by playing a transition effect and loading the game scene.
    /// </summary>
    public void NewGame()
    {
        if (isTransitioning)
            return;

        File.Delete(tutorialPath);
        File.Delete(savePath);

        StartCoroutine(FadeAndLoadScene(tutorialScene));
    }

    /// <summary>
    /// Handles fade-out animation and loads the target game scene.
    /// </summary>
    private IEnumerator FadeAndLoadScene(Loader.Scene scene)
    {
        isTransitioning = true;
        PlayClick();

        // Fade screen to black before loading
        if (fadeGroup != null)
        {
            fadeGroup.blocksRaycasts = true;
            yield return Fade(0f, 1f);
        }

        // Small delay to ensure fade completes cleanly
        yield return new WaitForSeconds(0.1f);

        Loader.Load(scene);
    }

    /// <summary>
    /// Smoothly interpolates the screen fade between two alpha values.
    /// </summary>
    /// <param name="from">Starting alpha value.</param>
    /// <param name="to">Target alpha value.</param>
    private IEnumerator Fade(float from, float to)
    {
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / fadeDuration);
            fadeGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        fadeGroup.alpha = to;
    }

    /// <summary>
    /// Opens the settings menu and updates UI selection.
    /// </summary>
    public void OnSettingsPressed()
    {
        PlayClick();

        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        ResetUISelection(null);
    }

    /// <summary>
    /// Closes the settings menu and restores main menu selection.
    /// </summary>
    public void OnCloseSettingsPressed()
    {
        PlayClick();

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        ResetUISelection(firstSelectedButton);
    }

    /// <summary>
    /// Exits the application or stops play mode if running inside the Unity Editor.
    /// </summary>
    public void ExitGame()
    {
        PlayClick();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void ContinueGame()
    {
        if (hasTutorial)
        {
            TutorialManager.TutorialData tutorialData = SaveManager.Instance.LoadData<TutorialManager.TutorialData>(TutorialManager.ID);

            bool tutorialFinished = tutorialData.tutorialFinished;
            if (tutorialFinished)
            {
                StartCoroutine(FadeAndLoadScene(mainScene));
            }
            else
            {
                StartCoroutine(FadeAndLoadScene(tutorialScene));
            }
        }
    }

    /// <summary>
    /// Plays a UI click sound effect if one is assigned.
    /// </summary>
    private void PlayClick()
    {
        ApplyVolumeSettings();

        if (clickSound == null)
            return;

        audioSource.PlayOneShot(clickSound);
    }

    /// <summary>
    /// Clears current UI selection and schedules a new selection on the next frame.
    /// This avoids EventSystem selection bugs.
    /// </summary>
    /// <param name="target">UI element to select after reset.</param>
    private void ResetUISelection(GameObject target)
    {
        if (EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(SelectNextFrame(target));
    }

    /// <summary>
    /// Selects a UI element after one frame delay to ensure proper EventSystem update.
    /// </summary>
    /// <param name="target">UI element to select.</param>
    private IEnumerator SelectNextFrame(GameObject target)
    {
        yield return null;

        if (EventSystem.current != null && target != null)
        {
            EventSystem.current.SetSelectedGameObject(target);
        }
    }

    /// <summary>
    /// Applies all saved audio settings to the audio sources.
    /// </summary>
    public void ApplyVolumeSettings()
    {
        float master = OptionsMenu.MasterVolume;
        float music = OptionsMenu.MusicVolume;
        float sfx = OptionsMenu.SfxVolume;

        AudioListener.volume = master;

        if (musicSource != null)
            musicSource.volume = music * master;

        if (audioSource != null)
            audioSource.volume = sfx * master;
    }

    public void OnSoundPressed()
    {
        PlayClick();

        if (audioSection != null)
            audioSection.SetActive(true);

        if (controlsSection != null)
            controlsSection.SetActive(false);
    }

    public void OnControlsPressed()
    {
        PlayClick();

        if (controlsSection != null)
            controlsSection.SetActive(true);

        if (audioSection != null)
            audioSection.SetActive(false);
    }
}
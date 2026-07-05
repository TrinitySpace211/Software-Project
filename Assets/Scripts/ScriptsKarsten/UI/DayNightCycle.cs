using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using UnityEngine.InputSystem;

/// <summary>
/// Controls the full day-night cycle system including:
/// - Time progression
/// - UI display
/// - Notifications
/// - Dynamic lighting (sun, color, intensity, ambient light)
/// - Sunrise and sunset transitions
/// - Extraction after surviving a defined number of nights
/// </summary>
public class DayNightCycle : MonoBehaviour, ISaveable {

    public static readonly string ID = "DayNightCycle";

    public AchievementSO achievementDay1;
    public AchievementSO achievementDay5;
    private bool achievement1Gained = false;
    private bool achievement2Gained = false;

    /// <summary>
    /// UI text showing the current time and current time state.
    /// </summary>
    public TMP_Text timeText;

    /// <summary>
    /// UI text used for transition notifications.
    /// </summary>
    public TMP_Text notificationText;

    /// <summary>
    /// UI text used to display the save confirmation message.
    /// </summary>
    public TextMeshProUGUI gameSavedUIText;

    /// <summary>
    /// Speed at which in-game time advances per real second.
    /// </summary>
    public float hoursPerRealSecond = 0.1f;

    /// <summary>
    /// Current in-game time of day.
    /// </summary>
    private float timeOfDay = 6f;

    /// <summary>
    /// Tracks the current day and night progression.
    /// </summary>
    private int dayNumber = 1;
    private int nightNumber = 0;

    /// <summary>
    /// Counts how many nights the player has survived.
    /// </summary>
    private int survivedNights = 0;

    /// <summary>
    /// Gets the number of survived nights.
    /// </summary>
    public int SurvivedNights => survivedNights;

    /// <summary>
    /// Event fired when a new day begins.
    /// </summary>
    [Header("Wave Events")]
    public UnityEvent onNewDayStarted;

    /// <summary>
    /// Event fired when a new night begins.
    /// </summary>
    public UnityEvent onNightStarted;

    /// <summary>
    /// Possible states of the current time cycle.
    /// </summary>
    private enum TimeState {
        Night,
        Sunrise,
        Day,
        Sunset
    }

    private TimeState currentState;

    /// <summary>
    /// Directional light used as the sun or moon.
    /// </summary>
    public Light sunLight;

    /// <summary>
    /// Maximum light intensity during daytime.
    /// </summary>
    public float dayLightIntensity = 1.2f;

    /// <summary>
    /// Minimum light intensity during nighttime.
    /// </summary>
    public float nightLightIntensity = 0.5f;

    /// <summary>
    /// Light color used during the day.
    /// </summary>
    public Color dayColor = Color.white;

    /// <summary>
    /// Light color used during the night.
    /// </summary>
    public Color nightColor = new Color(0.45f, 0.45f, 0.55f);

    /// <summary>
    /// Audio clip played during the day.
    /// </summary>
    public AudioClip dayMusic;

    /// <summary>
    /// Audio clip played during the night.
    /// </summary>
    public AudioClip nightMusic;

    /// <summary>
    /// Volume applied to the day music.
    /// </summary>
    public float dayMusicVolume;

    /// <summary>
    /// Volume applied to the night music.
    /// </summary>
    public float nightMusicVolume;

    /// <summary>
    /// Player object reference used during extraction handling.
    /// </summary>
    public GameObject playerObject;

    /// <summary>
    /// Reference to the extraction controller.
    /// </summary>
    public ExtractionController extractionController;

    /// <summary>
    /// Night on which extraction should begin.
    /// </summary>
    public int extractionNight = 10;

    /// <summary>
    /// Scene loaded after the extraction sequence.
    /// </summary>
    public Loader.Scene extractionScene = Loader.Scene.ExtractionScene;

    /// <summary>
    /// Delay before the extraction scene starts loading.
    /// </summary>
    public float extractionSceneDelay = 3f;

    /// <summary>
    /// Canvas group used for the fade-to-black transition.
    /// </summary>
    public CanvasGroup fadeCanvasGroup;

    /// <summary>
    /// Duration of the fade-to-black transition.
    /// </summary>
    public float fadeDuration = 1.5f;

    /// <summary>
    /// Prevents the extraction sequence from being started more than once.
    /// </summary>
    private bool extractionTriggered;

    /// <summary>
    /// Cached reference to the Player component.
    /// </summary>
    private Player playerScript;

    /// <summary>
    /// Audio source used for day and night music.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// Event fired when sunset begins.
    /// </summary>
    public static event Action OnSunsetStarted;

    /// <summary>
    /// Event fired when sunrise begins.
    /// </summary>
    public static event Action OnSunriseStarted;

    /// <summary>
    /// Initializes the system and hides notification UI elements.
    /// </summary>
    void Start() {
        if (gameSavedUIText != null)
            gameSavedUIText.gameObject.SetActive(false);

        if (notificationText != null) {
            notificationText.gameObject.SetActive(false);
        }
        if (fadeCanvasGroup != null) {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }
        if (playerObject != null) {
            playerScript = playerObject.GetComponent<Player>();
        }
        audioSource = GetComponent<AudioSource>();

        currentState = GetTimeState();

        if (audioSource != null && currentState == TimeState.Day) {
            audioSource.clip = dayMusic;
            audioSource.volume = dayMusicVolume * SoundManager.Instance.volume;
            audioSource.Play();
        }

        ShowNotification("Protect the Fuel Tank. Hold out until Night 10.");

        if (SaveManager.Instance != null)
            SaveManager.Instance.LoadGame();
    }

    /// <summary>
    /// Main update loop that advances time and updates state, UI, and lighting.
    /// </summary>
    void Update() {
        if (extractionTriggered)
            return;

        UpdateTime();
        HandleState();
        UpdateUI();
        UpdateLighting();
    }

    /// <summary>
    /// Stops background music when the player dies.
    /// </summary>
    /// <param name="position">World position where the player died.</param>
    private void PlayerHealth_OnDeath(Vector3 position) {
        audioSource.Stop();
    }

    /// <summary>
    /// Advances the in-game time.
    /// </summary>
    private void UpdateTime() {
        timeOfDay += Time.deltaTime * hoursPerRealSecond;
        timeOfDay %= 24f;
    }

    /// <summary>
    /// Detects state changes and triggers the matching transition behavior.
    /// </summary>
    private void HandleState() {
        TimeState newState = GetTimeState();

        if (newState != currentState) {
            currentState = newState;

            switch (currentState) {
                case TimeState.Sunrise:
                    OnSunriseStarted?.Invoke();
                    ShowNotification("Sunrise begins...");

                    if (audioSource != null) {
                        StartCoroutine(FadeInOutMusic(audioSource, dayMusic, dayMusicVolume, 2f));
                    }
                    break;

                case TimeState.Day:
                    // Count a night as survived once the player reaches the next day.
                    CheckAchievement();

                    if (nightNumber > 0) {
                        survivedNights++;

                        if (SaveManager.Instance != null) {
                            SaveManager.Instance.SaveGame();
                            StartCoroutine(ShowSavedGame());
                        }
                    }

                    ShowNotification($"Day {dayNumber} begins!");
                    onNewDayStarted?.Invoke();

                    if (dayNumber > extractionNight) {
                        TriggerExtraction();
                    }

                    break;

                case TimeState.Sunset:
                    OnSunsetStarted?.Invoke();
                    ShowNotification("Sunset begins...");
                    break;

                case TimeState.Night:
                    ShowNotification($"Night {nightNumber + 1} begins!");
                    onNightStarted?.Invoke();

                    dayNumber++;
                    nightNumber++;

                    if (audioSource != null) {
                        StartCoroutine(FadeInOutMusic(audioSource, nightMusic, nightMusicVolume, 2f));
                    }

                    break;
            }
        }
    }

    /// <summary>
    /// Fades out the current music and starts the next track with a fade-in.
    /// </summary>
    private IEnumerator FadeInOutMusic(AudioSource audioSource, AudioClip musicClip, float targetVolume, float duration) {
        float startVolume = audioSource.volume;
        while (audioSource.volume > 0) {
            audioSource.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;

        audioSource.volume = 0f;
        audioSource.clip = musicClip;
        audioSource.Play();

        while (audioSource.volume < targetVolume) {
            audioSource.volume += targetVolume * Time.deltaTime / duration;
            yield return null;
        }

        audioSource.volume = targetVolume;
    }

    /// <summary>
    /// Shows and then hides the saved-game confirmation text.
    /// </summary>
    private IEnumerator ShowSavedGame() {
        if (gameSavedUIText == null)
            yield break;

        gameSavedUIText.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        gameSavedUIText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Checks whether an achievement should be awarded for the current day.
    /// </summary>
    private void CheckAchievement() {
        if (dayNumber == 2 && !achievement1Gained) {
            AchievementManager.triggerAchievement?.Invoke(achievementDay1);
            achievement1Gained = true;
        } else if (dayNumber == 6 && !achievement2Gained) {
            AchievementManager.triggerAchievement?.Invoke(achievementDay5);
            achievement2Gained = true;
        }
    }

    /// <summary>
    /// Starts the extraction sequence and prevents it from running again.
    /// </summary>
    private void TriggerExtraction() {
        if (extractionTriggered)
            return;

        extractionTriggered = true;

        if (playerScript != null) {
            playerScript.enabled = false;
        }

        if (extractionController != null) {
            extractionController.StartExtraction();
        }

        StartCoroutine(LoadExtractionSceneRoutine());
    }

    /// <summary>
    /// Waits for the configured delay, fades to black, and loads the extraction scene.
    /// </summary>
    private IEnumerator LoadExtractionSceneRoutine() {
        yield return new WaitForSeconds(extractionSceneDelay);
        yield return StartCoroutine(FadeToBlackRoutine());
        Loader.Load(extractionScene);
    }

    /// <summary>
    /// Fades the screen to black using the configured canvas group.
    /// </summary>
    private IEnumerator FadeToBlackRoutine() {
        if (fadeCanvasGroup == null)
            yield break;

        fadeCanvasGroup.interactable = true;
        fadeCanvasGroup.blocksRaycasts = true;

        float elapsed = 0f;
        float startAlpha = fadeCanvasGroup.alpha;

        while (elapsed < fadeDuration) {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Updates the on-screen time and day/night display.
    /// </summary>
    private void UpdateUI() {
        int hours = Mathf.FloorToInt(timeOfDay);
        int minutes = Mathf.FloorToInt((timeOfDay - hours) * 60f);

        string stateText = "";

        if (currentState == TimeState.Day)
            stateText = $"Day {dayNumber}";
        else if (currentState == TimeState.Night)
            stateText = $"Night {nightNumber}";
        else
            stateText = $"Day {dayNumber}";

        if (timeText != null) {
            timeText.text = $"{stateText}\n{hours:00}:{minutes:00}";
        }
    }

    /// <summary>
    /// Displays a temporary notification message.
    /// </summary>
    private void ShowNotification(string message) {
        StartCoroutine(NotificationRoutine(message));
    }

    /// <summary>
    /// Shows a notification for a short amount of time, then hides it again.
    /// </summary>
    private IEnumerator NotificationRoutine(string message) {
        if (notificationText == null)
            yield break;

        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(6f);

        notificationText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Determines the current time state from the current in-game time.
    /// </summary>
    /// <returns>The current time state.</returns>
    private TimeState GetTimeState() {
        if (timeOfDay >= 5f && timeOfDay < 6f)
            return TimeState.Sunrise;

        if (timeOfDay >= 6f && timeOfDay < 20f)
            return TimeState.Day;

        if (timeOfDay >= 20f && timeOfDay < 22f)
            return TimeState.Sunset;

        return TimeState.Night;
    }

    /// <summary>
    /// Updates sun rotation, light intensity, light color, and ambient lighting.
    /// </summary>
    private void UpdateLighting() {
        if (sunLight == null)
            return;

        float normalizedTime = timeOfDay / 24f;

        sunLight.transform.rotation = Quaternion.Euler(
            normalizedTime * 360f - 90f,
            170f,
            0f
        );

        float t = 0f;

        switch (currentState) {
            case TimeState.Sunrise:
                t = Mathf.InverseLerp(5f, 6f, timeOfDay);
                break;

            case TimeState.Day:
                t = 1f;
                break;

            case TimeState.Sunset:
                t = Mathf.InverseLerp(22f, 20f, timeOfDay);
                break;

            case TimeState.Night:
                t = 0f;
                break;
        }

        sunLight.intensity = Mathf.Lerp(
            nightLightIntensity,
            dayLightIntensity,
            t
        );

        sunLight.color = Color.Lerp(
            nightColor,
            dayColor,
            t
        );

        RenderSettings.ambientIntensity = Mathf.Lerp(
            0.4f,
            0.8f,
            t
        );
    }

    /// <summary>
    /// Returns the current in-game time.
    /// </summary>
    public float TimeOfDay => timeOfDay;

    /// <summary>
    /// Sets the current time manually for debugging or testing.
    /// </summary>
    public void SetTime(float time) {
        timeOfDay = time;
    }

    #region Save/Load
    /// <summary>
    /// Returns the unique save ID for this object.
    /// </summary>
    public string GetSaveID() => ID;

    /// <summary>
    /// Serializes the current day-night state.
    /// </summary>
    public object Save() {
        return new DayNightData {
            survivedNights = survivedNights,
            achievement1Gained = achievement1Gained,
            achievement2Gained = achievement2Gained
        };
    }

    /// <summary>
    /// Restores the day-night state from saved data.
    /// </summary>
    public void Load(object data) {
        DayNightData dayNightData = (DayNightData)data;
        survivedNights = dayNightData.survivedNights;
        dayNumber = 1 + survivedNights;
        nightNumber = survivedNights;
        achievement1Gained = dayNightData.achievement1Gained;
        achievement2Gained = dayNightData.achievement2Gained;
        timeOfDay = 6;
        //Debug.Log("Loaded Night: " + survivedNights);
    }

    /// <summary>
    /// Serializable save data for the day-night cycle.
    /// </summary>
    [Serializable]
    public class DayNightData {
        public int survivedNights;
        public bool achievement1Gained;
        public bool achievement2Gained;
    }
    #endregion

    /// <summary>
    /// Registers event listeners and save handling when enabled.
    /// </summary>
    private void OnEnable() {
        PlayerHealth.OnDeath += PlayerHealth_OnDeath;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    /// <summary>
    /// Unregisters event listeners and save handling when disabled.
    /// </summary>
    private void OnDisable() {
        PlayerHealth.OnDeath -= PlayerHealth_OnDeath;

        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }
}
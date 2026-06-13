using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// Controls the full day-night cycle system including:
/// - Time progression
/// - UI display
/// - Notifications
/// - Dynamic lighting (sun, color, intensity, ambient light)
/// - Sunrise and sunset transitions
/// - Extraction after surviving a defined number of nights
/// </summary>
public class DayNightCycle : MonoBehaviour {
    /// <summary>
    /// UI text showing current time and state.
    /// </summary>
    public TMP_Text timeText;

    /// <summary>
    /// UI text for transition notifications.
    /// </summary>
    public TMP_Text notificationText;

    /// <summary>
    /// Speed of time progression (hours per real second).
    /// </summary>
    public float hoursPerRealSecond = 0.1f;

    /// <summary>
    /// Current in-game time (0�24 hours).
    /// </summary>
    private float timeOfDay = 6f;

    /// <summary>
    /// Day and Night cycle counter.
    /// </summary>
    private int dayNumber = 1;
    private int nightNumber = 0;

    /// <summary>
    /// Number of nights the player has successfully survived.
    /// </summary>
    private int survivedNights = 0;

    /// <summary>
    /// Returns the amount of survived nights.
    /// </summary>
    public int SurvivedNights => survivedNights;

    /// <summary>
    /// Event fired when a new day begins.
    /// Connect to WaveManager.OnNewDay() via the Inspector.
    /// </summary>
    [Header("Wave Events")]
    public UnityEvent onNewDayStarted;

    /// <summary>
    /// Event fired when a new night begins.
    /// Connect to your objective attack logic via the Inspector.
    /// </summary>
    public UnityEvent onNightStarted;

    /// <summary>
    /// Current time state.
    /// </summary>
    private enum TimeState {
        Night,
        Sunrise,
        Day,
        Sunset
    }

    private TimeState currentState;

    /// <summary>
    /// Directional light acting as sun/moon.
    /// </summary>
    public Light sunLight;

    /// <summary>
    /// Maximum light intensity during day.
    /// </summary>
    public float dayLightIntensity = 1.2f;

    /// <summary>
    /// Minimum light intensity during night.
    /// </summary>
    public float nightLightIntensity = 0.5f;

    /// <summary>
    /// Color during daytime.
    /// </summary>
    public Color dayColor = Color.white;

    /// <summary>
    /// Color during nighttime.
    /// </summary>
    public Color nightColor = new Color(0.45f, 0.45f, 0.55f);

    /// <summary>
    /// Player object reference for disabling controls during extraction.
    /// </summary>
    public GameObject playerObject;

    /// <summary>
    /// Extraction controller reference that starts the cutscene.
    /// </summary>
    public ExtractionController extractionController;

    /// <summary>
    /// Night number on which extraction should start.
    /// </summary>
    public int extractionNight = 10;

    /// <summary>
    /// Name of the scene that will be loaded after the extraction sequence.
    /// </summary>
    public string extractionSceneName = "ExtractionScene";

    /// <summary>
    /// Delay before switching to the extraction scene.
    /// </summary>
    public float extractionSceneDelay = 3f;

    /// <summary>
    /// Fade screen used for the black transition.
    /// </summary>
    public CanvasGroup fadeCanvasGroup;

    /// <summary>
    /// Duration of the black fade before loading the extraction scene.
    /// </summary>
    public float fadeDuration = 1.5f;

    /// <summary>
    /// Prevents the extraction from starting multiple times.
    /// </summary>
    private bool extractionTriggered;

    /// <summary>
    /// Cached Player script reference.
    /// </summary>
    private Player playerScript;

    /// <summary>
    /// Initializes system and hides notification UI.
    /// </summary>
    void Start() {
        if (notificationText != null) {
            notificationText.gameObject.SetActive(false);
        }

        if (fadeCanvasGroup != null) {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.interactable = false;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        currentState = GetTimeState();

        if (playerObject != null) {
            playerScript = playerObject.GetComponent<Player>();
        }
        ShowNotification("Protect the Fuel Tank. Hold out until Night 10.");
    }

    /// <summary>
    /// Main update loop.
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
    /// Advances in-game time.
    /// </summary>
    private void UpdateTime() {
        timeOfDay += Time.deltaTime * hoursPerRealSecond;
        timeOfDay %= 24f;
    }

    /// <summary>
    /// Determines current time state and handles transitions.
    /// </summary>
    private void HandleState() {
        TimeState newState = GetTimeState();

        if (newState != currentState) {
            currentState = newState;

            switch (currentState) {
                case TimeState.Sunrise:
                    ShowNotification("Sunrise begins...");
                    break;

                case TimeState.Day:
                    // Count a night as survived once the player reaches the next day.
                    if (nightNumber > 0) {
                        survivedNights++;
                    }

                    ShowNotification($"Day {dayNumber} begins!");
                    onNewDayStarted?.Invoke();

                    if (dayNumber > extractionNight) {
                        TriggerExtraction();
                    }

                    break;

                case TimeState.Sunset:
                    ShowNotification("Sunset begins...");
                    break;

                case TimeState.Night:
                    ShowNotification($"Night {nightNumber + 1} begins!");
                    onNightStarted?.Invoke();

                    dayNumber++;
                    nightNumber++;

                    break;
            }
        }
    }

    /// <summary>
    /// Starts the extraction sequence, disables player controls, fades the screen to black, and loads the extraction scene.
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
    /// Loads the extraction scene after the configured delay and fade.
    /// </summary>
    private IEnumerator LoadExtractionSceneRoutine() {
        yield return new WaitForSeconds(extractionSceneDelay);
        yield return StartCoroutine(FadeToBlackRoutine());
        SceneManager.LoadScene(extractionSceneName);
    }

    /// <summary>
    /// Fades the screen to black using the assigned CanvasGroup.
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
    /// Updates UI clock display.
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
    /// Shows a temporary notification.
    /// </summary>
    private void ShowNotification(string message) {
        StartCoroutine(NotificationRoutine(message));
    }

    /// <summary>
    /// Notification coroutine.
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
    /// Determines the current time state based on the in-game time.
    /// The day is divided into four phases:
    /// - Sunrise (05:00�06:00)
    /// - Day (06:00�20:00)
    /// - Sunset (20:00�22:00)
    /// - Night (22:00�05:00)
    /// </summary>
    /// <returns>
    /// The current TimeState from enum based on timeOfDay.
    /// </returns>
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
    /// Updates sun rotation, intensity, color and ambient lighting.
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
            0.5f,
            1f,
            t
        );
    }

    /// <summary>
    /// Returns current time (read-only).
    /// </summary>
    public float TimeOfDay => timeOfDay;

    /// <summary>
    /// Sets time manually (debug/testing).
    /// </summary>
    public void SetTime(float time) {
        timeOfDay = time;
    }
}
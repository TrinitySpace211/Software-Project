using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
///     Controls the full day-night cycle system including:
///     - Time progression
///     - UI display
///     - Notifications
///     - Dynamic lighting (sun, color, intensity, ambient light)
///     - Sunrise and sunset transitions
/// </summary>
public class DayNightCycle : MonoBehaviour {
    /// <summary>
    ///     UI text showing current time and state.
    /// </summary>
    public TMP_Text timeText;

    /// <summary>
    ///     UI text for transition notifications.
    /// </summary>
    public TMP_Text notificationText;

    /// <summary>
    ///     Speed of time progression (hours per real second).
    /// </summary>
    public float hoursPerRealSecond = 0.1f;

    /// <summary>
    ///     Directional light acting as sun/moon.
    /// </summary>
    public Light sunLight;

    /// <summary>
    ///     Maximum light intensity during day.
    /// </summary>
    public float dayLightIntensity = 1.2f;

    /// <summary>
    ///     Minimum light intensity during night.
    /// </summary>
    public float nightLightIntensity = 0.5f;

    /// <summary>
    ///     Color during daytime.
    /// </summary>
    public Color dayColor = Color.white;

    /// <summary>
    ///     Color during nighttime.
    /// </summary>
    public Color nightColor = new(0.45f, 0.45f, 0.55f);

    /// <summary>
    ///     Event fired when a new day begins.
    ///     Connect to WaveManager.OnNewDay() via the Inspector.
    /// </summary>
    [Header("Wave Events")] public UnityEvent onNewDayStarted;

    private TimeState currentState;

    /// <summary>
    ///     Day cycle counter.
    /// </summary>
    private int cycleNumber = 1;

    /// <summary>
    ///     Current in-game time (0�24 hours).
    /// </summary>
    private float timeOfDay = 6f;

    /// <summary>
    ///     Returns current time (read-only).
    /// </summary>
    public float TimeOfDay => timeOfDay;

    /// <summary>
    ///     Initializes system and hides notification UI.
    /// </summary>
    private void Start() {
        notificationText.gameObject.SetActive(false);
        currentState = GetTimeState();
    }

    /// <summary>
    ///     Main update loop.
    /// </summary>
    private void Update() {
        UpdateTime();
        HandleState();
        UpdateUI();
        UpdateLighting();
    }

    /// <summary>
    ///     Advances in-game time.
    /// </summary>
    private void UpdateTime() {
        timeOfDay += Time.deltaTime * hoursPerRealSecond;
        timeOfDay %= 24f;
    }

    /// <summary>
    ///     Determines current time state and handles transitions.
    /// </summary>
    private void HandleState() {
        var newState = GetTimeState();

        if (newState != currentState) {
            currentState = newState;

            switch (currentState) {
                case TimeState.Sunrise:
                    ShowNotification("Sunrise begins...");
                    break;

                case TimeState.Day:
                    cycleNumber = Mathf.Max(1, cycleNumber + 1);
                    ShowNotification($"Day {cycleNumber} begins!");
                    onNewDayStarted?.Invoke();
                    break;

                case TimeState.Sunset:
                    ShowNotification("Sunset begins...");
                    break;

                case TimeState.Night:
                    ShowNotification($"Night {cycleNumber} begins!");
                    break;
            }
        }
    }

    /// <summary>
    ///     Updates UI clock display.
    /// </summary>
    private void UpdateUI() {
        var hours = Mathf.FloorToInt(timeOfDay);
        var minutes = Mathf.FloorToInt((timeOfDay - hours) * 60f);

        var stateText = "";

        if (currentState == TimeState.Day)
            stateText = $"Day {cycleNumber}";
        else if (currentState == TimeState.Night)
            stateText = $"Night {cycleNumber}";
        else
            stateText = $"Day {cycleNumber}";

        timeText.text = $"{stateText}\n{hours:00}:{minutes:00}";
    }

    /// <summary>
    ///     Shows a temporary notification.
    /// </summary>
    private void ShowNotification(string message) {
        StartCoroutine(NotificationRoutine(message));
    }

    /// <summary>
    ///     Notification coroutine.
    /// </summary>
    private IEnumerator NotificationRoutine(string message) {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        notificationText.gameObject.SetActive(false);
    }

    /// <summary>
    ///     Determines the current time state based on the in-game time.
    ///     The day is divided into four phases:
    ///     - Sunrise (05:00�06:00)
    ///     - Day (06:00�20:00)
    ///     - Sunset (20:00�22:00)
    ///     - Night (22:00�05:00)
    /// </summary>
    /// <returns>
    ///     The current TimeState from enum based on timeOfDay.
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
    ///     Updates sun rotation, intensity, color and ambient lighting.
    /// </summary>
    private void UpdateLighting() {
        if (sunLight == null)
            return;

        var normalizedTime = timeOfDay / 24f;

        sunLight.transform.rotation = Quaternion.Euler(
            normalizedTime * 360f - 90f,
            170f,
            0f
        );

        var t = 0f;

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
    ///     Sets time manually (debug/testing).
    /// </summary>
    public void SetTime(float time) {
        timeOfDay = time;
    }

    /// <summary>
    ///     Current time state.
    /// </summary>
    private enum TimeState {
        Night,
        Sunrise,
        Day,
        Sunset
    }
}
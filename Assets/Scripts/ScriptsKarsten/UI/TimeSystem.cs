using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Controls the day and night cycle,
/// updates the UI clock,
/// and shows notifications when switching
/// between day and night.
/// </summary>
public class DayNightCycle : MonoBehaviour {
    /// <summary>
    /// Reference to the UI text displaying
    /// the current cycle and time.
    /// </summary>
    public TMP_Text timeText;

    /// <summary>
    /// Reference to the UI text used
    /// for transition notifications.
    /// </summary>
    public TMP_Text notificationText;

    /// <summary>
    /// Determines how fast in-game time progresses.
    /// Value represents in-game hours per real second.
    /// </summary>
    public float minutesPerRealSecond = 0.1f;

    /// <summary>
    /// Current in-game time of day.
    /// Starts at 06:00 AM.
    /// </summary>
    private float timeOfDay = 6f;


    /// <summary>
    /// Current cycle number
    /// like Day 1, Night 1, etc.
    /// </summary>
    private int cycleNumber = 1;

    /// <summary>
    /// Tracks whether the current state is day or night.
    /// </summary>
    private bool isDay = true;

    /// <summary>
    /// Initializes the notification UI
    /// by hiding it at game start.
    /// </summary>
    void Start() {
        notificationText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the in-game time,
    /// detects day/night transitions,
    /// and refreshes the UI.
    /// </summary>
    void Update() {
        // Advance time continuously
        timeOfDay += Time.deltaTime * minutesPerRealSecond;

        // Reset after 24 hours
        timeOfDay %= 24f;

        // Determine whether it is currently day
        bool currentlyDay =
            (timeOfDay >= 6f && timeOfDay < 22f);

        // Detect transition between day and night
        if (currentlyDay != isDay) {
            isDay = currentlyDay;

            if (isDay) {
                // new day starts
                cycleNumber++;

                // update cycle number to a maximum of 10
                if (cycleNumber > 10)
                    cycleNumber = 10;

                ShowNotification(
                    $"Day {cycleNumber} begins!"
                );
            } else {
                // Night starts
                ShowNotification(
                    $"Night {cycleNumber} begins!"
                );
            }
        }

        // Refresh UI
        UpdateUI();
    }

    /// <summary>
    /// Updates the visible UI text
    /// showing current cycle and clock time.
    /// </summary>
    void UpdateUI() {
        // Extract hours
        int hours =
            Mathf.FloorToInt(timeOfDay);

        // Extract minutes
        int minutes =
            Mathf.FloorToInt(
                (timeOfDay - hours) * 60f
            );

        // Determine current state label
        string state = isDay
            ? $"Day {cycleNumber}"
            : $"Night {cycleNumber}";

        // Update UI text
        timeText.text =
            $"{state}\n{hours:00}:{minutes:00}";
    }

    /// <summary>
    /// Starts the notification coroutine.
    /// </summary>
    /// <param name="message">
    /// Message to display on screen.
    /// </param>
    void ShowNotification(string message) {
        StartCoroutine(
            NotificationRoutine(message)
        );
    }

    /// <summary>
    /// Displays a notification message
    /// for a short duration.
    /// </summary>
    /// <param name="message">
    /// Message shown to the player.
    /// </param>
    /// <returns>
    /// Coroutine enumerator.
    /// </returns>
    IEnumerator NotificationRoutine(string message) {
        notificationText.text = message;
        notificationText.gameObject.SetActive(true);

        // Keep notification visible
        yield return new WaitForSeconds(3f);

        // Hide notification
        notificationText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Public read-only access as a Getter for tests
    /// </summary>
    public float TimeOfDay => timeOfDay;

    /// <summary>
    /// Setter for the tests
    /// </summary>
    /// <param name="time"></param>
    public void SetTime(float time) {
        timeOfDay = time;
    }
}
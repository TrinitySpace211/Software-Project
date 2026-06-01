using NUnit.Framework;
using UnityEngine;
using TMPro;
using UnityEngine.TestTools;
using System.Collections;

public class DayNightCycleTests {
    private GameObject testObject;
    private TimeSystem cycle;

    private TMP_Text timeText;
    private TMP_Text notificationText;

    /// <summary>
    /// Creates a fresh instance of the system before each test.
    /// </summary>
    [UnitySetUp]
    public IEnumerator Setup() {
        // Create system under test
        testObject = new GameObject("DayNightCycle");
        cycle = testObject.AddComponent<TimeSystem>();

        // Mock UI elements
        GameObject timeGO = new GameObject("TimeText");
        timeText = timeGO.AddComponent<TextMeshProUGUI>();

        GameObject notifGO = new GameObject("NotificationText");
        notificationText = notifGO.AddComponent<TextMeshProUGUI>();

        // Inject dependencies
        cycle.timeText = timeText;
        cycle.notificationText = notificationText;

        yield return null;
    }


    /// <summary>
    /// Ensures system starts in a valid state.
    /// </summary>
    [UnityTest]
    public IEnumerator Starts_With_Valid_State() {
        yield return null;

        Assert.IsNotNull(cycle);
        Assert.IsNotNull(cycle.timeText);
        Assert.IsNotNull(cycle.notificationText);

        Assert.GreaterOrEqual(cycle.TimeOfDay, 6f);
    }

    /// <summary>
    /// Ensures time increases over real time.
    /// </summary>
    [UnityTest]
    public IEnumerator Time_Increases_Over_Time() {
        float startTime = cycle.TimeOfDay;

        yield return new WaitForSeconds(0.2f);

        float newTime = cycle.TimeOfDay;

        Assert.Greater(newTime, startTime);
    }

    /// <summary>
    /// Ensures time wrapping at 24h works correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator Time_Wraps_After_24_Hours() {
        cycle.SetTime(23.9f);

        yield return new WaitForSeconds(0.3f);

        Assert.Less(cycle.TimeOfDay, 24f);
    }

    /// <summary>
    /// Ensures night transition triggers notification.
    /// </summary>
    [UnityTest]
    public IEnumerator Night_Transition_Shows_Notification() {
        cycle.SetTime(21.9f);

        yield return null;

        cycle.SetTime(22.1f);

        yield return null;

        Assert.IsTrue(
            notificationText.text.Contains("Night") ||
            notificationText.text.Contains("Nacht"),
            "Night transition should trigger notification"
        );
    }

    /// <summary>
    /// Ensures UI text is updated correctly.
    /// </summary>
    [UnityTest]
    public IEnumerator UI_Updates_Correctly() {
        yield return new WaitForSeconds(0.2f);

        Assert.IsFalse(string.IsNullOrEmpty(timeText.text));
        Assert.IsTrue(
            timeText.text.Contains(":"),
            "Time format should contain ':'"
        );
    }

    /// <summary>
    /// Ensures day/night label is present in UI.
    /// </summary>
    [UnityTest]
    public IEnumerator UI_Contains_Day_Or_Night_Label() {
        yield return new WaitForSeconds(0.2f);

        bool hasLabel =
            timeText.text.Contains("Day") ||
            timeText.text.Contains("Night") ||
            timeText.text.Contains("Tag") ||
            timeText.text.Contains("Nacht");

        Assert.IsTrue(
            hasLabel,
            "UI should contain day or night label"
        );
    }

    /// <summary>
    /// Ensures that the sun light reference is correctly assigned
    /// and not null when injected into the DayNightCycle system.
    /// </summary>
    [UnityTest]
    public IEnumerator Sun_Light_Is_Assigned() {
        var lightGO = new GameObject("Sun");
        var light = lightGO.AddComponent<Light>();

        cycle.sunLight = light;

        yield return null;

        Assert.IsNotNull(cycle.sunLight, "Sun light should be assigned");
    }

    /// <summary>
    /// Verifies that the light intensity during daytime
    /// is higher than during nighttime.
    /// </summary>
    [UnityTest]
    public IEnumerator Day_Has_Higher_Light_Than_Night() {
        var lightGO = new GameObject("Sun");
        var light = lightGO.AddComponent<Light>();

        cycle.sunLight = light;

        // Day
        cycle.SetTime(12f);
        yield return null;

        float dayIntensity = light.intensity;

        // Night
        cycle.SetTime(23f);
        yield return null;

        float nightIntensity = light.intensity;

        Assert.Greater(dayIntensity, nightIntensity,
            "Day light should be stronger than night light");
    }

    /// <summary>
    /// Ensures that nighttime correctly reduces light intensity
    /// and does not exceed the configured day light intensity.
    /// </summary>
    [UnityTest]
    public IEnumerator Night_Reduces_Light_Intensity() {
        var lightGO = new GameObject("Sun");
        var light = lightGO.AddComponent<Light>();

        cycle.sunLight = light;

        cycle.SetTime(23f);
        yield return null;

        Assert.LessOrEqual(light.intensity, cycle.dayLightIntensity,
            "Night intensity should not exceed day intensity");
    }

    /// <summary>
    /// Cleans up after each test.
    /// </summary>
    [UnityTearDown]
    public IEnumerator TearDown() {
        Object.Destroy(testObject);
        Object.Destroy(timeText.gameObject);
        Object.Destroy(notificationText.gameObject);

        yield return null;
    }
}


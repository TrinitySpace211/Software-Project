using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Lights the Safe Zone
/// The FloodLights get activated when the Sun sets and get deactivated when the Sun rises
/// </summary>
public class FloodLights : MonoBehaviour {

    public Light spotLight;
    public Light pointLight;

    public float spotLightMaxIntensity;
    public float pointLightMaxIntensity;

    public float waitBeforeActivate = 1f;

    private void Start() {
        spotLight.enabled = false;
        pointLight.enabled = false;
    }

    /// <summary>
    /// When the Sun sets the Lights will be turned on
    /// </summary>
    private void DayNightCycle_OnSunsetStarted() {
        StartCoroutine(TurnOnSpotLight());
        StartCoroutine(TurnOnPointLight());
    }

    /// <summary>
    /// When the Sun rises the Lights will be turned off
    /// </summary>
    private void DayNightCycle_OnSunriseStarted() {
        spotLight.enabled = false;
        pointLight.enabled = false;
    }

    /// <summary>
    /// Turns the Spot Lights on by slowly increasing the intensity
    /// </summary>
    private IEnumerator TurnOnSpotLight() {
        yield return new WaitForSeconds(waitBeforeActivate);

        spotLight.enabled = true;
        spotLight.intensity = 0f;

        while (spotLight.intensity < spotLightMaxIntensity) {
            spotLight.intensity += Time.deltaTime * 30f;
            yield return null;
        }
    }

    /// <summary>
    /// Turns the Point Lights on by slowly increasing the intensity
    /// </summary>
    private IEnumerator TurnOnPointLight() {
        yield return new WaitForSeconds(waitBeforeActivate);

        pointLight.enabled = true;
        pointLight.intensity = 0f;

        while (pointLight.intensity < pointLightMaxIntensity) {
            pointLight.intensity += Time.deltaTime * 15f;
            yield return null;
        }
    }

    /// <summary>
    /// Subscribes all the Events
    /// </summary>
    private void OnEnable() {
        DayNightCycle.OnSunsetStarted += DayNightCycle_OnSunsetStarted;
        DayNightCycle.OnSunriseStarted += DayNightCycle_OnSunriseStarted;
    }

    /// <summary>
    /// Unsubscribes all the Events
    /// </summary>
    private void OnDisable() {
        DayNightCycle.OnSunsetStarted -= DayNightCycle_OnSunsetStarted;
        DayNightCycle.OnSunriseStarted -= DayNightCycle_OnSunriseStarted;
    }
}

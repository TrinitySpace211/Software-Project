using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Lights the Streets
/// The Lights get activated when the Sun sets and get deactivated when the Sun rises
/// </summary>
public class PoleLight : MonoBehaviour {

    public bool ignore = false;
    public bool flicker = false;

    [Header("Flicker Settings")]
    public float minIntensity = 0.5f;
    public float maxIntensitySpotLight = 10f;
    public float maxIntensityPointLight = 10f;

    public float minInterval = 0.02f;
    public float maxInterval = 0.15f;

    public float waitBeforeActivate = 2f;

    public Light spotLight;
    public Light pointLight;

    private void Start() {
        spotLight.enabled = false;
        pointLight.enabled = false;

        if (ignore) return;
    }

    /// <summary>
    /// When the Sun rises the Lights will be turned off
    /// </summary>
    private void DayNightCycle_OnSunriseStarted() {
        if (ignore) return;

        spotLight.enabled = false;
        pointLight.enabled = false;
    }

    /// <summary>
    /// When the Sun sets the Lights will be turned on
    /// </summary>
    private void DayNightCycle_OnSunsetStarted() {
        if (ignore) return;

        StartCoroutine(TurnOnLight());
    }

    /// <summary>
    /// Turns the Lights on by slowly increasing the intensity
    /// and if flicker is set to true then it will start the flicker Coroutine
    /// </summary>
    private IEnumerator TurnOnLight() {
        yield return new WaitForSeconds(waitBeforeActivate);

        spotLight.enabled = true;
        pointLight.enabled = true;

        for (int i = 0; i < Random.Range(5, 10); i++) {
            spotLight.enabled = !spotLight.enabled;
            pointLight.enabled = !pointLight.enabled;

            float intensityTemp = Random.Range(minIntensity, maxIntensitySpotLight);
            spotLight.intensity = intensityTemp;
            pointLight.intensity = intensityTemp;

            yield return new WaitForSeconds(Random.Range(0.03f, 0.15f));
        }

        spotLight.enabled = true;
        pointLight.enabled = true;
        spotLight.intensity = 0f;
        pointLight.intensity = 0f;

        bool max1 = false;
        bool max2 = false;
        while (!max1 && !max2) {

            if (spotLight.intensity < maxIntensitySpotLight) {
                spotLight.intensity += Time.deltaTime * 15f;
            } else {
                max1 = true;
            }

            if (pointLight.intensity < maxIntensityPointLight) {
                pointLight.intensity += Time.deltaTime * 15f;
            } else {
                max2 = true;
            }

            yield return null;
        }

        if (flicker) {
            StartCoroutine(Flicker());
        }
    }

    /// <summary>
    /// Starts to randomly change the intensity of the Lights
    /// </summary>
    private IEnumerator Flicker() {
        while (spotLight.enabled && pointLight.enabled) {
            spotLight.intensity = Random.Range(minIntensity, maxIntensitySpotLight);
            pointLight.intensity = Random.Range(minIntensity, maxIntensityPointLight);
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
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

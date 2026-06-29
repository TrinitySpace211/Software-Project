using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class FloodLights : MonoBehaviour {

    public Light spotLight;
    public Light pointLight;

    public float spotLightMaxIntensity;
    public float pointLightMaxIntensity;

    public float waitBeforeActivate = 1f;

    private void Start() {
        spotLight.enabled = false;
        pointLight.enabled = false;

        DayNightCycle.OnSunsetStarted += DayNightCycle_OnSunsetStarted;
        DayNightCycle.OnSunriseStarted += DayNightCycle_OnSunriseStarted;
    }

    private void DayNightCycle_OnSunsetStarted() {
        StartCoroutine(TurnOnSpotLight());
        StartCoroutine(TurnOnPointLight());
    }

    private void DayNightCycle_OnSunriseStarted() {
        spotLight.enabled = false;
        pointLight.enabled = false;
    }

    private IEnumerator TurnOnSpotLight() {
        yield return new WaitForSeconds(waitBeforeActivate);

        spotLight.enabled = true;
        spotLight.intensity = 0f;

        while (spotLight.intensity < spotLightMaxIntensity) {
            spotLight.intensity += Time.deltaTime * 30f;
            yield return null;
        }
    }
    private IEnumerator TurnOnPointLight() {
        yield return new WaitForSeconds(waitBeforeActivate);

        pointLight.enabled = true;
        pointLight.intensity = 0f;

        while (pointLight.intensity < pointLightMaxIntensity) {
            pointLight.intensity += Time.deltaTime * 15f;
            yield return null;
        }

    }
}

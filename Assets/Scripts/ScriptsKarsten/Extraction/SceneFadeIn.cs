using UnityEngine;
using System.Collections;

/// <summary>
/// Handles a fade-in effect at the start of a scene,
/// gradually making a CanvasGroup transparent.
/// </summary>
public class SceneFadeIn : MonoBehaviour {
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1.5f;

    /// <summary>
    /// Initializes the fade-in effect and starts the fade coroutine.
    /// </summary>
    private void Start() {
        if (fadeGroup == null)
            fadeGroup = GetComponentInChildren<CanvasGroup>();

        if (fadeGroup != null) {
            fadeGroup.alpha = 1f;
            StartCoroutine(FadeToTransparent());
        }
    }

    /// <summary>
    /// Gradually fades the CanvasGroup from opaque to transparent.
    /// </summary>
    private IEnumerator FadeToTransparent() {
        float t = 0f;

        while (t < fadeDuration) {
            t += Time.deltaTime;
            fadeGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        fadeGroup.alpha = 0f;
    }
}
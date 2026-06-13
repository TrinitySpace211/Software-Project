using UnityEngine;
using System.Collections;

public class SceneFadeIn : MonoBehaviour {
    [SerializeField] private CanvasGroup fadeGroup;
    [SerializeField] private float fadeDuration = 1.5f;

    private void Start() {
        if (fadeGroup == null)
            fadeGroup = GetComponentInChildren<CanvasGroup>();

        if (fadeGroup != null) {
            fadeGroup.alpha = 1f;
            StartCoroutine(FadeToTransparent());
        }
    }

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
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the death screen UI, including fade-in effects,
/// </summary>
public class DeathScreen : MonoBehaviour {
    [Header("UI")]
    public CanvasGroup deathCanvas;
    public CanvasGroup buttonCanvas;
    public TMP_Text nightsText;
    public TMP_Text youDiedText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deathSound;
    public float deathSoundVolume;
    public AudioClip youDiedSound;
    public float youDiedSoundVolume;

    [Header("Scene")]
    public Loader.Scene mainMenuScene = Loader.Scene.MainMenu;
    public Loader.Scene gameScene = Loader.Scene.MainScene;

    private void Start() {
        youDiedText.alpha = 0f;
        nightsText.alpha = 0f;
    }

    /// <summary>
    /// Triggers the death screen sequence.
    /// </summary>
    public void ShowDeathScreen(int survivedNights) {
        StartCoroutine(DeathRoutine(survivedNights));
    }

    /// <summary>
    /// Handles the full death screen flow including UI disabling,
    /// fade-in animations, audio playback, and interaction enabling.
    /// </summary>
    private IEnumerator DeathRoutine(int survivedNights) {
        yield return new WaitForSeconds(3f);

        Time.timeScale = 0f;

        // Enable death UI
        deathCanvas.gameObject.SetActive(true);
        buttonCanvas.gameObject.SetActive(true);

        deathCanvas.alpha = 0f;
        buttonCanvas.alpha = 0f;

        buttonCanvas.interactable = false;
        buttonCanvas.blocksRaycasts = false;

        // Display survived nights text
        if (nightsText != null) {
            nightsText.text =
                $"You survived {survivedNights} night{(survivedNights == 1 ? "" : "s")}";
        }

        // Play you Died sound
        if (audioSource != null && youDiedSound != null) {
            audioSource.volume = youDiedSoundVolume * SoundManager.Instance.volume;
            audioSource.PlayOneShot(youDiedSound);
        }

        // Fade in main death canvas
        float t = 0f;
        while (t < 1f) {
            t += Time.unscaledDeltaTime;
            deathCanvas.alpha = t;
            yield return null;
        }

        // Play death sound
        if (audioSource != null && deathSound != null) {
            audioSource.volume = deathSoundVolume * SoundManager.Instance.volume;
            audioSource.PlayOneShot(deathSound);
        }

        // Fade in death text
        float dt = 0f;
        while (dt < 1f) {
            dt += Time.unscaledDeltaTime;
            youDiedText.alpha = dt;
            yield return null;
        }

        // Fade in nights text
        float nt = 0f;
        while (nt < 1f) {
            nt += Time.unscaledDeltaTime;
            nightsText.alpha = nt;
            yield return null;
        }

        // Wait before showing buttons
        yield return new WaitForSecondsRealtime(4f);

        buttonCanvas.interactable = true;
        buttonCanvas.blocksRaycasts = true;

        // Fade in button canvas
        float b = 0f;
        while (b < 1f) {
            b += Time.unscaledDeltaTime;
            buttonCanvas.alpha = b;
            yield return null;
        }
    }

    /// <summary>
    /// Restarts the current scene.
    /// </summary>
    public void Retry() {
        Time.timeScale = 1f;
        Loader.Load(gameScene);
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void MainMenu() {
        Time.timeScale = 1f;
        Loader.Load(mainMenuScene);
    }

    /// <summary>
    /// Exits the application or stops play mode in the Unity editor.
    /// </summary>
    public void Exit() {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
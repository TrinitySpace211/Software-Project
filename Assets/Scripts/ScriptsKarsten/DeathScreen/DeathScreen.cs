using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controls the death screen UI, including fade-in effects,
/// disabling gameplay UI, and scene transitions after player death.
/// </summary>
public class DeathScreen : MonoBehaviour {
    [Header("UI")]
    public CanvasGroup deathCanvas;
    public CanvasGroup buttonCanvas;
    public TMP_Text nightsText;

    [Header("Disable On Death")]
    public GameObject inventoryUI;
    public GameObject crosshair;
    public GameObject timeDisplay;
    public GameObject healthBar;
    public GameObject gasTankHud;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deathSound;

    [Header("Scene")]
    public string mainMenuScene = "MainMenu";
    public string gameScene = "MainScene";

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
        Time.timeScale = 0f;

        // Disable gameplay-related UI elements
        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        if (crosshair != null)
            crosshair.SetActive(false);

        if (timeDisplay != null)
            timeDisplay.SetActive(false);

        if (healthBar != null)
            healthBar.SetActive(false);

        if (gasTankHud != null)
            gasTankHud.SetActive(false);

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

        // Play death sound
        if (audioSource != null && deathSound != null) {
            audioSource.PlayOneShot(deathSound);
        }

        // Fade in main death canvas
        float t = 0f;
        while (t < 1f) {
            t += Time.unscaledDeltaTime;
            deathCanvas.alpha = t;
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Loads the main menu scene.
    /// </summary>
    public void MainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
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
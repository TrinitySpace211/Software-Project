using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deathSound;

    [Header("Scene")]
    public string mainMenuScene = "MainMenu";
    public string gameScene = "MainScene";


    public void ShowDeathScreen(int survivedNights) {
        StartCoroutine(DeathRoutine(survivedNights));
    }

    private IEnumerator DeathRoutine(int survivedNights) {
        Time.timeScale = 0f;

        // Inventory, Text, Healthbar und Crosshair ausblenden
        if (inventoryUI != null)
            inventoryUI.SetActive(false);

        if (crosshair != null)
            crosshair.SetActive(false);

        if (timeDisplay != null)
            timeDisplay.SetActive(false);

        if (healthBar != null)
            healthBar.SetActive(false);

        deathCanvas.gameObject.SetActive(true);
        buttonCanvas.gameObject.SetActive(true);

        deathCanvas.alpha = 0f;
        buttonCanvas.alpha = 0f;

        buttonCanvas.interactable = false;
        buttonCanvas.blocksRaycasts = false;

        if (nightsText != null) {
            nightsText.text =
                $"You survived {survivedNights} night{(survivedNights == 1 ? "" : "s")}";
        }

        if (audioSource != null && deathSound != null) {
            audioSource.PlayOneShot(deathSound);
        }

        float t = 0f;
        while (t < 1f) {
            t += Time.unscaledDeltaTime;
            deathCanvas.alpha = t;
            yield return null;
        }

        yield return new WaitForSecondsRealtime(4f);

        buttonCanvas.interactable = true;
        buttonCanvas.blocksRaycasts = true;

        float b = 0f;
        while (b < 1f) {
            b += Time.unscaledDeltaTime;
            buttonCanvas.alpha = b;
            yield return null;
        }
    }

    public void Retry() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void MainMenu() {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void Exit() {
        Time.timeScale = 1f;

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
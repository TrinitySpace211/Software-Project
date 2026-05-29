using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the player's health bar UI.
/// </summary>
public class HealthBar : MonoBehaviour {

    private Image image;
    private PlayerStats playerStats;

    private void Awake() {
        image = GetComponent<Image>();

        if (image == null) {
            Debug.LogError("HealthBar: Kein Image Component gefunden!");
        }
    }

    /// <summary>
    /// Initializes the health bar.
    /// </summary>
    public void Initialize(PlayerStats stats) {

        if (stats == null) {
            return;
        }

        playerStats = stats;
        UpdateHealthBar();
    }

    private void Update() {

        if (playerStats == null || image == null)
            return;

        UpdateHealthBar();
    }

    /// <summary>
    /// Updates the health bar fill amount.
    /// </summary>
    public void UpdateHealthBar() {

        if (playerStats == null || image == null)
            return;

        image.fillAmount =
            playerStats.currentHealth /
            playerStats.maxHealth;
    }
}
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the player's health bar UI.
/// </summary>
public class HealthBar : MonoBehaviour {

    private Image image;
    private PlayerStats playerStats;

    /// <summary>
    /// Caches the Image component used for the health bar.
    /// </summary>
    private void Awake() {
        image = GetComponent<Image>();

        if (image == null) {
            Debug.LogError("HealthBar: Kein Image Component gefunden!");
        }
    }

    /// <summary>
    /// Initializes the health bar with the given player stats.
    /// </summary>
    public void Initialize(PlayerStats stats) {
        if (stats == null) {
            return;
        }

        playerStats = stats;
        UpdateHealthBar();
    }

    /// <summary>
    /// Updates the health bar every frame.
    /// </summary>
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

    /// <summary>
    /// Assigns the UI Image used by the health bar.
    /// </summary>
    /// <param name="img">The Image component that displays the health fill.</param>
    public void SetImage(Image img) {
        image = img;
    }
}
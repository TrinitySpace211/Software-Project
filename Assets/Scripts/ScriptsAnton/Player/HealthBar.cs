using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
    private Image image;
    private PlayerStats playerStats;

    // Wichtig: Awake statt Start (Fix für NullReference)
    private void Awake() {
        image = GetComponent<Image>();
    }

    // Verbindung zu PlayerStats
    public void Initialize(PlayerStats stats) {
        playerStats = stats;

        UpdateHealthBar();
    }

    public void UpdateHealthBar() {
        // Safety Check (verhindert NullReference komplett)
        if (image == null || playerStats == null)
            return;

        image.fillAmount =
            (float)playerStats.CurrentHealth /
            playerStats.MaxHealth;
    }
}
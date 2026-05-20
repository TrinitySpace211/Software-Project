using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {

    private Image image;
    private PlayerStats playerStats;

    // Cache der Image-Komponente f�r schnelleren Zugriff und weniger GetComponent-Aufrufe
    private void Awake() {
        image = GetComponent<Image>();
    }

    // Verkn�pft die HealthBar mit den PlayerStats und initialisiert direkt den UI-Zustand
    public void Initialize(PlayerStats stats) {
        playerStats = stats;
        UpdateHealthBar();
    }

    // Aktualisiert die Anzeige basierend auf aktuellem und maximalem Leben
    public void UpdateHealthBar() {

        // Schutz vor fehlender Initialisierung (verhindert NullReference Exceptions)
        if (image == null || playerStats == null)
            return;

        image.fillAmount =
            (float)playerStats.currentHealth /
            playerStats.maxHealth;
    }
}
using UnityEngine;
using UnityEngine.UI;

// Diese Klasse steuert die Healthbar des Spielers
public class HealthBar : MonoBehaviour {

    // Referenz auf die Image-Komponente
    // Das ist der rote Balken, dessen fillAmount verändert wird
    private Image image;

    // Aktuelle Lebenspunkte des Spielers
    public float health = 100;

    // Start() wird einmal beim Start des Spiels ausgeführt
    private void Start() {

        // Holt automatisch die Image-Komponente
        // vom gleichen GameObject, an dem dieses Script hängt
        image = GetComponent<Image>();

        // Setzt die Healthbar passend zum aktuellen Leben
        image.fillAmount = health / 100f;
    }

    // Funktion zum Verändern der Lebenspunkte
    // changeAmount
    // Negativer Wert = Schaden
    // Positiver Wert = Heilung

    public void ChangeHealth(float changeAmount) {

        // Verändert den aktuellen Lebenswert
        health += changeAmount;

        // Verhindert ungültige Werte
        // Leben bleibt immer zwischen 0 und 100
        health = Mathf.Clamp(health, 0, 100);

        // Aktualisiert den sichtbaren Füllstand der Healthbar

        image.fillAmount = health / 100f;
    }

    // Update() wird in jedem Frame ausgeführt
    void Update() {

        // Prüft, ob die Leertaste gedrückt wurde
        if (UnityEngine.InputSystem.Keyboard.current
            .spaceKey.wasPressedThisFrame) {

            // Zieht 10 Leben ab
            ChangeHealth(-10);

        }

        // Prüft, ob ALT gedrückt wurde
        else if (UnityEngine.InputSystem.Keyboard.current
                 .altKey.wasPressedThisFrame) {

            // Fügt 10 Leben hinzu
            ChangeHealth(10);
        }
    }
}
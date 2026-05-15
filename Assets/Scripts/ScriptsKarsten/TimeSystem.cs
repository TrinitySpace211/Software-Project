using UnityEngine;
using TMPro;

// Dieses Script steuert den Tag-/Nacht-Zyklus
public class DayNightCycle : MonoBehaviour {

    // Referenz auf das UI-Textfeld
    public TMP_Text timeText;

    // Bestimmt, wie schnell die Zeit vergeht
    public float minutesPerRealSecond = 0.1f;

    // Speichert die aktuelle Uhrzeit im Spiel
    // Startwert = 06:00 Uhr morgens
    private float timeOfDay = 6f;

    // Update() wird in jedem einzelnen Frame aufgerufen
    void Update() {

        // Erhöht die Spielzeit kontinuierlich
        // Time.deltaTime ist die Zeit seit dem letzten Frame
        timeOfDay += Time.deltaTime * minutesPerRealSecond;

        // Sorgt dafür, dass die Uhr nach 24 Stunden wieder bei 0 beginnt
        timeOfDay %= 24f;

        // Aktualisiert die Anzeige im UI
        UpdateUI();
    }

    // Diese Funktion aktualisiert den sichtbaren Text
    void UpdateUI() {

        // Holt die Stunden aus der Dezimalzahl
        int hours = Mathf.FloorToInt(timeOfDay);

        // Berechnet die Minuten
        int minutes = Mathf.FloorToInt((timeOfDay - hours) * 60f);

        // Prüft, ob Tag oder Nacht ist
        //
        // Tag: 06:00 bis 21:59
        // Nacht: 22:00 bis 05:59
        string dayState =
            (timeOfDay >= 6f && timeOfDay < 22f)
            ? "Tag"
            : "Nacht";

        // Setzt den Text im UI
        timeText.text = $"{dayState}\n{hours:00}:{minutes:00}";
    }
}
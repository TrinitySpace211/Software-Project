using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Speichert den Schrott vom Spieler.
/// </summary>
public class PlayerScrapWallet : MonoBehaviour {
    // Schrott, mit dem der Spieler am Anfang startet.
    [SerializeField] private int startingScrap;

    // Aktueller Schrott vom Spieler.
    [SerializeField] private int currentScrap;

    // Text, der kurz angezeigt wird, wenn der Spieler Schrott bekommt.
    [SerializeField] private Text scrapMessageText;

    // Wie lange die Schrott-Meldung sichtbar bleibt.
    [SerializeField] private float messageDuration = 2f;

    // Andere Scripts können so den aktuellen Schrott lesen.
    public int ScrapAmount => currentScrap;

    private float messageTimer;

    private void Awake() {
        // Setzt den Startwert und erstellt den Text, falls keiner gesetzt wurde.
        currentScrap = Mathf.Max(0, startingScrap);
        CreateMessageTextIfMissing();
    }

    private void Update() {
        SyncScrapFromInventory();

        // Wenn keine Meldung aktiv ist, muss nichts gemacht werden.
        if (scrapMessageText == null || !scrapMessageText.gameObject.activeSelf) {
            return;
        }

        // Versteckt die Meldung nach kurzer Zeit.
        messageTimer -= Time.deltaTime;
        if (messageTimer <= 0f) {
            scrapMessageText.gameObject.SetActive(false);
        }
    }

    public void AddScrap(int amount) {
        // Negative oder leere Werte werden ignoriert.
        if (amount <= 0) {
            return;
        }

        // Schrott erhöhen und eine Meldung anzeigen.
        currentScrap += amount;
        ScrapInventorySaver.AddScrapToInventory(amount);
        ShowScrapMessage(amount);
        Debug.Log($"Player received {amount} scrap. Total scrap: {currentScrap}");
    }

    public bool TrySpendScrap(int amount) {
        // 0 oder weniger kostet nichts.
        if (amount <= 0) {
            return true;
        }

        // Wenn der Spieler nicht genug Schrott hat, kann er nicht bezahlen.
        if (currentScrap < amount) {
            return false;
        }

        // Schrott abziehen, wenn genug vorhanden ist.
        currentScrap -= amount;
        return true;
    }

    private void SyncScrapFromInventory() {
        // Wenn Scrap im Inventar geaendert wurde, wird die Wallet angepasst.
        if (ScrapInventorySaver.TryGetScrapAmount(out int inventoryScrap)) {
            currentScrap = inventoryScrap;
        }
    }

    private void ShowScrapMessage(int amount) {
        // Ohne Textfeld kann keine Meldung angezeigt werden.
        if (scrapMessageText == null) {
            return;
        }

        // Text aktualisieren und anzeigen.
        scrapMessageText.text = $"+{amount} Scrap erhalten\nScrap: {currentScrap}";
        scrapMessageText.gameObject.SetActive(true);
        messageTimer = messageDuration;
    }

    private void CreateMessageTextIfMissing() {
        // Wenn schon ein Text im Inspector gesetzt wurde, benutzen wir diesen.
        if (scrapMessageText != null) {
            scrapMessageText.gameObject.SetActive(false);
            return;
        }

        // Erstellt ein einfaches Canvas für die Schrott-Meldung.
        GameObject canvasObject = new GameObject("Scrap Message Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // Erstellt das Text-Objekt oben auf dem Bildschirm.
        GameObject textObject = new GameObject("Scrap Message");
        textObject.transform.SetParent(canvas.transform, false);

        // Position und Größe vom Text.
        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -80f);
        rectTransform.sizeDelta = new Vector2(420f, 90f);

        // Text optisch vorbereiten.
        scrapMessageText = textObject.AddComponent<Text>();
        scrapMessageText.alignment = TextAnchor.MiddleCenter;
        scrapMessageText.fontSize = 30;
        scrapMessageText.color = Color.white;
        scrapMessageText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (scrapMessageText.font == null) {
            scrapMessageText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        scrapMessageText.raycastTarget = false;
        scrapMessageText.gameObject.SetActive(false);
    }
}

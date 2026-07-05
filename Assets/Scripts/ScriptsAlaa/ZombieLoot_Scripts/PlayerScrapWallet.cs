using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stores the player's scrap.
/// </summary>
public class PlayerScrapWallet : MonoBehaviour {
    // Scrap the player starts with.
    [SerializeField] private int startingScrap;

    // Player's current scrap.
    [SerializeField] private int currentScrap;

    // Text shown briefly when the player receives scrap.
    [SerializeField] private Text scrapMessageText;

    // How long the scrap message remains visible.
    [SerializeField] private float messageDuration = 2f;

    // Allows other scripts to read the current scrap amount.
    public int ScrapAmount => currentScrap;

    private float messageTimer;

    private void Awake() {
        // Sets the starting value and creates the text if none was assigned.
        currentScrap = Mathf.Max(0, startingScrap);
        CreateMessageTextIfMissing();
    }

    private void Update() {
        SyncScrapFromInventory();

        // Nothing needs to be done when no message is active.
        if (scrapMessageText == null || !scrapMessageText.gameObject.activeSelf) {
            return;
        }

        // Hides the message after a short time.
        messageTimer -= Time.deltaTime;
        if (messageTimer <= 0f) {
            scrapMessageText.gameObject.SetActive(false);
        }
    }

    public void AddScrap(int amount) {
        // Ignores negative or empty values.
        if (amount <= 0) {
            return;
        }

        // Increases the scrap amount and displays a message.
        currentScrap += amount;

        if (ScrapInventorySaver.instance != null) {
            ScrapInventorySaver.instance.AddScrapToInventory(amount);
        }

        ShowScrapMessage(amount);

    }

    public bool TrySpendScrap(int amount) {
        // An amount of 0 or less costs nothing.
        if (amount <= 0) {
            return true;
        }

        // The player cannot pay without enough scrap.
        if (currentScrap < amount) {
            return false;
        }

        // Removes scrap when enough is available.
        currentScrap -= amount;
        return true;
    }

    private void SyncScrapFromInventory() {
        // Updates the wallet when the scrap amount in the inventory changes.
        if (ScrapInventorySaver.instance != null) {
            if (ScrapInventorySaver.instance.TryGetScrapAmount(out int inventoryScrap)) {
                currentScrap = inventoryScrap;
            }
        }

    }

    private void ShowScrapMessage(int amount) {
        // A message cannot be displayed without a text field.
        if (scrapMessageText == null) {
            return;
        }

        // Updates and displays the text.
        scrapMessageText.text = $"+{amount} Scrap collected\nScrap: {currentScrap}";
        scrapMessageText.gameObject.SetActive(true);
        messageTimer = messageDuration;
    }

    private void CreateMessageTextIfMissing() {
        // Uses the text already assigned in the Inspector when available.
        if (scrapMessageText != null) {
            scrapMessageText.gameObject.SetActive(false);
            return;
        }

        // Creates a simple canvas for the scrap message.
        GameObject canvasObject = new GameObject("Scrap Message Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        // Creates the text object at the top of the screen.
        GameObject textObject = new GameObject("Scrap Message");
        textObject.transform.SetParent(canvas.transform, false);

        // Position and size of the text.
        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -200f);
        rectTransform.sizeDelta = new Vector2(420f, 90f);

        // Prepares the visual appearance of the text.
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

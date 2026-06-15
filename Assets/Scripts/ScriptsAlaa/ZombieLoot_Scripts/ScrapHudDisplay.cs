using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zeigt oben im UI das Scrap-Icon und die aktuelle Scrap-Anzahl an.
/// </summary>
public class ScrapHudDisplay : MonoBehaviour {
    // Spieler-Konto, aus dem die Scrap-Anzahl gelesen wird.
    [SerializeField] private PlayerScrapWallet playerWallet;

    // Icon vom Scrap. Hier kann Icon_Scrap.png eingetragen werden.
    [SerializeField] private Sprite scrapIcon;

    // Feste Position, falls keine Health-Bar gesetzt wurde.
    [SerializeField] private Vector2 fallbackHudPosition = new Vector2(20f, -90f);

    // Größe vom Icon.
    [SerializeField] private Vector2 iconSize = new Vector2(34f, 34f);

    // Farbe vom Anzahl-Text.
    [SerializeField] private Color textColor = new Color(1f, 0.82f, 0.35f);

    private RectTransform hudRect;
    private Text scrapCountText;

    private void Awake() {
        HideOldImageOnThisObject();
        FindWalletIfMissing();
        CreateHud();
    }

    private void Update() {
        FindWalletIfMissing();
        UpdateHudPosition();
        UpdateScrapCount();
    }

    private void FindWalletIfMissing() {
        // Sucht das Wallet automatisch, falls es im Inspector nicht gesetzt wurde.
        if (playerWallet != null) {
            return;
        }

        playerWallet = FindFirstObjectByType<PlayerScrapWallet>();
    }

    private void HideOldImageOnThisObject() {
        // Falls dieses Script aus Versehen auf ein rotes UI-Image gelegt wurde,
        // wird dieses alte Bild ausgeblendet.
        Image oldImage = GetComponent<Image>();
        if (oldImage != null) {
            oldImage.enabled = false;
        }
    }

    private void CreateHud() {
        // Erstellt ein eigenes Canvas nur für die Scrap-Anzeige.
        GameObject canvasObject = new GameObject("Scrap HUD Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasScaler.scaleFactor = 1f;
        canvasScaler.referencePixelsPerUnit = 100f;

        canvasObject.AddComponent<GraphicRaycaster>();

        // Container für Icon und Text.
        GameObject hudObject = new GameObject("Scrap HUD");
        hudObject.transform.SetParent(canvas.transform, false);

        hudRect = hudObject.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(0f, 1f);
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.anchoredPosition = fallbackHudPosition;
        hudRect.sizeDelta = new Vector2(126f, 46f);

        // Kleiner dunkler Hintergrund, damit die Anzeige zum Zombie-Spiel passt.
        Image background = hudObject.AddComponent<Image>();
        background.color = new Color(0.02f, 0.03f, 0.025f, 0.82f);
        background.raycastTarget = false;

        // Icon links in der Anzeige.
        GameObject iconObject = new GameObject("Scrap Icon");
        iconObject.transform.SetParent(hudObject.transform, false);

        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(8f, 0f);
        iconRect.sizeDelta = iconSize;

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.sprite = scrapIcon;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        // Text rechts neben dem Icon.
        GameObject textObject = new GameObject("Scrap Count");
        textObject.transform.SetParent(hudObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(50f, 0f);
        textRect.offsetMax = new Vector2(-8f, 0f);

        scrapCountText = textObject.AddComponent<Text>();
        scrapCountText.alignment = TextAnchor.MiddleLeft;
        scrapCountText.fontSize = 24;
        scrapCountText.fontStyle = FontStyle.Bold;
        scrapCountText.color = textColor;
        scrapCountText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (scrapCountText.font == null) {
            scrapCountText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        scrapCountText.raycastTarget = false;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        UpdateScrapCount();
    }

    private void UpdateHudPosition() {
        // Hält das Scrap-HUD an einer festen Position.
        if (hudRect == null) {
            return;
        }

        hudRect.anchoredPosition = fallbackHudPosition;
    }

    private void UpdateScrapCount() {
        // Aktualisiert die Zahl neben dem Scrap-Icon.
        if (scrapCountText == null) {
            return;
        }

        int scrapAmount = playerWallet != null ? playerWallet.ScrapAmount : 0;
        scrapCountText.text = $": {scrapAmount}";
    }
}

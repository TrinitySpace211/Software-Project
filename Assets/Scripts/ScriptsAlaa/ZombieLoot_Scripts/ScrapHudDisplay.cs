using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the scrap icon and the current scrap amount at the top of the UI.
/// </summary>
public class ScrapHudDisplay : MonoBehaviour {
    // Player wallet used to read the scrap amount.
    [SerializeField] private PlayerScrapWallet playerWallet;

    // Scrap icon. Icon_Scrap.png can be assigned here.
    [SerializeField] private Sprite scrapIcon;

    // Fixed position used when no health bar is assigned.
    [SerializeField] private Vector2 fallbackHudPosition = new Vector2(20f, -125f);

    // Size of the icon.
    [SerializeField] private Vector2 iconSize = new Vector2(38f, 38f);

    // Color of the amount text.
    [SerializeField] private Color textColor = new Color(1f, 0.82f, 0.35f);

    private RectTransform hudRect;
    private Text scrapCountText;
    private GameObject hudRoot;

    private void Awake() {
        HideOldImageOnThisObject();
        FindWalletIfMissing();
        CreateHud();
    }

    private void Update() {
        if (hudRoot == null || !hudRoot.activeSelf)
            return;
        FindWalletIfMissing();
        UpdateHudPosition();
        UpdateScrapCount();

        if (playerWallet.GetComponent<PlayerHealth>().GetIsDead()) {
            SetVisible(false);
        }
    }

    private void FindWalletIfMissing() {
        // Finds the wallet automatically if it was not assigned in the Inspector.
        if (playerWallet != null) {
            return;
        }

        playerWallet = FindFirstObjectByType<PlayerScrapWallet>();
    }

    private void HideOldImageOnThisObject() {
        // If this script was accidentally placed on a red UI image,
        // the old image is hidden.
        Image oldImage = GetComponent<Image>();
        if (oldImage != null) {
            oldImage.enabled = false;
        }
    }

    private void CreateHud() {
        // Creates a separate canvas for the scrap display.
        hudRoot = new GameObject("Scrap HUD Canvas");
        Canvas canvas = hudRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler canvasScaler = hudRoot.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasScaler.scaleFactor = 1f;
        canvasScaler.referencePixelsPerUnit = 100f;

        hudRoot.AddComponent<GraphicRaycaster>();

        // Container for the icon and text.
        GameObject hudObject = new GameObject("Scrap HUD");
        hudObject.transform.SetParent(hudRoot.transform, false);

        hudRect = hudObject.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(0f, 1f);
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.anchoredPosition = fallbackHudPosition;
        hudRect.sizeDelta = new Vector2(140f, 52f);

        // Small dark background that matches the zombie game.
        Image background = hudObject.AddComponent<Image>();
        background.color = new Color(0.02f, 0.03f, 0.025f, 0.82f);
        background.raycastTarget = false;

        // Icon on the left side of the display.
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

        // Text to the right of the icon.
        GameObject textObject = new GameObject("Scrap Count");
        textObject.transform.SetParent(hudObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(55f, 0f);
        textRect.offsetMax = new Vector2(-8f, 0f);

        scrapCountText = textObject.AddComponent<Text>();
        scrapCountText.alignment = TextAnchor.MiddleLeft;
        scrapCountText.fontSize = 26;
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
        // Keeps the scrap HUD at a fixed position.
        if (hudRect == null) {
            return;
        }

        hudRect.anchoredPosition = fallbackHudPosition;
    }

    private void UpdateScrapCount() {
        // Updates the number next to the scrap icon.
        if (scrapCountText == null) {
            return;
        }

        int scrapAmount = playerWallet != null ? playerWallet.ScrapAmount : 0;
        if (ScrapInventorySaver.instance.TryGetScrapAmount(out int inventoryScrap)) {
            scrapAmount = inventoryScrap;
        }

        scrapCountText.text = $": {scrapAmount}";
    }
    public void SetVisible(bool visible) {
        if (hudRoot != null)
            hudRoot.SetActive(visible);
    }
}

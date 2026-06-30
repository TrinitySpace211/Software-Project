using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Displays the ammunition of the currently equipped weapon.
/// </summary>
public class AmmunitionHudDisplay : MonoBehaviour {
    // Player weapon selector used to read the current weapon.
    [SerializeField] private PlayerWeaponSelector weaponSelector;

    // Ammunition icon from the project.
    [SerializeField] private Sprite ammunitionIcon;

    // Fixed position on the screen.
    [SerializeField] private Vector2 hudPosition = new Vector2(20f, -145f);

    // Size of the icon.
    [SerializeField] private Vector2 iconSize = new Vector2(34f, 34f);

    // Color of the ammunition text.
    [SerializeField] private Color textColor = new Color(0.85f, 0.95f, 1f);

    private PlayerWeaponSelector weaponSelector;
    private Inventory inventory;

    private RectTransform hudRect;
    private Image ammunitionIconImage;
    private Text ammunitionText;
    private GameObject hudObject;
    private GameObject canvasObject;

    private void Awake() {
        RemoveOwnRuntimeHudCanvas();
        HideOldImageOnThisObject();
        FindWeaponSelectorIfMissing();
        CreateHud();
    }

    private void Start() {
        weaponSelector = player.GetPlayerGunSelector();
        inventory = player.GetInventory();
    }

    private void Update() {
        FindWeaponSelectorIfMissing();
        FindIconIfMissing();
        UpdateAmmunitionIcon();
        UpdateHudPosition();
        UpdateAmmunitionText();
    }

    private void FindWeaponSelectorIfMissing() {
        // Finds the weapon selector automatically if it is empty in the Inspector.
        if (weaponSelector != null) {
            return;
        }

        player = FindFirstObjectByType<Player>();
    }

    private void FindIconIfMissing() {
        // Automatically finds the logo if the icon is empty in the Inspector.
        if (ammunitionIcon != null) {
            return;
        }

#if UNITY_EDITOR
        ammunitionIcon = AssetDatabase.LoadAssetAtPath<Sprite>(
            "Assets/Scripts/ScriptsAlaa/ammunition logo/ammunition logo.png"
        );
#endif
    }

    private void RemoveOwnRuntimeHudCanvas() {
        // Removes only the HUD created by this object.
        Transform oldCanvas = transform.Find("Ammunition HUD Canvas");
        if (oldCanvas != null) {
            Destroy(oldCanvas.gameObject);
        }
    }

    private void HideOldImageOnThisObject() {
        // Hides the old image if the script is attached to a UI image.
        Image oldImage = GetComponent<Image>();
        if (oldImage != null) {
            oldImage.enabled = false;
        }
    }

    private void CreateHud() {
        // Creates a separate canvas for the ammunition display.
        canvasObject = new GameObject("Ammunition HUD Canvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasScaler.scaleFactor = 1f;
        canvasScaler.referencePixelsPerUnit = 100f;

        canvasObject.AddComponent<GraphicRaycaster>();

        // Container for the icon and text.
        hudObject = new GameObject("Ammunition HUD");
        hudObject.transform.SetParent(canvas.transform, false);

        hudRect = hudObject.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(0f, 1f);
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.anchoredPosition = hudPosition;
        hudRect.sizeDelta = new Vector2(175f, 46f);

        // Dark background that makes the display easy to read.
        Image background = hudObject.AddComponent<Image>();
        background.color = new Color(0.02f, 0.025f, 0.03f, 0.82f);
        background.raycastTarget = false;

        // Icon on the left side of the display.
        GameObject iconObject = new GameObject("Ammunition Icon");
        iconObject.transform.SetParent(hudObject.transform, false);

        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(8f, 0f);
        iconRect.sizeDelta = iconSize;

        ammunitionIconImage = iconObject.AddComponent<Image>();
        ammunitionIconImage.color = Color.white;
        ammunitionIconImage.preserveAspect = true;
        ammunitionIconImage.raycastTarget = false;
        UpdateAmmunitionIcon();

        // Text to the right of the icon.
        GameObject textObject = new GameObject("Ammunition Count");
        textObject.transform.SetParent(hudObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(50f, 0f);
        textRect.offsetMax = new Vector2(-4f, 0f);

        ammunitionText = textObject.AddComponent<Text>();
        ammunitionText.alignment = TextAnchor.MiddleLeft;
        ammunitionText.fontSize = 23;
        ammunitionText.fontStyle = FontStyle.Bold;
        ammunitionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (ammunitionText.font == null) {
            ammunitionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        ammunitionText.raycastTarget = false;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        shadow.effectDistance = new Vector2(1.5f, -1.5f);

        UpdateAmmunitionText();
    }

    private void ApplyTextColor() {
        // Always uses the color from the Inspector field "Text Color".
        if (ammunitionText == null) {
            return;
        }

        ammunitionText.color = textColor;
        ammunitionText.canvasRenderer.SetColor(textColor);
        ammunitionText.SetVerticesDirty();
        ammunitionText.SetMaterialDirty();
    }

    private void UpdateHudPosition() {
        // Keeps the display at a fixed position.
        if (hudRect == null) {
            return;
        }

        hudRect.anchoredPosition = hudPosition;
    }

    private void UpdateAmmunitionIcon() {
        // Updates the icon after a reimport or an Inspector change.
        if (ammunitionIconImage == null) {
            return;
        }

        ammunitionIconImage.sprite = ammunitionIcon;
        ammunitionIconImage.enabled = ammunitionIcon != null;
        ammunitionIconImage.SetAllDirty();
    }

    private void UpdateAmmunitionText() {
        // Hides the display when no weapon is equipped.
        if (hudObject == null || ammunitionText == null) {
            return;
        }

        GunSO activeGun = weaponSelector != null ? weaponSelector.activeGun : null;
        bool hasGun = activeGun != null;
        hudObject.SetActive(hasGun);

        if (!hasGun) {
            return;
        }

        ApplyTextColor();
        ammunitionText.text = $": {activeGun.currentAmmo} / {inventory.GetAllAmmo(activeGun)}";
        ammunitionText.SetVerticesDirty();
        ammunitionText.SetMaterialDirty();
        ammunitionText.SetAllDirty();
    }
}

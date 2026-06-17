using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zeigt die Munition der aktuell ausgeruesteten Waffe an.
/// </summary>
public class AmmunitionHudDisplay : MonoBehaviour {
    // Spieler-Waffenauswahl, aus der die aktuelle Waffe gelesen wird.
    [SerializeField] private PlayerWeaponSelector weaponSelector;

    // Munition-Icon aus dem Projekt.
    [SerializeField] private Sprite ammunitionIcon;

    // Feste Position auf dem Bildschirm.
    [SerializeField] private Vector2 hudPosition = new Vector2(20f, -145f);

    // Groesse vom Icon.
    [SerializeField] private Vector2 iconSize = new Vector2(42f, 42f);

    // Farbe vom Munitionstext.
    [SerializeField] private Color textColor = new Color(0.85f, 0.95f, 1f);

    private RectTransform hudRect;
    private Text ammunitionText;
    private GameObject hudObject;

    private void Awake() {
        RemoveOldRuntimeHudCanvases();
        HideOldImageOnThisObject();
        FindWeaponSelectorIfMissing();
        CreateHud();
    }

    private void Update() {
        FindWeaponSelectorIfMissing();
        UpdateHudPosition();
        UpdateAmmunitionText();
    }

    private void FindWeaponSelectorIfMissing() {
        // Sucht die Waffenauswahl automatisch, falls sie im Inspector leer ist.
        if (weaponSelector != null) {
            return;
        }

        weaponSelector = FindFirstObjectByType<PlayerWeaponSelector>();
    }

    private void RemoveOldRuntimeHudCanvases() {
        // Entfernt alte automatisch erstellte Ammo-HUDs, damit nicht mehrere Anzeigen uebereinander liegen.
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases) {
            if (canvas != null && canvas.gameObject.name == "Ammunition HUD Canvas") {
                Destroy(canvas.gameObject);
            }
        }
    }

    private void HideOldImageOnThisObject() {
        // Falls das Script auf einem UI-Bild liegt, wird dieses alte Bild versteckt.
        Image oldImage = GetComponent<Image>();
        if (oldImage != null) {
            oldImage.enabled = false;
        }
    }

    private void CreateHud() {
        // Erstellt ein eigenes Canvas fuer die Munitionsanzeige.
        GameObject canvasObject = new GameObject("Ammunition HUD Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        canvasScaler.scaleFactor = 1f;
        canvasScaler.referencePixelsPerUnit = 100f;

        canvasObject.AddComponent<GraphicRaycaster>();

        // Container fuer Icon und Text.
        hudObject = new GameObject("Ammunition HUD");
        hudObject.transform.SetParent(canvas.transform, false);

        hudRect = hudObject.AddComponent<RectTransform>();
        hudRect.anchorMin = new Vector2(0f, 1f);
        hudRect.anchorMax = new Vector2(0f, 1f);
        hudRect.pivot = new Vector2(0f, 1f);
        hudRect.anchoredPosition = hudPosition;
        hudRect.sizeDelta = new Vector2(150f, 46f);

        // Dunkler Hintergrund, damit man die Anzeige gut lesen kann.
        Image background = hudObject.AddComponent<Image>();
        background.color = new Color(0.04f, 0.06f, 0.05f, 0.72f);
        background.raycastTarget = false;

        // Icon links in der Anzeige.
        GameObject iconObject = new GameObject("Ammunition Icon");
        iconObject.transform.SetParent(hudObject.transform, false);

        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0f, 0.5f);
        iconRect.anchorMax = new Vector2(0f, 0.5f);
        iconRect.pivot = new Vector2(0f, 0.5f);
        iconRect.anchoredPosition = new Vector2(8f, 0f);
        iconRect.sizeDelta = iconSize;

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.sprite = ammunitionIcon;
        iconImage.enabled = ammunitionIcon != null;
        iconImage.color = Color.white;
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        // Text rechts neben dem Icon.
        GameObject textObject = new GameObject("Ammunition Count");
        textObject.transform.SetParent(hudObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 0f);
        textRect.anchorMax = new Vector2(1f, 1f);
        textRect.offsetMin = new Vector2(46f, 0f);
        textRect.offsetMax = new Vector2(-4f, 0f);

        ammunitionText = textObject.AddComponent<Text>();
        ammunitionText.alignment = TextAnchor.MiddleLeft;
        ammunitionText.fontSize = 23;
        ammunitionText.fontStyle = FontStyle.Bold;
        ammunitionText.color = textColor;
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

    private void UpdateHudPosition() {
        // Haelt die Anzeige an einer festen Position.
        if (hudRect == null) {
            return;
        }

        hudRect.anchoredPosition = hudPosition;
    }

    private void UpdateAmmunitionText() {
        // Wenn keine Waffe ausgeruestet ist, wird die Anzeige versteckt.
        if (hudObject == null || ammunitionText == null) {
            return;
        }

        GunSO activeGun = weaponSelector != null ? weaponSelector.activeGun : null;
        bool hasGun = activeGun != null;
        hudObject.SetActive(hasGun);

        if (!hasGun) {
            return;
        }

        // Setzt die Farbe jedes Frame, damit der Inspector-Wert immer uebernommen wird.
        ammunitionText.color = textColor;
        ammunitionText.text = $": {activeGun.GetCurrentAmmo()} / {activeGun.GetMaxAmmo()}";
        ammunitionText.SetAllDirty();
    }
}

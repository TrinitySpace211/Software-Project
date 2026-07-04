using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

/// <summary>
/// Displays a loot chest and allows it to be opened with F.
/// </summary>
public class LootChest : MonoBehaviour {
    private const string PromptText = "Press F to open chest";

    [Header("References")]
    // Player and camera can be assigned in the Inspector.
    // Empty fields are found automatically.
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;

    [Header("Interaction")]
    // Distance and key used to open the chest.
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private Key interactionKey = Key.F;

    [Header("Visual Feedback")]
    // Position, color, and intensity of the hint and light.
    [SerializeField] private Vector3 textOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private Vector3 lightOffset = new Vector3(0f, 0.7f, 0f);
    [SerializeField] private Color neverOpenedColor = new Color(1f, 0.86f, 0.18f);
    [SerializeField] private Color openedColor = new Color(0.25f, 1f, 0.45f);
    [SerializeField] private float lightRange = 3f;
    [SerializeField] private float lightIntensity = 2.5f;
    [SerializeField] private float pulseSpeed = 4f;

    private Text hintText;
    private Light lootLight;
    private bool playerInRange;
    private bool hasBeenOpened;
    private bool isCurrentlyOpen;
    private float textBaseScale;

    private void Awake() {
        FindMissingReferences();
        CreateHintText();
        CreateLootLight();
    }

    private void Update() {
        FindMissingReferences();
        CheckPlayerDistance();
        ResetOpenStateAfterPlayerLeaves();
        UpdateHintText();
        UpdateLootLight();
        CheckInteractionInput();
    }

    private void FindMissingReferences() {
        // Finds the player automatically by using the Player tag.
        if (player == null) {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) {
                player = playerObject.transform;
            }
        }

        // Finds the camera so the text can face it.
        if (playerCamera == null) {
            playerCamera = Camera.main;
        }
    }

    private void CreateHintText() {
        // Creates a small hint above the chest.
        GameObject canvasObject = new GameObject("LootHint", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform);
        canvasObject.transform.localPosition = textOffset;
        canvasObject.transform.localRotation = Quaternion.identity;

        textBaseScale = 0.003f;
        canvasObject.transform.localScale = Vector3.one * textBaseScale;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(360f, 50f);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = playerCamera;
        canvas.sortingOrder = 20;

        GameObject textObject = new GameObject("HintText", typeof(RectTransform));
        textObject.transform.SetParent(canvasObject.transform);
        textObject.transform.localPosition = Vector3.zero;
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = Vector3.one;

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = new Vector2(360f, 50f);

        hintText = textObject.AddComponent<Text>();
        hintText.text = PromptText;
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.fontSize = 20;
        hintText.color = Color.white;
        hintText.raycastTarget = false;

        Font builtInFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (builtInFont == null) {
            builtInFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        hintText.font = builtInFont;
        hintText.horizontalOverflow = HorizontalWrapMode.Overflow;
        hintText.verticalOverflow = VerticalWrapMode.Overflow;
        hintText.gameObject.SetActive(false);
    }

    private void CreateLootLight() {
        // Creates the light so the loot chest stands out.
        GameObject lightObject = new GameObject("LootLight");
        lightObject.transform.SetParent(transform);
        lightObject.transform.localPosition = lightOffset;
        lightObject.transform.localRotation = Quaternion.identity;

        lootLight = lightObject.AddComponent<Light>();
        lootLight.type = LightType.Point;
        lootLight.color = neverOpenedColor;
        lootLight.range = lightRange;
        lootLight.intensity = lightIntensity;
        lootLight.shadows = LightShadows.None;
    }

    private void CheckPlayerDistance() {
        // Checks whether the player is close enough to the chest.
        if (player == null) {
            playerInRange = false;
            return;
        }

        playerInRange = Vector3.Distance(player.position, transform.position) <= interactionDistance;
    }

    private void ResetOpenStateAfterPlayerLeaves() {
        // After opening, the light stays off while the player remains nearby.
        // The chest can be used again after the player leaves.
        if (isCurrentlyOpen && !playerInRange) {
            isCurrentlyOpen = false;
        }
    }

    private void UpdateHintText() {
        if (hintText == null) {
            return;
        }

        // The hint always faces the camera.
        Transform textRoot = hintText.transform.parent;
        if (playerCamera != null) {
            Vector3 lookDirection = textRoot.position - playerCamera.transform.position;
            if (lookDirection.sqrMagnitude > 0.001f) {
                textRoot.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        hintText.text = PromptText;
        hintText.gameObject.SetActive(playerInRange && !isCurrentlyOpen);
    }

    private void UpdateLootLight() {
        if (lootLight == null) {
            return;
        }

        // The light turns off immediately after opening.
        if (isCurrentlyOpen) {
            lootLight.enabled = false;
            return;
        }

        // Never opened before: yellow light.
        // In range or already opened: green light.
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.25f;
        lootLight.enabled = true;
        lootLight.color = (playerInRange || hasBeenOpened) ? openedColor : neverOpenedColor;
        lootLight.range = lightRange;
        lootLight.intensity = lightIntensity * pulse;
    }

    /// <summary>
    /// Resets the chest display for a new day.
    /// </summary>
    public void ResetForNewDay() {
        hasBeenOpened = false;
        isCurrentlyOpen = false;
        UpdateLootLight();
    }

    private void CheckInteractionInput() {
        // The player can open a closed chest while in range.
        if (isCurrentlyOpen || !playerInRange || Keyboard.current == null) {
            return;
        }

        KeyControl keyControl = Keyboard.current[interactionKey];
        if (keyControl != null && keyControl.wasPressedThisFrame) {
            OpenChest();
        }
    }

    private void OpenChest() {
        // Remembers that the chest has just been opened.
        hasBeenOpened = true;
        isCurrentlyOpen = true;
        Debug.Log($"Loot chest '{name}' was opened.");
    }

    /* private void OnDrawGizmosSelected() {
        // Shows the range and text height in the editor.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        Gizmos.DrawLine(transform.position, transform.position + textOffset);
    } */
}

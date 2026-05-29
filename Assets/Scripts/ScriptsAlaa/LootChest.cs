using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

/// <summary>
/// Makes a chest recognizable as lootable and allows the player to open it with the E key.
/// </summary>
public class LootChest : MonoBehaviour
{
    private const string PromptText = "E drücken, um Kiste zu öffnen";
    private const string OpenedText = "Geöffnet";

    [Header("References")]
    // Player and camera can be assigned in the Inspector.
    // Player and camera can be assigned in the Inspector.
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;

    [Header("Interaction")]
    // Distance and key used for interaction.
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private Key interactionKey = Key.E;

    [Header("Visual Feedback")]
    // Position, color, and intensity of the hint and glow.
    [SerializeField] private Vector3 textOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private Vector3 lightOffset = new Vector3(0f, 0.7f, 0f);
    [SerializeField] private Color lootColor = new Color(1f, 0.86f, 0.18f);
    [SerializeField] private Color interactColor = new Color(0.25f, 1f, 0.45f);
    [SerializeField] private float lightRange = 3f;
    [SerializeField] private float lightIntensity = 2.5f;
    [SerializeField] private float pulseSpeed = 4f;

    private Text hintText;
    private Light lootLight;
    private bool playerInRange;
    private bool isOpened;
    private float textBaseScale;

    private void Awake()
    {
        FindMissingReferences();
        CreateHintText();
        CreateLootLight();
    }

    private void Update()
    {
        FindMissingReferences();
        CheckPlayerDistance();
        UpdateHintText();
        UpdateLootLight();
        CheckInteractionInput();
    }

    private void FindMissingReferences()
    {
        // Automatically find the player using the "Player" tag..
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        // Automatically find the camera so the text can face it.
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void CreateHintText()
    {
        // Creates a small text above the chest.
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
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.horizontalOverflow = HorizontalWrapMode.Overflow;
        hintText.verticalOverflow = VerticalWrapMode.Overflow;
        hintText.gameObject.SetActive(false);
    }

    private void CreateLootLight()
    {
        // Creates the light so the chest stands out as lootable..
        GameObject lightObject = new GameObject("LootLight");
        lightObject.transform.SetParent(transform);
        lightObject.transform.localPosition = lightOffset;
        lightObject.transform.localRotation = Quaternion.identity;

        lootLight = lightObject.AddComponent<Light>();
        lootLight.type = LightType.Point;
        lootLight.color = lootColor;
        lootLight.range = lightRange;
        lootLight.intensity = lightIntensity;
        lootLight.shadows = LightShadows.None;
    }

    private void CheckPlayerDistance()
    {
        // Checks whether the player is close enough to the chest.
        if (player == null || isOpened)
        {
            playerInRange = false;
            return;
        }

        playerInRange = Vector3.Distance(player.position, transform.position) <= interactionDistance;
    }

    private void UpdateHintText()
    {
        if (hintText == null)
        {
            return;
        }

        //  The text always faces the camera.
        Transform textRoot = hintText.transform.parent;
        if (playerCamera != null)
        {
            Vector3 lookDirection = textRoot.position - playerCamera.transform.position;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                textRoot.rotation = Quaternion.LookRotation(lookDirection);
            }
        }

        if (isOpened)
        {
            hintText.text = OpenedText;
            hintText.gameObject.SetActive(false);
            return;
        }

        hintText.text = PromptText;
        hintText.gameObject.SetActive(playerInRange);
    }

    private void UpdateLootLight()
    {
        if (lootLight == null)
        {
            return;
        }

        // Turn off the light after the chest has been opened.
        if (isOpened)
        {
            lootLight.enabled = false;
            return;
        }

        // The light pulses slightly. It turns green when the player is in range.
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.25f;
        lootLight.color = playerInRange ? interactColor : lootColor;
        lootLight.range = lightRange;
        lootLight.intensity = lightIntensity * pulse;
    }

    private void CheckInteractionInput()
    {
        // The player can only open the chest once and only while in range.
        if (isOpened || !playerInRange || Keyboard.current == null)
        {
            return;
        }

        KeyControl keyControl = Keyboard.current[interactionKey];
        if (keyControl != null && keyControl.wasPressedThisFrame)
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        //  Marks the chest as opened.
        isOpened = true;
        Debug.Log($"Loot chest '{name}' was opened.");
    }

    private void OnDrawGizmosSelected()
    {
        // Displays the interaction range and text height in the editor.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        Gizmos.DrawLine(transform.position, transform.position + textOffset);
    }
}

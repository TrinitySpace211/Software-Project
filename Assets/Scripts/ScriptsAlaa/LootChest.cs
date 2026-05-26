using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

/// <summary>
/// Macht eine Kiste als lootbar erkennbar und erlaubt dem Spieler, sie mit E zu öffnen.
/// </summary>
public class LootChest : MonoBehaviour
{
    private const string PromptText = "E drücken, um Kiste zu öffnen";
    private const string OpenedText = "Geöffnet";

    [Header("References")]
    // Player und Kamera können im Inspector gesetzt werden.
    // Falls sie leer sind, sucht das Script sie automatisch.
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;

    [Header("Interaction")]
    // Entfernung und Taste für die Interaktion.
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private Key interactionKey = Key.E;

    [Header("Visual Feedback")]
    // Position, Farbe und Stärke vom Hinweis und vom Leuchten.
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
        // Player automatisch über den Tag "Player" suchen.
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        // Kamera automatisch suchen, damit der Text zur Kamera schauen kann.
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void CreateHintText()
    {
        // Erstellt einen kleinen Text über der Kiste.
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
        // Erstellt das Licht, damit die Kiste als lootbar auffällt.
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
        // Prüft, ob der Spieler nah genug an der Kiste ist.
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

        // Der Text schaut immer zur Kamera.
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

        // Nach dem Öffnen geht das Licht aus.
        if (isOpened)
        {
            lootLight.enabled = false;
            return;
        }

        // Das Licht pulsiert leicht. In Reichweite wird es grün.
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * 0.25f;
        lootLight.color = playerInRange ? interactColor : lootColor;
        lootLight.range = lightRange;
        lootLight.intensity = lightIntensity * pulse;
    }

    private void CheckInteractionInput()
    {
        // Der Spieler kann nur in Reichweite und nur einmal öffnen.
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
        // Merkt sich, dass die Kiste geöffnet wurde.
        isOpened = true;
        Debug.Log($"Loot chest '{name}' was opened.");
    }

    private void OnDrawGizmosSelected()
    {
        // Zeigt im Editor die Interaktionsreichweite und die Text-Höhe an.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);
        Gizmos.DrawLine(transform.position, transform.position + textOffset);
    }
}

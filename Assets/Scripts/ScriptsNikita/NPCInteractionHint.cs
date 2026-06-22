using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows an interaction hint above the NPC when the player is close enough.
/// The hint disappears when the NPC dialog is open.
/// </summary>
public class NPCInteractionHint : MonoBehaviour {

    /// <summary>
    /// Text that will be shown above the NPC.
    /// </summary>
    private const string PromptText = "Press F to speak";

    /// <summary>
    /// Player and camera can be assigned in the Inspector.
    /// // If they are empty, the script tries to find them automatically.
    /// </summary>
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;

    /// <summary>
    /// Maximum distance at which the text should be visible.
    /// </summary>
    [SerializeField] private float interactionDistance = 2.5f;

    /// <summary>
    /// Maximum distance at which the text should be visible.
    /// </summary>
    [SerializeField] private Vector3 textOffset = new Vector3(0f, 2.2f, 0f);

    /// <summary>
    /// Reference to the NPC dialog.
    /// // Needed so the hint can be hidden while the dialog is open.
    /// </summary>
    [SerializeField] private NPCDialog npcDialog;

    /// <summary>
    /// Runtime-created canvas object that contains the hint text.
    /// </summary>
    private GameObject canvasObject;

    /// <summary>
    /// Text component that displays the interaction hint.
    /// </summary>
    private Text hintText;

    private void Awake() {

        // If no NPCDialog was assigned in the Inspector,
        // try to find it on the same GameObject.
        if (npcDialog == null) {
            npcDialog = GetComponent<NPCDialog>();
        }

        FindReferences();
        CreateText();
    }

    private void Update() {
        FindReferences();
        UpdateVisibility();
        LookAtCamera();
    }

    /// <summary>
    /// Finds missing references automatically.
    /// This is useful if Player or Camera were not assigned in the Inspector.
    /// </summary>
    private void FindReferences() {
        // Find the player by using the "Player" tag.
        if (player == null) {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null) {
                player = playerObject.transform;
            }
        }

        // Find the main camera.
        if (playerCamera == null) {
            playerCamera = Camera.main;
        }
    }

    /// <summary>
    /// Creates a small world-space canvas with a text above the NPC.
    /// </summary>
    private void CreateText() {
        // Create a new GameObject with RectTransform and Canvas components.
        canvasObject = new GameObject("NPCInteractionHint", typeof(RectTransform), typeof(Canvas));

        // Parent the canvas to the NPC.
        // This makes the text move together with the NPC.
        canvasObject.transform.SetParent(transform);

        // Position the text above the NPC.
        canvasObject.transform.localPosition = textOffset;
        canvasObject.transform.localRotation = Quaternion.identity;

        // Scale the canvas down because world-space UI is very large by default.
        canvasObject.transform.localScale = Vector3.one * 0.0045f;

        // Set up the canvas as world-space canvas.
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = playerCamera;

        // Set the size of the canvas area.
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(500f, 80f);

        // Create a child object for the actual text.
        GameObject textObject = new GameObject("HintText", typeof(RectTransform));

        // Parent the text object to the canvas.
        textObject.transform.SetParent(canvasObject.transform);
        textObject.transform.localPosition = Vector3.zero;
        textObject.transform.localRotation = Quaternion.identity;
        textObject.transform.localScale = Vector3.one;

        // Set the size of the text area.
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(500f, 80f);

        // Add a normal Unity UI Text component.
        hintText = textObject.AddComponent<Text>();

        // Set the visible text.
        hintText.text = PromptText;

        // Center the text inside the text area.
        hintText.alignment = TextAnchor.MiddleCenter;

        // Text size.
        hintText.fontSize = 30;

        // Text color.
        hintText.color = Color.white;

        // This text should not block mouse clicks or UI raycasts.
        hintText.raycastTarget = false;

        // Use Unity's built-in runtime font.
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Hide the hint at the beginning.
        canvasObject.SetActive(false);
    }

    /// <summary>
    /// Shows or hides the hint depending on player distance and dialog state.
    /// </summary>
    private void UpdateVisibility() {

        // If required references are missing, do nothing.
        if (player == null || canvasObject == null)
            return;

        // If the NPC dialog is already open, hide the interaction hint.
        if (npcDialog != null && npcDialog.IsDialogOpen) {
            canvasObject.SetActive(false);
            return;
        }

        // Calculate the distance between player and NPC.
        float distance = Vector3.Distance(player.position, transform.position);

        // Show the text only if the player is close enough.
        canvasObject.SetActive(distance <= interactionDistance);
    }

    /// <summary>
    /// Rotates the hint so it always faces the player's camera.
    /// </summary>
    private void LookAtCamera() {

        // If canvas or camera is missing, do nothing.
        if (canvasObject == null || playerCamera == null)
            return;

        // Direction from camera to the text.
        Vector3 lookDirection = canvasObject.transform.position - playerCamera.transform.position;

        // Prevent invalid rotation if direction is almost zero.
        if (lookDirection.sqrMagnitude > 0.001f) {
            canvasObject.transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
}
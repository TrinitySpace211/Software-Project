using UnityEngine;

public class NPCDialogFollow : MonoBehaviour
{

    [Header("References")]
    public Transform npc;
    public Camera playerCamera;

    [Header("Position")]
    public Vector3 offset = new Vector3(120f, 80f, 0f);

    [Header("Screen Clamp")]
    public float screenMargin = 30f;

    [Header("State")]
    public bool isFrozen = false;

    private RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();

        if (playerCamera == null) {
            playerCamera = Camera.main;
        }
    }

    private void Update() {
        // If the dialog position is frozen, do not update it anymore.
        if (isFrozen)
            return;

        UpdatePosition();
    }

    /// <summary>
    /// Updates the dialog position based on the NPC screen position.
    /// </summary>
    public void UpdatePosition() {
        if (npc == null || playerCamera == null || rectTransform == null)
            return;

        Vector3 screenPosition = playerCamera.WorldToScreenPoint(npc.position);

        rectTransform.position = screenPosition + offset;

        ClampToScreen();
    }

    /// <summary>
    /// Positions the dialog once and then freezes it.
    /// </summary>
    public void FreezePosition() {
        UpdatePosition();

        isFrozen = true;
    }

    /// <summary>
    /// Allows the dialog to follow the NPC again.
    /// </summary>
    public void UnfreezePosition() {
        isFrozen = false;
    }

    /// <summary>
    /// Keeps the dialog inside the visible screen area.
    /// </summary>
    private void ClampToScreen() {
        Vector3[] corners = new Vector3[4];

        rectTransform.GetWorldCorners(corners);

        float left = corners[0].x;
        float bottom = corners[0].y;
        float right = corners[2].x;
        float top = corners[2].y;

        Vector3 correction = Vector3.zero;

        if (left < screenMargin) {
            correction.x = screenMargin - left;
        }

        if (right > Screen.width - screenMargin) {
            correction.x = (Screen.width - screenMargin) - right;
        }

        if (bottom < screenMargin) {
            correction.y = screenMargin - bottom;
        }

        if (top > Screen.height - screenMargin) {
            correction.y = (Screen.height - screenMargin) - top;
        }

        rectTransform.position += correction;
    }
}

    /*
    public Transform npc;
    public Vector3 offset = new Vector3(120f, 80f, 0f);

    private RectTransform rectTransform;

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update() {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(npc.position);
        rectTransform.position = screenPos + offset;
    }
    */

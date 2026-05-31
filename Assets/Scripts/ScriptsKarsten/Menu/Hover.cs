using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Adds a hover effect to a UI button, including
/// color overlay changes and scale animation on pointer enter/exit.
/// </summary>
public class ButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private RectTransform buttonRoot;
    [SerializeField] private Image hoverOverlay;

    [SerializeField] private Color overlayNormalColor = new Color(0.25f, 0.01f, 0.01f, 0f);
    [SerializeField] private Color overlayHoverColor = new Color(0.35f, 0.02f, 0.02f, 0.35f);

    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.06f, 1.06f, 1.06f);

    /// <summary>
    /// Initializes references and resets the visual state.
    /// </summary>
    private void Awake() {
        if (buttonRoot == null)
            buttonRoot = GetComponent<RectTransform>();

        if (hoverOverlay == null)
            hoverOverlay = GetComponentInChildren<Image>();

        ResetVisualState();
    }

    /// <summary>
    /// Resets visual state when the object is disabled.
    /// </summary>
    private void OnDisable() {
        ResetVisualState();
    }

    /// <summary>
    /// Resets the button to its default scale and overlay color.
    /// </summary>
    private void ResetVisualState() {
        if (hoverOverlay != null)
            hoverOverlay.color = overlayNormalColor;

        if (buttonRoot != null)
            buttonRoot.localScale = normalScale;
    }

    /// <summary>
    /// Called when the pointer enters the button area.
    /// Applies hover visual effects (color + scale).
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        if (hoverOverlay != null)
            hoverOverlay.color = overlayHoverColor;

        if (buttonRoot != null)
            buttonRoot.localScale = hoverScale;
    }

    /// <summary>
    /// Called when the pointer exits the button area.
    /// Restores the default visual state.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        ResetVisualState();
    }
}
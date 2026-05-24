using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HorrorButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] private RectTransform buttonRoot;
    [SerializeField] private Image hoverOverlay;

    [SerializeField] private Color overlayNormalColor = new Color(0.25f, 0.01f, 0.01f, 0f);
    [SerializeField] private Color overlayHoverColor = new Color(0.35f, 0.02f, 0.02f, 0.35f);

    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 hoverScale = new Vector3(1.03f, 1.03f, 1.03f);

    private void Awake() {
        if (buttonRoot == null)
            buttonRoot = GetComponent<RectTransform>();

        if (hoverOverlay == null)
            hoverOverlay = GetComponentInChildren<Image>();

        if (hoverOverlay != null)
            hoverOverlay.color = overlayNormalColor;

        if (buttonRoot != null)
            buttonRoot.localScale = normalScale;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (hoverOverlay != null)
            hoverOverlay.color = overlayHoverColor;

        if (buttonRoot != null)
            buttonRoot.localScale = hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (hoverOverlay != null)
            hoverOverlay.color = overlayNormalColor;

        if (buttonRoot != null)
            buttonRoot.localScale = normalScale;
    }
}
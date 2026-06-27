using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Represents a single inventory or hotbar slot.
/// Handles item storage, UI updates, and mouse hover detection.
/// </summary>
public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    /// <summary>
    /// Indicates whether the mouse cursor is currently hovering over this slot.
    /// </summary>
    public bool hovering;

    /// <summary>
    /// Indicates if a hotbar slot is currently selected through the key buttons 1-5.
    /// </summary>
    public bool selected;

    [SerializeField] private TextMeshProUGUI slotNumberTxt;

    /// <summary>
    /// The item currently stored in this slot.
    /// </summary>
    private ItemSO heldItem;

    /// <summary>
    /// The amount of items currently stored in this slot.
    /// </summary>
    private int itemAmount;

    /// <summary>
    /// UI image used to display the item's icon.
    /// </summary>
    private Image iconImage;

    /// <summary>
    /// UI text used to display the item amount.
    /// </summary>
    private TextMeshProUGUI amountTxt;

    /// <summary>
    /// The RectTransform component of this slot.
    /// </summary>
    private RectTransform rectTransform;

    /// <summary>
    /// Initializes references to UI components used by the slot.
    /// </summary>
    private void Awake() {
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update() {
        float targetScale = selected ? 1.1f : 1f;
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, Vector3.one * targetScale, Time.deltaTime * 10f);
    }

    /// <summary>
    /// Returns the item currently stored in this slot.
    /// </summary>
    public ItemSO GetItem() {
        return heldItem;
    }

    /// <summary>
    /// Returns the current amount of items in this slot.
    /// </summary>
    public int GetAmount() {
        return itemAmount;
    }

    /// <summary>
    /// Assigns an item and amount to this slot.
    /// Updates the slot UI afterwards.
    /// </summary>
    public void SetItem(ItemSO item, int amount = 1) {
        heldItem = item;
        itemAmount = amount;
        UpdateSlot();
    }

    /// <summary>
    /// Sets whether the slot is selected.
    /// </summary>
    public void SetSelected(bool state) {
        selected = state;
    }

    /// <summary>
    /// Returns whether the slot is selected.
    /// </summary>
    public bool GetSelected() {
        return selected;
    }

    /// <summary>
    /// Updates the slot UI based on the current item data.
    /// </summary>
    public void UpdateSlot() {
        if (heldItem != null) {
            iconImage.enabled = true;
            iconImage.sprite = heldItem.icon;

            if (amountTxt != null)
                amountTxt.text = itemAmount > 1 ? itemAmount.ToString() : "";
        } else {
            iconImage.enabled = false;

            if (amountTxt != null)
                amountTxt.text = "";
        }
    }

    /// <summary>
    /// Adds a specified amount to the current item stack.
    /// </summary>
    public int AddAmount(int amountToAdd) {
        itemAmount += amountToAdd;
        UpdateSlot();
        return itemAmount;
    }

    /// <summary>
    /// Removes a specified amount from the current item stack.
    /// Clears the slot if the amount reaches zero or below.
    /// </summary>
    public int RemoveAmount(int amountToRemove) {
        itemAmount -= amountToRemove;

        if (itemAmount <= 0) {
            ClearSlot();
        } else {
            UpdateSlot();
        }

        return itemAmount;
    }

    /// <summary>
    /// Clears the slot by removing the item and resetting the amount.
    /// </summary>
    public void ClearSlot() {
        heldItem = null;
        itemAmount = 0;
        UpdateSlot();
    }

    /// <summary>
    /// Checks whether this slot currently contains an item.
    /// </summary>
    public bool HasItem() {
        return heldItem != null;
    }

    /// <summary>
    /// Called when the mouse pointer enters this slot.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData) {
        hovering = true;
    }

    /// <summary>
    /// Called when the mouse pointer exits this slot.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData) {
        hovering = false;
    }
}
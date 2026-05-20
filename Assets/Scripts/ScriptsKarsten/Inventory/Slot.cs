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
    /// Initializes references to UI components used by the slot.
    /// </summary>
    private void Awake() {
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Returns the item currently stored in this slot.
    /// </summary>
    /// <returns>
    /// The stored <see cref="ItemSO"/>, or null if the slot is empty.
    /// </returns>
    public ItemSO GetItem() {
        return heldItem;
    }

    /// <summary>
    /// Returns the current amount of items in this slot.
    /// </summary>
    /// <returns>
    /// The item quantity.
    /// </returns>
    public int GetAmount() {
        return itemAmount;
    }

    /// <summary>
    /// Assigns an item and amount to this slot.
    /// Updates the slot UI afterwards.
    /// </summary>
    /// <param name="item">
    /// The item to place into the slot.
    /// </param>
    /// <param name="amount">
    /// The amount of items to store. Defaults to 1.
    /// </param>
    public void SetItem(ItemSO item, int amount = 1) {
        heldItem = item;
        itemAmount = amount;

        UpdateSlot();
    }

    /// <summary>
    /// Updates the slot UI based on the current item data.
    /// Displays the icon and amount if an item exists,
    /// otherwise clears the UI.
    /// </summary>
    public void UpdateSlot() {
        if (heldItem != null) {
            iconImage.enabled = true;
            iconImage.sprite = heldItem.icon;
            amountTxt.text = itemAmount.ToString();
        } else {
            iconImage.enabled = false;
            amountTxt.text = "";
        }
    }

    /// <summary>
    /// Adds a specified amount to the current item stack.
    /// </summary>
    /// <param name="amountToAdd">
    /// The amount to add.
    /// </param>
    /// <returns>
    /// The updated item amount.
    /// </returns>
    public int AddAmount(int amountToAdd) {
        itemAmount += amountToAdd;
        UpdateSlot();

        return itemAmount;
    }

    /// <summary>
    /// Removes a specified amount from the current item stack.
    /// Clears the slot if the amount reaches zero or below.
    /// </summary>
    /// <param name="amountToRemove">
    /// The amount to remove.
    /// </param>
    /// <returns>
    /// The updated item amount.
    /// </returns>
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
    /// Clears the slot by removing the item
    /// and resetting the amount to zero.
    /// </summary>
    public void ClearSlot() {
        heldItem = null;
        itemAmount = 0;
        UpdateSlot();
    }

    /// <summary>
    /// Checks whether this slot currently contains an item.
    /// </summary>
    /// <returns>
    /// True if the slot contains an item; otherwise false.
    /// </returns>
    public bool HasItem() {
        return heldItem != null;
    }

    /// <summary>
    /// Called when the mouse pointer enters this slot.
    /// Sets the hovering state to true.
    /// </summary>
    /// <param name="eventData">
    /// Event data associated with the pointer event.
    /// </param>
    public void OnPointerEnter(PointerEventData eventData) {
        hovering = true;
    }

    /// <summary>
    /// Called when the mouse pointer exits this slot.
    /// Sets the hovering state to false.
    /// </summary>
    /// <param name="eventData">
    /// Event data associated with the pointer event.
    /// </param>
    public void OnPointerExit(PointerEventData eventData) {
        hovering = false;
    }
}
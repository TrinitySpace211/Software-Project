using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

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
    /// Indicates if a hotbar slot ist currently selected through the key buttons 1-5
    /// </summary>
    public bool selected;

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
    /// The RectTransform component of this slot
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
    /// If set to true the Slot scale will be increased to 1.1 so that it gives the player the feedback, 
    /// that this hotbar slot has been selected
    /// </summary>
    /// <param name="state">If the slot is selected or not</param>
    public void SetSelected(bool state) {
        selected = state;
    }

    /// <summary>
    /// returns if this slot has been selected via input key 1-5
    /// </summary>
    /// <returns>the selected state</returns>
    public bool GetSelected() {
        return selected;
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
            if (itemAmount > 1) {
                amountTxt.text = itemAmount.ToString();
            } else if (itemAmount == 1) {
                amountTxt.text = "";
            }

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
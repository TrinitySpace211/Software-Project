using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using Image = UnityEngine.UI.Image;

/// <summary>
/// Manages the player's inventory system, including item storage,
/// hotbar management, drag-and-drop functionality, and inventory UI handling.
/// </summary>
public class Inventory : MonoBehaviour {
    /// <summary>
    /// Test item used for adding wood to the inventory.
    /// </summary>
    public ItemSO woodItem;

    /// <summary>
    /// Test item used for adding an axe to the inventory.
    /// </summary>
    public ItemSO axeItem;

    /// <summary>
    /// Reference to the hotbar object containing hotbar slots.
    /// </summary>
    public GameObject hotbarObj;

    /// <summary>
    /// Parent object containing all inventory slots.
    /// </summary>
    public GameObject inventorySlotParent;

    /// <summary>
    /// Root inventory UI container.
    /// </summary>
    public GameObject container;

    /// <summary>
    /// UI image used to display the dragged item's icon.
    /// </summary>
    public Image dragIcon;

    /// <summary>
    /// List of all inventory slots.
    /// </summary>
    private List<Slot> inventorySlots = new List<Slot>();

    /// <summary>
    /// List of all hotbar slots.
    /// </summary>
    private List<Slot> hotbarSlots = new List<Slot>();

    /// <summary>
    /// Combined list of all available slots.
    /// </summary>
    private List<Slot> allSlots = new List<Slot>();

    /// <summary>
    /// The slot currently being dragged.
    /// </summary>
    private Slot draggedSlot = null;

    /// <summary>
    /// Indicates whether an item is currently being dragged.
    /// </summary>
    private bool isDragging = false;

    /// <summary>
    /// Indicates whether the inventory is currently open (paused state).
    /// </summary>
    private bool isPaused = false;

    /// <summary>
    /// Initializes slot collections by retrieving
    /// inventory and hotbar slots from child objects.
    /// </summary>
    private void Awake() {
        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
    }

    /// <summary>
    /// Hides the inventory container at startup.
    /// </summary>
    private void Start() {
        container.SetActive(false);
    }

    /// <summary>
    /// Handles input for testing item additions, inventory toggling,
    /// and drag-and-drop interactions.
    /// </summary>
    private void Update() {
        if (Keyboard.current.bKey.wasPressedThisFrame) {
            AddItem(woodItem, 3);
        } else if (Keyboard.current.nKey.wasPressedThisFrame) {
            AddItem(axeItem, 1);
        }

        if (Keyboard.current.iKey.wasPressedThisFrame) {
            container.SetActive(!container.activeInHierarchy);

            // Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
            // ? CursorLockMode.None
            // : CursorLockMode.Locked;

            Cursor.visible = !Cursor.visible;
            TogglePause();
        }

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();
    }

    /// <summary>
    /// Adds an item to the inventory.
    /// Existing stacks are filled first before empty slots are used.
    /// </summary>
    /// <param name="itemToAdd">The item to add.</param>
    /// <param name="amount">The amount of the item to add.</param>
    public void AddItem(ItemSO itemToAdd, int amount) {
        int remaining = amount;

        // Fill existing stacks first
        foreach (Slot slot in allSlots) {
            if (slot.HasItem() && slot.GetItem() == itemToAdd) {
                int currentAmount = slot.GetAmount();
                int maxStack = itemToAdd.maxStackSize;

                if (currentAmount < maxStack) {
                    int spaceLeft = maxStack - currentAmount;
                    int amountToAdd = Mathf.Min(spaceLeft, remaining);

                    slot.SetItem(itemToAdd, currentAmount + amountToAdd);
                    remaining -= amountToAdd;

                    if (remaining <= 0)
                        return;
                }
            }
        }

        // Place remaining items into empty slots
        foreach (Slot slot in allSlots) {
            if (!slot.HasItem()) {
                int amountToPlace = Mathf.Min(itemToAdd.maxStackSize, remaining);
                slot.SetItem(itemToAdd, amountToPlace);
                remaining -= amountToPlace;

                if (remaining <= 0)
                    return;
            }
        }

        if (remaining > 0) {
            Debug.Log(
                $"Inventory is full. Could not add {remaining}x {itemToAdd.itemName}."
            );
        }
    }

    /// <summary>
    /// Starts dragging an item if the player clicks on a slot containing an item.
    /// </summary>
    private void StartDrag() {
        if (Mouse.current.leftButton.wasPressedThisFrame) {
            Slot hovered = GetHoveredSlot();

            if (hovered != null && hovered.HasItem()) {
                draggedSlot = hovered;
                isDragging = true;

                dragIcon.sprite = hovered.GetItem().icon;
                dragIcon.color = new Color(1, 1, 1, 0.5f);
                dragIcon.enabled = true;
            }
        }
    }

    /// <summary>
    /// Ends the drag operation and attempts to place
    /// the dragged item into the hovered slot.
    /// </summary>
    private void EndDrag() {
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging) {
            Slot hovered = GetHoveredSlot();
            
            if (hovered != null) {
                HandleDrop(draggedSlot, hovered);

                dragIcon.enabled = false;
                draggedSlot = null;
                isDragging = false;
            }
        }
    }

    /// <summary>
    /// Returns the currently hovered slot.
    /// </summary>
    /// <returns>
    /// The hovered <see cref="Slot"/> or null if no slot is hovered.
    /// </returns>
    private Slot GetHoveredSlot() {
        foreach (Slot s in allSlots) {
            if (s.hovering)
                return s;
        }

        return null;
    }

    /// <summary>
    /// Handles item transfer logic between slots,
    /// including stacking, swapping, and moving items.
    /// </summary>
    /// <param name="from">The source slot.</param>
    /// <param name="to">The destination slot.</param>
    private void HandleDrop(Slot from, Slot to) {
        if (from == to)
            return;

        // Stack items if possible
        if (to.HasItem() && to.GetItem() == from.GetItem()) {
            int max = to.GetItem().maxStackSize;
            int space = max - to.GetAmount();

            if (space > 0) {
                int move = Mathf.Min(space, from.GetAmount());

                to.SetItem(to.GetItem(), to.GetAmount() + move);
                from.SetItem(from.GetItem(), from.GetAmount() - move);

                if (from.GetAmount() <= 0)
                    from.ClearSlot();

                return;
            }
        }

        // Swap items if destination slot is occupied
        if (to.HasItem()) {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();

            to.SetItem(from.GetItem(), from.GetAmount());
            from.SetItem(tempItem, tempAmount);

            return;
        }

        // Move item into empty slot
        to.SetItem(from.GetItem(), from.GetAmount());
        from.ClearSlot();
    }

    /// <summary>
    /// Updates the drag icon position to follow the mouse cursor.
    /// </summary>
    private void UpdateDragItemPosition() {
        if (isDragging) {
            dragIcon.transform.position = Input.mousePosition;
        }
    }

    /// <summary>
    /// Toggles the inventory pause state,
    /// enabling/disabling the inventory UI and game time.
    /// </summary>
    private void TogglePause() {
        isPaused = !isPaused;

        container.SetActive(isPaused);
        Cursor.visible = isPaused;
        Time.timeScale = isPaused ? 0f : 1f;
    }
}
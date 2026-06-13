using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manages the player's inventory system, including item storage,
/// hotbar management, drag-and-drop functionality, and inventory UI handling.
/// </summary>
public class Inventory : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private List<RectTransform> hotbarSlotsRect;
    [SerializeField] private GameObject crosshair;

    [Header("Item References")]
    [SerializeField] private ItemSO[] guns;
    [SerializeField] private ItemSO[] melees;
    [SerializeField] private ItemSO[] debugItems;

    private static readonly Key[] debugKeys = new Key[] {
        Key.B,
        Key.N,
        Key.M,
        Key.V,
        Key.C,
        Key.Digit7,
        Key.Digit8,
        Key.Digit9,
        Key.Digit0,
        Key.X
    };

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
    /// Holds the previous selected hotbar slot number
    /// </summary>
    private int previousSlot = -1;

    /// <summary>
    /// Saves the Time were the Player selected a hotbar
    /// </summary>
    private float lastTimeSelected;

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
        playerInputHandler.OnHotbarSlotPressed += PlayerInputHandler_OnHotbarSlotPressed;

        container.SetActive(false);
    }

    /// <summary>
    /// Handles input for testing item additions, inventory toggling,
    /// and drag-and-drop interactions.
    /// </summary>
    private void Update() {
        for (int i = 0; i < debugItems.Length && i < debugKeys.Length; i++) {
            if (Keyboard.current[debugKeys[i]].wasPressedThisFrame) {
                AddItem(debugItems[i], 1);
            }
        }

        if (Keyboard.current.iKey.wasPressedThisFrame) {
            container.SetActive(!container.activeInHierarchy);

            // Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
            // ? CursorLockMode.None
            // : CursorLockMode.Locked;
            Cursor.visible = !Cursor.visible;
            crosshair.SetActive(!crosshair.activeSelf);

            //TogglePause();
        }

        if (EventSystem.current.IsPointerOverGameObject()) {

        }

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();
    }

    /// <summary>
    /// Triggers when the Keys 1-5 are pressed.
    /// The Hotbars get selected and if there is a Weapon or Equipment then
    /// the Player will change to this item
    /// </summary>
    /// <param name="slot">the key which represents the Hotbarslot</param>
    private void PlayerInputHandler_OnHotbarSlotPressed(int slot) {
        ItemSO item = hotbarSlots[slot].GetItem();

        if (!isDragging) {
            if (Time.time > 1f + lastTimeSelected) {
                lastTimeSelected = Time.time;
                if (previousSlot != -1) {
                    hotbarSlots[previousSlot].SetSelected(false);
                }

                hotbarSlots[slot].SetSelected(true);

                if (item == null) {
                    if (previousSlot != -1) {
                        ClearPreviousSelectionIfNeeded(hotbarSlots[previousSlot].GetItem());
                    }
                } else if (item != null) {
                    if (IsWeapon(item)) {
                        ToggleWeaponSelect(item);
                    } else if (IsMelee(item)) {
                        ToggleMeleeSelect(item);
                    }
                }

                previousSlot = slot;
            }

        }
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
        foreach (Slot slot in inventorySlots) {
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

            if (to.GetSelected()) {
                if (IsWeapon(to.GetItem())) {
                    ToggleWeaponSelect(to.GetItem());
                } else if (IsMelee(to.GetItem())) {
                    ToggleMeleeSelect(to.GetItem());
                }
            } else if (from.GetSelected()) {
                ClearPreviousSelectionIfNeeded(from.GetItem());
            }

            return;
        }

        // Move item into empty slot
        to.SetItem(from.GetItem(), from.GetAmount());

        if (to.GetSelected()) {
            if (IsWeapon(to.GetItem())) {
                ToggleWeaponSelect(to.GetItem());
            } else if (IsMelee(to.GetItem())) {
                ToggleMeleeSelect(to.GetItem());
            }
        } else if (from.GetSelected()) {
            ClearPreviousSelectionIfNeeded(from.GetItem());
        }

        from.ClearSlot();
    }

    /// <summary>
    /// Updates the drag icon position to follow the mouse cursor.
    /// </summary>
    private void UpdateDragItemPosition() {
        if (isDragging) {
            dragIcon.transform.position = Mouse.current.position.ReadValue();
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

    private void ToggleWeaponSelect(ItemSO weapon) {
        if (weapon == null) {
            player.GetPlayerGunSelector().DequipWeapon();
            return;
        }

        switch (weapon.gunType) {
            case GunType.AssaultRifle:
                player.GetPlayerGunSelector().SelectAssaultRifle();
                break;
            case GunType.Pistol:
                player.GetPlayerGunSelector().SelectPistol();
                break;
            case GunType.Shotgun:
                player.GetPlayerGunSelector().SelectShotgun();
                break;
            case GunType.Sniper:
                player.GetPlayerGunSelector().SelectSniper();
                break;
            default:
                player.GetPlayerGunSelector().DequipWeapon();
                break;
        }
    }

    private void ToggleMeleeSelect(ItemSO weapon) {
        if (weapon == null) {
            player.GetPlayerGunSelector().DequipMelee();
            return;
        }

        switch (weapon.meleeType) {
            case MeleeType.Knife:
                player.GetPlayerGunSelector().SelectKnife();
                break;
            case MeleeType.Baseball_Bat:
                player.GetPlayerGunSelector().SelectBaseball();
                break;
            case MeleeType.Crowbar:
                player.GetPlayerGunSelector().SelectCrowbar();
                break;
            case MeleeType.Hatchet:
                player.GetPlayerGunSelector().SelectHatchet();
                break;
            case MeleeType.Sword:
                player.GetPlayerGunSelector().SelectSword();
                break;
            case MeleeType.Tomahawk:
                player.GetPlayerGunSelector().SelectTomahawk();
                break;
            default:
                player.GetPlayerGunSelector().DequipMelee();
                break;
        }
    }

    private bool IsWeapon(ItemSO item) {
        return item != null && item.itemType == ItemType.Gun;
    }

    private bool IsMelee(ItemSO item) {
        return item != null && item.itemType == ItemType.Melee;
    }

    private void ClearPreviousSelectionIfNeeded(ItemSO item) {
        if (IsWeapon(item)) {
            ToggleWeaponSelect(null);
        } else if (IsMelee(item)) {
            ToggleMeleeSelect(null);
        }
    }
}
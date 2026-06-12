using System;
using System.Collections.Generic;
using UnityEngine;
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

    /// <summary>
    /// Item used for adding an assaultRifle to the inventory.
    /// </summary>
    public ItemSO assaultRifle;

    /// <summary>
    /// Item used for adding an pistol to the inventory.
    /// </summary>
    public ItemSO pistol;

    /// <summary>
    /// Item used for adding an shotgun to the inventory.
    /// </summary>
    public ItemSO shotgun;

    /// <summary>
    /// Item used for adding an sniper to the inventory.
    /// </summary>
    public ItemSO sniper;

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
        if (Keyboard.current.bKey.wasPressedThisFrame) {
            AddItem(assaultRifle, 1);
        } else if (Keyboard.current.nKey.wasPressedThisFrame) {
            AddItem(pistol, 1);
        } else if (Keyboard.current.mKey.wasPressedThisFrame) {
            AddItem(shotgun, 1);
        } else if (Keyboard.current.vKey.wasPressedThisFrame) {
            AddItem(sniper, 1);
        }

        if (Keyboard.current.iKey.wasPressedThisFrame) {
            container.SetActive(!container.activeInHierarchy);

            // Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
            // ? CursorLockMode.None
            // : CursorLockMode.Locked;

            Cursor.visible = !Cursor.visible;
            //TogglePause();
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
            if (hotbarSlots[slot].GetSelected()) {
                hotbarSlots[slot].SetSelected(false);

                if (item == assaultRifle
                    || item == pistol
                    || item == shotgun
                    || item == sniper) {
                    ToggleWeaponSelect(null);
                }
            } else {

                if (previousSlot != -1) {
                    hotbarSlots[previousSlot].SetSelected(false);
                }

                hotbarSlots[slot].SetSelected(true);

                //Check for item in hotbar then equip the item
                if (item == assaultRifle) {
                    ToggleWeaponSelect(assaultRifle);
                } else if (item == pistol) {
                    ToggleWeaponSelect(pistol);
                } else if (item == shotgun) {
                    ToggleWeaponSelect(shotgun);
                } else if (item == sniper) {
                    ToggleWeaponSelect(sniper);
                } else if (previousSlot != -1) {
                    if (hotbarSlots[previousSlot].GetItem() == assaultRifle
                        || hotbarSlots[previousSlot].GetItem() == pistol
                        || hotbarSlots[previousSlot].GetItem() == shotgun
                        || hotbarSlots[previousSlot].GetItem() == sniper) {
                        ToggleWeaponSelect(null);
                    }
                }//Add more else if statements for more options health kit, grenade

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
                if (to.GetItem() == assaultRifle) {
                    ToggleWeaponSelect(assaultRifle);
                } else if (to.GetItem() == pistol) {
                    ToggleWeaponSelect(pistol);
                } else if (to.GetItem() == shotgun) {
                    ToggleWeaponSelect(shotgun);
                } else if (to.GetItem() == sniper) {
                    ToggleWeaponSelect(sniper);
                }
            } else if (from.GetSelected()) {
                ToggleWeaponSelect(null);
            }

            return;
        }

        // Move item into empty slot
        to.SetItem(from.GetItem(), from.GetAmount());

        if (to.GetSelected()) {
            if (to.GetItem() == assaultRifle) {
                ToggleWeaponSelect(assaultRifle);
            } else if (to.GetItem() == pistol) {
                ToggleWeaponSelect(pistol);
            } else if (to.GetItem() == shotgun) {
                ToggleWeaponSelect(shotgun);
            } else if (to.GetItem() == sniper) {
                ToggleWeaponSelect(sniper);
            }
        } else if (from.GetSelected()) {
            ToggleWeaponSelect(null);
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
        if (weapon == assaultRifle) {
            player.GetPlayerGunSelector().SelectAssaultRifle();
        } else if (weapon == pistol) {
            player.GetPlayerGunSelector().SelectPistol();
        } else if (weapon == shotgun) {
            player.GetPlayerGunSelector().SelectShotgun();
        } else if (weapon == sniper) {
            player.GetPlayerGunSelector().SelectSniper();
        } else if (weapon == null) {
            player.GetPlayerGunSelector().DequipWeapon();
        }
    }

    /*
 *******************************************************************************
 * Ergänzugen für den NPC. Benötigt werden öffentliche Methoden.
 */

    /// <summary>
    /// Returns the total amount of a specific item across all inventory and hotbar slots.
    /// </summary>
    /// <param name="itemToCount">The item that should be counted.</param>
    /// <returns>The total amount of the item.</returns>
    public int GetItemAmount(ItemSO itemToCount) {
        if (itemToCount == null)
            return 0;

        int totalAmount = 0;

        foreach (Slot slot in allSlots) {
            if (slot.HasItem() && slot.GetItem() == itemToCount) {
                totalAmount += slot.GetAmount();
            }
        }

        return totalAmount;
    }

    /// <summary>
    /// Checks if the inventory contains at least a specific amount of an item.
    /// </summary>
    /// <param name="itemToCheck">The item that should be checked.</param>
    /// <param name="requiredAmount">The required amount.</param>
    /// <returns>True if enough items exist; otherwise false.</returns>
    public bool HasItemAmount(ItemSO itemToCheck, int requiredAmount) {
        return GetItemAmount(itemToCheck) >= requiredAmount;
    }

    /// <summary>
    /// Removes a specific amount of an item from the inventory.
    /// Items are removed from existing stacks until the required amount was removed.
    /// </summary>
    /// <param name="itemToRemove">The item that should be removed.</param>
    /// <param name="amountToRemove">The amount that should be removed.</param>
    /// <returns>True if the item amount could be removed; otherwise false.</returns>
    public bool RemoveItem(ItemSO itemToRemove, int amountToRemove) {
        if (itemToRemove == null)
            return false;

        if (!HasItemAmount(itemToRemove, amountToRemove))
            return false;

        int remainingAmount = amountToRemove;

        foreach (Slot slot in allSlots) {
            if (slot.HasItem() && slot.GetItem() == itemToRemove) {
                int amountInSlot = slot.GetAmount();
                int amountToTake = Mathf.Min(amountInSlot, remainingAmount);

                slot.RemoveAmount(amountToTake);
                remainingAmount -= amountToTake;

                if (remainingAmount <= 0)
                    return true;
            }
        }

        return true;
    }

}


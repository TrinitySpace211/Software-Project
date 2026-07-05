using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Manages the player's inventory system, including item storage,
/// hotbar management, drag-and-drop functionality, and inventory UI handling.
/// </summary>
public class Inventory : MonoBehaviour, ISaveable {
    private static readonly string ID = "Inventory";
    public static Inventory Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private List<RectTransform> hotbarSlotsRect;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private TooltipUI tooltipUI;

    [Header("Item References")]
    [SerializeField] private ItemSO[] guns;
    [SerializeField] private ItemSO[] melees;
    [SerializeField] private ItemSO grenade;
    [SerializeField] private ItemSO[] healthItems;
    [SerializeField] private ItemSO[] ammunitions;
    [SerializeField] private ItemSO scrap;

    [Header("Hotbar Swap Speed")]
    [SerializeField] private float swapSpeed = 0.8f;

    public PauseMenu pauseMenu;

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

    public NPCDialog nPCDialog;

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
    /// Holds the previously selected hotbar slot index.
    /// </summary>
    private int previousSlot = -1;

    /// <summary>
    /// Stores the time when the player last selected a hotbar slot.
    /// </summary>
    private float lastTimeSelected;

    /// <summary>
    /// Stores the currently selected hotbar slot.
    /// </summary>
    private Slot selectedSlot = null;

    /// <summary>
    /// Initializes slot collections by retrieving
    /// inventory and hotbar slots from child objects.
    /// </summary>
    private void Awake() {
        Instance = this;

        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());

        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);
    }

    /// <summary>
    /// Hides the inventory container at startup and sets up input event handlers.
    /// </summary>
    private void Start() {
        playerInputHandler.OnHotbarSlotPressed += PlayerInputHandler_OnHotbarSlotPressed;
        playerInputHandler.OnRightClickUIAction += PlayerInputHandler_OnRightClickUIPressed;
        Item.OnItemCollected += Item_OnItemCollected;

        container.SetActive(false);

        string savePath = Path.Combine(Application.persistentDataPath, "save.json");
        if (!File.Exists(savePath)) {
            foreach (ItemSO gun in guns) {
                if (gun.gunType == GunType.Pistol) {
                    hotbarSlots[0].SetItem(gun);
                    AddItem(ItemType.Ammunition, AmmunitionType.Ammo9mm, 30);
                    AddItem(ItemType.Melee, MeleeType.Baseball_Bat);
                    AddItem(ItemType.Consumable, HealthItemType.Bandage, 4);
                    AddItem(ItemType.Consumable, HealthItemType.HealthPack, 2);
                }
            }
        }
    }

    /// <summary>
    /// Opens the tooltip for a hovered inventory item on right-click.
    /// </summary>
    private void PlayerInputHandler_OnRightClickUIPressed() {
        foreach (Slot slot in inventorySlots) {
            if (slot.hovering && slot.GetItem() != null && slot.GetItem().gunType == GunType.None && !isDragging) {
                tooltipUI.Visibile(true);
                tooltipUI.SetSelectedSlot(slot);
            }
        }
    }

    /// <summary>
    /// Handles inventory toggling, drag-and-drop updates, and input-based UI behavior.
    /// </summary>
    private void Update() {
        if (pauseMenu.IsPaused || DebugController.Instance.GetConsoleVisibility())
            return;

        if (Keyboard.current.iKey.wasPressedThisFrame) {
            container.SetActive(!container.activeInHierarchy);

            if (!container.activeInHierarchy)
                tooltipUI.Visibile(false);

            if (container.activeInHierarchy || nPCDialog.IsDialogOpen) {
                crosshair.SetActive(false);
                Cursor.visible = true;
            } else {
                crosshair.SetActive(true);
                Cursor.visible = false;
            }
        }

        StartDrag();
        UpdateDragItemPosition();
        EndDrag();
    }

    /// <summary>
    /// Handles hotbar slot selection when the corresponding number key is pressed.
    /// The selected item is equipped if it is allowed and the player is able to switch.
    /// </summary>
    /// <param name="slot">The hotbar slot index that was pressed.</param>
    private void PlayerInputHandler_OnHotbarSlotPressed(int slot) {
        if (pauseMenu.IsPaused || DebugController.Instance.GetConsoleVisibility())
            return;

        ItemSO item = hotbarSlots[slot].GetItem();

        if (item != null) {
            if (item.itemType == ItemType.Scrap || item.itemType == ItemType.Ammunition) {
                return;
            }
        }

        if (!isDragging) {
            if (Time.time > swapSpeed + lastTimeSelected
            && !player.GetPlayerAnimation().GetIsReloading()
            && !player.GetPlayerAnimation().GetIsThrowingGrenade()
            && !player.GetIsMeleeAttacking()
            && !hotbarSlots[slot].GetSelected()) {

                lastTimeSelected = Time.time;
                if (previousSlot != -1) {
                    hotbarSlots[previousSlot].SetSelected(false);
                }

                hotbarSlots[slot].SetSelected(true);
                selectedSlot = hotbarSlots[slot];

                if (item == null) {
                    if (previousSlot != -1) {
                        ClearPreviousSelectionIfNeeded(hotbarSlots[previousSlot].GetItem());
                    }
                } else if (item != null) {
                    if (IsWeapon(item)) {
                        ToggleWeaponSelect(item);
                    } else if (IsMelee(item)) {
                        ToggleMeleeSelect(item);
                    } else if (IsGrenade(item)) {
                        ToggleGrenade(item);
                    } else if (IsConsumable(item)) {
                        ToggleConsumable(item);
                    }
                }

                previousSlot = slot;
            }

        }
    }

    /// <summary>
    /// Adds collected world items to the inventory based on their specific item type.
    /// </summary>
    /// <param name="item">The collected item instance.</param>
    private void Item_OnItemCollected(Item item) {
        GunType collectedGun = item.GetItemType<GunType>();
        MeleeType collectedMelee = item.GetItemType<MeleeType>();
        ItemType collectedGrenade = item.GetItemType<ItemType>();
        HealthItemType collectedHealthItem = item.GetItemType<HealthItemType>();
        AmmunitionType collectedAmmoType = item.GetItemType<AmmunitionType>();
        if (collectedGun != GunType.None) {
            foreach (ItemSO gun in guns) {
                if (gun.gunType == collectedGun) {
                    AddItem(gun, 1);
                    return;
                }
            }
        } else if (collectedMelee != MeleeType.None) {
            foreach (ItemSO melee in melees) {
                if (melee.meleeType == collectedMelee) {
                    AddItem(melee, 1);
                    return;
                }
            }
        } else if (collectedGrenade != ItemType.None) {
            if (collectedGrenade == ItemType.Grenade) {
                AddItem(grenade, 1);
                return;
            }
        } else if (collectedHealthItem != HealthItemType.None) {
            foreach (ItemSO healthItem in healthItems) {
                if (healthItem.healthItemType == collectedHealthItem) {
                    AddItem(healthItem, 1);
                    return;
                }
            }
        } else if (collectedAmmoType != AmmunitionType.None) {
            foreach (ItemSO ammoItem in ammunitions) {
                if (ammoItem.ammunitionType == collectedAmmoType) {
                    int amount = UnityEngine.Random.Range((int)(ammoItem.maxStackSize * 0.1), (int)(ammoItem.maxStackSize * 0.3));
                    AddItem(ammoItem, amount);
                    return;
                }
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
    /// Adds a single item by matching its type against the configured item lists.
    /// </summary>
    public void AddItem<T>(ItemType itemType, T type) {
        switch (itemType) {
            case ItemType.Gun:
                foreach (ItemSO gun in guns) {
                    if (EqualityComparer<T>.Default.Equals(type, (T)(object)gun.gunType)) {
                        AddItem(gun, 1);
                        return;
                    }
                }
                break;
            case ItemType.Melee:
                foreach (ItemSO melee in melees) {
                    if (EqualityComparer<T>.Default.Equals(type, (T)(object)melee.meleeType)) {
                        AddItem(melee, 1);
                        return;
                    }
                }
                break;
        }

        Debug.LogWarning($"No Item Found with that Type {type}");
    }

    /// <summary>
    /// Adds a stackable item by matching its type against the configured item lists.
    /// </summary>
    public void AddItem<T>(ItemType itemType, T type, int amount) {
        switch (itemType) {
            case ItemType.Grenade:
                AddItem(grenade, amount);
                break;
            case ItemType.Consumable:
                foreach (ItemSO healthItem in healthItems) {
                    if (EqualityComparer<T>.Default.Equals(type, (T)(object)healthItem.healthItemType)) {
                        AddItem(healthItem, amount);
                        return;
                    }
                }
                break;
            case ItemType.Ammunition:
                foreach (ItemSO ammo in ammunitions) {
                    if (EqualityComparer<T>.Default.Equals(type, (T)(object)ammo.ammunitionType)) {
                        AddItem(ammo, amount);
                        return;
                    }
                }
                break;
            case ItemType.Scrap:
                AddItem(scrap, amount);
                break;
            default:
                Debug.LogWarning($"No Item Found with that Type {type}");
                break;
        }
    }

    /// <summary>
    /// Reduces the amount of the currently selected equipped item.
    /// </summary>
    /// <param name="amount">How much to reduce.</param>
    public void ConsumeEquippedItem(int amount = 1) {
        if (selectedSlot == null) return;

        if (!selectedSlot.HasItem()) return;

        int newAmount = selectedSlot.GetAmount() - amount;

        if (newAmount <= 0) {
            selectedSlot.ClearSlot();
            player.GetPlayerGunSelector().ResetItem();
        } else {
            selectedSlot.SetItem(selectedSlot.GetItem(), newAmount);
        }
    }

    /// <summary>
    /// Clears every inventory and hotbar slot.
    /// </summary>
    private void ClearAllSlots() {
        foreach (var s in allSlots) {
            s.SetItem(null);
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
                } else if (IsGrenade(to.GetItem())) {
                    ToggleGrenade(to.GetItem());
                } else if (IsConsumable(to.GetItem())) {
                    ToggleConsumable(to.GetItem());
                }
            } else if (from.GetSelected()) {
                ClearPreviousSelectionIfNeeded(from.GetItem());
            }

            return;
        }

        to.SetItem(from.GetItem(), from.GetAmount());

        if (to.GetSelected()) {
            if (IsWeapon(to.GetItem())) {
                ToggleWeaponSelect(to.GetItem());
            } else if (IsMelee(to.GetItem())) {
                ToggleMeleeSelect(to.GetItem());
            } else if (IsGrenade(to.GetItem())) {
                ToggleGrenade(to.GetItem());
            } else if (IsConsumable(to.GetItem())) {
                ToggleConsumable(to.GetItem());
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
    /// Equips or removes the selected weapon depending on the given item.
    /// </summary>
    private void ToggleWeaponSelect(ItemSO weapon) {
        if (weapon == null) {
            player.GetPlayerGunSelector().DequipWeapon();
            return;
        }

        if (!IsGunAllowedInGameMode(weapon.gunType)) {
            player.GetPlayerGunSelector().DequipWeapon();
            return;
        }

        switch (weapon.gunType) {
            case GunType.AssaultRifle:
                player.GetPlayerGunSelector().SelectAssaultRifle(weapon.gunSO);
                break;
            case GunType.Pistol:
                player.GetPlayerGunSelector().SelectPistol(weapon.gunSO);
                break;
            case GunType.Shotgun:
                player.GetPlayerGunSelector().SelectShotgun(weapon.gunSO);
                break;
            case GunType.Sniper:
                player.GetPlayerGunSelector().SelectSniper(weapon.gunSO);
                break;
            default:
                player.GetPlayerGunSelector().DequipWeapon();
                break;
        }
    }

    /// <summary>
    /// Returns whether the given gun type may be equipped in the current game mode.
    /// MeleeOnly forbids all guns, PistolMelee allows only the pistol.
    /// </summary>
    private bool IsGunAllowedInGameMode(GunType gunType) {
        switch (GameMode.Selected) {
            case GameModeType.MeleeOnly:
                return false;
            case GameModeType.PistolMelee:
                return gunType == GunType.Pistol;
            default:
                return true;
        }
    }

    /// <summary>
    /// Equips or removes the selected melee weapon.
    /// </summary>
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

    /// <summary>
    /// Equips or removes the selected grenade item.
    /// </summary>
    private void ToggleGrenade(ItemSO grenade) {
        if (grenade == null) {
            player.GetPlayerGunSelector().DequipGrenade();
        } else {
            switch (grenade.itemType) {
                case ItemType.Grenade:
                    player.GetPlayerGunSelector().SelectGrenade();
                    break;
            }
        }
    }

    /// <summary>
    /// Equips or removes the selected consumable item.
    /// </summary>
    private void ToggleConsumable(ItemSO consumable) {
        if (consumable == null) {
            player.GetPlayerGunSelector().DequipHealthPack();
        } else {
            player.GetPlayerGunSelector().SelectHealthPack(consumable.healthItemType);
        }
    }

    /// <summary>
    /// Checks whether the given item is a weapon.
    /// </summary>
    private bool IsWeapon(ItemSO item) {
        return item != null && item.itemType == ItemType.Gun;
    }

    /// <summary>
    /// Checks whether the given item is a melee item.
    /// </summary>
    private bool IsMelee(ItemSO item) {
        return item != null && item.itemType == ItemType.Melee;
    }

    /// <summary>
    /// Checks whether the given item is a grenade.
    /// </summary>
    private bool IsGrenade(ItemSO item) {
        return item != null && item.itemType == ItemType.Grenade;
    }

    /// <summary>
    /// Checks whether the given item is a consumable.
    /// </summary>
    private bool IsConsumable(ItemSO item) {
        return item != null && item.itemType == ItemType.Consumable;
    }

    /// <summary>
    /// Clears the previous equipped item if it matches the given item type.
    /// </summary>
    private void ClearPreviousSelectionIfNeeded(ItemSO item) {
        if (IsWeapon(item)) {
            ToggleWeaponSelect(null);
        } else if (IsMelee(item)) {
            ToggleMeleeSelect(null);
        } else if (IsGrenade(item)) {
            ToggleGrenade(null);
        } else if (IsConsumable(item)) {
            ToggleConsumable(null);
        }
    }

    /// <summary>
    /// Checks whether enough ammunition exists for the given gun and returns the amount needed.
    /// </summary>
    public bool GetAmmoAvailable(GunSO gun, out int ammoNeed) {
        int maxAmmo = gun.GetMaxAmmo();

        ammoNeed = maxAmmo - gun.currentAmmo;

        int ammoHas = GetAllAmmo(gun);

        if (ammoHas > 0) {
            if (ammoHas >= maxAmmo && gun.currentAmmo <= 0) {
                ammoNeed = maxAmmo;
            } else {
                if (ammoHas <= ammoNeed) {
                    ammoNeed = ammoHas;
                }
            }
            return true;
        } else {
            return false;
        }
    }

    /// <summary>
    /// Returns the total amount of matching ammunition across all slots.
    /// </summary>
    public int GetAllAmmo(GunSO gun) {
        int ammoHas = 0;
        foreach (Slot slot in allSlots) {
            if (slot.HasItem() && slot.GetItem().ammunitionType == gun.ammunitionType && slot.GetItem().gunType == GunType.None) {
                ammoHas += slot.GetAmount();
            }
        }
        return ammoHas;
    }

    /// <summary>
    /// Returns an ammo item ScriptableObject for the given ammunition type.
    /// </summary>
    public ItemSO GetItemSOWithGunType<T>(T ammunitionType) {
        foreach (ItemSO ammo in ammunitions) {
            if (EqualityComparer<T>.Default.Equals(ammunitionType, (T)(object)ammo.ammunitionType)) {
                return ammo;
            }
        }
        return null;
    }

    /// <summary>
    /// Returns the amount of the currently selected inventory slot.
    /// </summary>
    public int GetSelectedItemAmount() {
        if (selectedSlot != null) {
            return selectedSlot.GetAmount();
        }
        return -1;
    }

    /// <summary>
    /// Returns whether an item is currently being dragged.
    /// </summary>
    public bool GetIsDragging() {
        return isDragging;
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

    /// <summary>
    /// Returns all inventory slots managed by this inventory.
    /// </summary>
    public List<Slot> GetInventorySlots() {
        return inventorySlots;
    }

    #region Save/Load
    /// <summary>
    /// Returns the unique save identifier.
    /// </summary>
    public string GetSaveID() => ID;

    /// <summary>
    /// Creates a save data object containing the contents of all slots.
    /// </summary>
    public object Save() {
        InventoryData saveData = new InventoryData();

        for (int i = 0; i < allSlots.Count; i++) {
            var slot = allSlots[i];

            if (slot.HasItem()) {
                ItemSO item = slot.GetItem();

                SlotSaveData slotData = new SlotSaveData {
                    itemName = item.name,
                    amount = slot.GetAmount(),
                    slotIndex = i
                };

                if (item.gunSO != null) {
                    slotData.isGun = true;

                    item.gunSO.SaveGunData();

                    slotData.gunName = item.gunSO.gunName;
                    slotData.gunData = item.gunSO.GetGunData();
                }
                saveData.slots.Add(slotData);
            }
        }
        return saveData;
    }

    /// <summary>
    /// Restores inventory contents from saved data.
    /// </summary>
    public void Load(object data) {
        InventoryData loadData = (InventoryData)data;

        ClearAllSlots();

        foreach (var slotData in loadData.slots) {
            ItemSO item = Resources.Load<ItemSO>($"ScriptableObjects/ItemSO/{slotData.itemName}");

            if (item != null && slotData.isGun) {
                ItemSO runtimeItem = Instantiate(item);

                if (item.gunSO != null) {
                    GunSO gun = Instantiate(item.gunSO);
                    gun.LoadGunData(slotData.gunData);
                    runtimeItem.gunSO = gun;
                }

                allSlots[slotData.slotIndex].SetItem(runtimeItem, slotData.amount);
                continue;
            }

            allSlots[slotData.slotIndex].SetItem(item, slotData.amount);
        }
    }

    /// <summary>
    /// Serializable save data for a single inventory slot.
    /// </summary>
    [Serializable]
    public class SlotSaveData {
        public string itemName;
        public int amount;
        public int slotIndex;

        public bool isGun;
        public string gunName;
        public GunSO.GunData gunData;
    }

    /// <summary>
    /// Serializable save data container for the full inventory.
    /// </summary>
    [Serializable]
    public class InventoryData {
        public List<SlotSaveData> slots = new();
    }
    #endregion

    /// <summary>
    /// Registers the inventory with the save manager.
    /// </summary>
    private void OnEnable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    /// <summary>
    /// Unregisters the inventory from the save manager.
    /// </summary>
    private void OnDisable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }
}
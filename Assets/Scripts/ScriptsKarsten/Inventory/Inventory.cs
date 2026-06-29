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
    /// Saves the Slot that got selected in the hotbar
    /// </summary>
    private Slot selectedSlot = null;

    /// <summary>
    /// Path to the Save File
    /// </summary>
    private string savePath;

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
    /// Hides the inventory container at startup.
    /// </summary>
    private void Start() {
        playerInputHandler.OnHotbarSlotPressed += PlayerInputHandler_OnHotbarSlotPressed;
        Item.OnItemCollected += Item_OnItemCollected;

        container.SetActive(false);

        string savePath = Path.Combine(Application.persistentDataPath, "save.json");
        if (!File.Exists(savePath)) {
            foreach (ItemSO gun in guns) {
                if (gun.gunType == GunType.Pistol) {
                    hotbarSlots[0].SetItem(gun);
                    AddItem(ItemType.Ammunition, AmmunitionType.Ammo9mm, 30);
                    AddItem(ItemType.Consumable, HealthItemType.Bandage, 4);
                    AddItem(ItemType.Consumable, HealthItemType.HealthPack, 2);
                }
            }
        }
    }

    /// <summary>
    /// Handles input for testing item additions, inventory toggling,
    /// and drag-and-drop interactions.
    /// </summary>
    private void Update() {

        if (pauseMenu.IsPaused)
            return;

        if (Keyboard.current.iKey.wasPressedThisFrame) {
            container.SetActive(!container.activeInHierarchy);

            // Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
            // ? CursorLockMode.None
            // : CursorLockMode.Locked;

            if (container.activeInHierarchy || nPCDialog.IsDialogOpen) {
                crosshair.SetActive(false);
                Cursor.visible = true;
            } else {
                crosshair.SetActive(true);
                Cursor.visible = false;
            }

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
    /// If an Item is collected, than it will be shown in the Inventory
    /// </summary>
    /// <param name="item">The item that has been collected</param>
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
                    //Debug.Log(amount + " " + ammoItem.maxStackSize + " " + ammoItem.maxStackSize * 0.1);
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
                        //Debug.Log(ammo + " " + amount);
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
    /// Reduces the amount left at the selected Slot
    /// </summary>
    /// <param name="amount">How much to reduce</param>
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

        // Move item into empty slot
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

    private void ToggleConsumable(ItemSO consumable) {
        if (consumable == null) {
            player.GetPlayerGunSelector().DequipHealthPack();
        } else {
            player.GetPlayerGunSelector().SelectHealthPack(consumable.healthItemType);
        }
    }

    private bool IsWeapon(ItemSO item) {
        return item != null && item.itemType == ItemType.Gun;
    }

    private bool IsMelee(ItemSO item) {
        return item != null && item.itemType == ItemType.Melee;
    }

    private bool IsGrenade(ItemSO item) {
        return item != null && item.itemType == ItemType.Grenade;
    }

    private bool IsConsumable(ItemSO item) {
        return item != null && item.itemType == ItemType.Consumable;
    }

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

    public bool GetAmmoAvailable(GunSO gun, out int ammoNeed) {
        int maxAmmo = gun.shootConfigSO.maxAmmo;

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

    public int GetAllAmmo(GunSO gun) {
        int ammoHas = 0;
        foreach (Slot slot in allSlots) {
            if (slot.HasItem() && slot.GetItem().ammunitionType == gun.ammunitionType && slot.GetItem().gunType == GunType.None) {
                ammoHas += slot.GetAmount();
            }
        }
        return ammoHas;
    }

    public ItemSO GetItemSOWithGunType<T>(T ammunitionType) {
        foreach (ItemSO ammo in ammunitions) {
            if (EqualityComparer<T>.Default.Equals(ammunitionType, (T)(object)ammo.ammunitionType)) {
                return ammo;
            }
        }
        return null;
    }

    public int GetSelectedItemAmount() {
        if (selectedSlot != null) {
            return selectedSlot.GetAmount();
        }
        return -1;
    }

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

    #region Save/Load
    public string GetSaveID() => ID;

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

    public void Load(object data) {
        InventoryData loadData = (InventoryData)data;

        ClearAllSlots();

        foreach (var slotData in loadData.slots) {
            ItemSO item = Resources.Load<ItemSO>($"ScriptableObjects/ItemSO/{slotData.itemName}");

            if (item != null && slotData.isGun) {
                GunSO gun = Instantiate(item.gunSO);

                gun.LoadGunData(slotData.gunData);
            }

            allSlots[slotData.slotIndex].SetItem(item, slotData.amount);
        }
    }

    [Serializable]
    public class SlotSaveData {
        public string itemName;
        public int amount;
        public int slotIndex;

        //Is the Item in the Slot a Gun?
        public bool isGun;
        public string gunName;
        public GunSO.GunData gunData;
    }

    [Serializable]
    public class InventoryData {
        public List<SlotSaveData> slots = new();
    }
    #endregion

    private void OnEnable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Register(this);
    }

    private void OnDisable() {
        if (SaveManager.Instance != null)
            SaveManager.Instance.Unregister(this);
    }
}


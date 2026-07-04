using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Also stores scrap in the inventory.
/// </summary>
public class ScrapInventorySaver : MonoBehaviour {
    // The player's inventory.
    [SerializeField] private Inventory inventory;

    // The scrap item from the project.
    [SerializeField] private ItemSO scrapItem;

    // Allows other scripts to access this script easily.
    private static ScrapInventorySaver instance;

    private void Awake() {
        // Stores this script as the current instance.
        instance = this;
        FindInventoryIfMissing();
    }

    public static void AddScrapToInventory(int amount) {
        // Does nothing if this script is not in the scene.
        if (instance == null) {
            return;
        }

        instance.AddScrap(amount);
    }

    public static bool TryGetScrapAmount(out int amount) {
        amount = 0;

        if (instance == null) {
            return false;
        }

        return instance.TryCountScrapInInventory(out amount);
    }

    public static bool IsScrapItem(ItemSO item) {
        // Checks whether an item is the scrap item.
        return instance != null && item != null && item == instance.scrapItem;
    }

    private void AddScrap(int amount) {
        // Prevents invalid values from being added to the inventory.
        if (amount <= 0) {
            return;
        }

        // Finds the inventory if it has not been assigned yet.
        FindInventoryIfMissing();

        if (inventory == null) {
            Debug.LogWarning("Scrap konnte nicht ins Inventar gelegt werden, weil kein Inventory gefunden wurde.");
            return;
        }

        if (scrapItem == null) {
            Debug.LogWarning("Scrap konnte nicht ins Inventar gelegt werden, weil kein Scrap Item gesetzt wurde.");
            return;
        }

        // Adds scrap to the inventory.
        inventory.AddItem(scrapItem, amount);
    }

    private bool TryCountScrapInInventory(out int amount) {
        // Counts how much scrap is currently stored in the inventory.
        amount = 0;

        FindInventoryIfMissing();

        if (inventory == null || scrapItem == null) {
            return false;
        }

        HashSet<Slot> slots = new HashSet<Slot>();
        AddSlotsFromRoot(inventory.inventorySlotParent, slots);
        AddSlotsFromRoot(inventory.hotbarObj, slots);
        AddSlotsFromRoot(inventory.gameObject, slots);

        foreach (Slot slot in slots) {
            if (slot.GetItem() == scrapItem) {
                amount += slot.GetAmount();
            }
        }

        return true;
    }

    private void AddSlotsFromRoot(GameObject root, HashSet<Slot> slots) {
        // Collects all slots from a UI area.
        if (root == null) {
            return;
        }

        Slot[] foundSlots = root.GetComponentsInChildren<Slot>(true);
        foreach (Slot slot in foundSlots) {
            slots.Add(slot);
        }
    }

    private void FindInventoryIfMissing() {
        // No search is needed when the inventory is already assigned.
        if (inventory != null) {
            return;
        }

        // Automatically finds an Inventory in the scene.
        inventory = FindFirstObjectByType<Inventory>();
    }
}

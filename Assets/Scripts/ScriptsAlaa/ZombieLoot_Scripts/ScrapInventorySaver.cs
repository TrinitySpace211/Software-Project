using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Legt Scrap auch in das Inventar.
/// </summary>
public class ScrapInventorySaver : MonoBehaviour {
    // Das Inventar vom Spieler.
    [SerializeField] private Inventory inventory;

    // Das Scrap-Item aus dem Projekt.
    [SerializeField] private ItemSO scrapItem;

    // Damit andere Scripts dieses Script einfach erreichen koennen.
    private static ScrapInventorySaver instance;

    private void Awake() {
        // Speichert dieses Script als aktuelle Instanz.
        instance = this;
        FindInventoryIfMissing();
    }

    public static void AddScrapToInventory(int amount) {
        // Wenn dieses Script nicht in der Szene ist, passiert nichts.
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
        // Prueft, ob ein Item das Scrap-Item ist.
        return instance != null && item != null && item == instance.scrapItem;
    }

    private void AddScrap(int amount) {
        // Keine ungueltigen Werte ins Inventar legen.
        if (amount <= 0) {
            return;
        }

        // Sucht das Inventar, falls es noch nicht gesetzt wurde.
        FindInventoryIfMissing();

        if (inventory == null) {
            Debug.LogWarning("Scrap konnte nicht ins Inventar gelegt werden, weil kein Inventory gefunden wurde.");
            return;
        }

        if (scrapItem == null) {
            Debug.LogWarning("Scrap konnte nicht ins Inventar gelegt werden, weil kein Scrap Item gesetzt wurde.");
            return;
        }

        // Legt Scrap in das Inventar.
        inventory.AddItem(scrapItem, amount);
    }

    private bool TryCountScrapInInventory(out int amount) {
        // Zaehlt, wie viel Scrap aktuell wirklich im Inventar liegt.
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
        // Sammelt alle Slots aus einem UI-Bereich.
        if (root == null) {
            return;
        }

        Slot[] foundSlots = root.GetComponentsInChildren<Slot>(true);
        foreach (Slot slot in foundSlots) {
            slots.Add(slot);
        }
    }

    private void FindInventoryIfMissing() {
        // Wenn das Inventar schon gesetzt ist, muss nicht gesucht werden.
        if (inventory != null) {
            return;
        }

        // Sucht automatisch ein Inventory in der Szene.
        inventory = FindFirstObjectByType<Inventory>();
    }
}
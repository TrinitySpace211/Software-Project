using UnityEngine;

/// <summary>
/// Speichert erhaltenen Schrott zusätzlich im Inventar.
/// </summary>
public class ScrapInventorySaver : MonoBehaviour {
    // Inventar vom Spieler.
    [SerializeField] private Inventory inventory;

    // Scrap-Item, das ins Inventar gelegt wird.
    [SerializeField] private ItemSO scrapItem;

    private static ScrapInventorySaver instance;

    private void Awake() {
        instance = this;
        FindInventoryIfMissing();
    }

    public static void AddScrapToInventory(int amount) {
        // Wenn kein Script in der Szene liegt, wird nichts ins Inventar gelegt.
        if (instance == null) {
            return;
        }

        instance.AddScrap(amount);
    }

    private void AddScrap(int amount) {
        // Ungültige Werte ignorieren.
        if (amount <= 0) {
            return;
        }

        FindInventoryIfMissing();

        if (inventory == null) {
            Debug.LogWarning("Scrap konnte nicht ins Inventar gelegt werden, weil kein Inventory gefunden wurde.");
            return;
        }

        if (scrapItem == null) {
            Debug.LogWarning("Scrap konnte nicht ins Inventar gelegt werden, weil kein Scrap Item gesetzt wurde.");
            return;
        }

        inventory.AddItem(scrapItem, amount);
    }

    private void FindInventoryIfMissing() {
        // Sucht das Inventar automatisch, falls es im Inspector nicht gesetzt wurde.
        if (inventory != null) {
            return;
        }

        inventory = FindFirstObjectByType<Inventory>();
    }
}

using UnityEngine;

public enum ItemType {
    None,
    Gun,
    Melee,
    Consumable,
    Misc
}

/// <summary>
/// Represents an item definition stored as a ScriptableObject.
/// Contains item-related data such as visuals, stack size,
/// and prefab references.
/// </summary>
[CreateAssetMenu(fileName = "Item", menuName = "NewItem")]
public class ItemSO : ScriptableObject {
    public ItemType itemType;
    public GunType gunType;
    public MeleeType meleeType;
    /// <summary>
    /// The display name of the item.
    /// </summary>
    public string itemName;

    /// <summary>
    /// The icon displayed in the inventory and UI.
    /// </summary>
    public Sprite icon;

    /// <summary>
    /// The maximum number of items that can be stacked
    /// in a single inventory slot.
    /// </summary>
    public int maxStackSize;

    /// <summary>
    /// The world prefab used when the item exists in the game world
    /// (e.g., dropped on the ground).
    /// </summary>
    public GameObject itemPrefab;
}
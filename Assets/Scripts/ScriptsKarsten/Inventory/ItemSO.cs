using UnityEngine;

public enum ItemType {
    None,
    Gun,
    Melee,
    Grenade,
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
    public HealthItemType healthItemType;

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

    /// <summary>
    /// The prefab displayed in the player's hand
    /// when the item is equipped or held.
    /// </summary>
    public GameObject handItemPrefab;

    /// <summary>
    /// Damage value of the item if it is used as a weapon.
    /// </summary>
    public int baseDamage;
}
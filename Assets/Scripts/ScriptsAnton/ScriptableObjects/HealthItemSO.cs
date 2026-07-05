using UnityEngine;

/// <summary>
/// All the HealthItem Types
/// </summary>
public enum HealthItemType {
    None,
    Bandage,
    HealthBottle,
    Syringe,
    HealthPack
}

/// <summary>
/// Scriptable Object of the many health items (Bandage, Health Bottle, Syringe, Health Pack and more)
/// Contains a Function to heal a Player
/// </summary>
[CreateAssetMenu(fileName = "HealthItemSO", menuName = "Health/HealthItemSO")]
public class HealthItemSO : ScriptableObject {
    public string healthPackName;
    public float healAmount;
    public WeaponSlot weaponSlot;
    public HealthItemType type;
    public GameObject modelPrefab;

    private GameObject model;

    /// <summary>
    /// Spawns the prefab or activates it
    /// </summary>
    /// <param name="parent">The parent to set the instance to</param>
    public void Spawn(Transform parent) {
        if (model == null) {
            model = Instantiate(modelPrefab);
            model.transform.SetParent(parent, false);
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;
        } else {
            model.SetActive(true);
        }
    }

    /// <summary>
    /// Heals the Player by the amount that is set in the Scriptable Object
    /// </summary>
    /// <param name="player">The Player that should be healed</param>
    public void Heal(Player player) {
        player.GetPlayerHealth().HealPlayerHealth(healAmount);
    }

    /// <summary>
    /// Deactivates the Item
    /// </summary>
    public void Despawn() {
        model.SetActive(false);
    }

    /// <summary>
    /// Destroys the GameObject when fully used from the Inventory.
    /// </summary>
    public void DestroySelf() {
        Destroy(model);
    }
}

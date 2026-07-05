using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Class of the dropped Items
/// </summary>
public class Item : MonoBehaviour {

    private bool inRange = false;
    private Outline[] outlines;
    private SphereCollider sphereCollider;
    private MeleeType meleeType;
    private GunType gunType;
    private ItemType grenade;
    private HealthItemType healthItemType;
    private AmmunitionType ammunitionType;

    //Event
    public static event Action<Item> OnItemCollected;

    /// <summary>
    /// Turns all the Outlines off
    /// </summary>
    private void Start() {
        sphereCollider = GetComponent<SphereCollider>();
        outlines = GetComponentsInChildren<Outline>();

        foreach (Outline outline in outlines) {
            outline.enabled = false;
        }
    }

    /// <summary>
    /// Sets the Outlines
    /// </summary>
    /// <param name="isVisible">true enables all the Outlines false disables them</param>
    public void SetOutlineState(bool isVisible) {
        foreach (Outline outline in outlines) {
            if (outline != null) {
                outline.enabled = isVisible;
            }
        }
    }

    /// <summary>
    /// Sends an Event which Item got collected and destroys the item afterwards
    /// </summary>
    public void Collect() {
        foreach (Outline outline in outlines) {
            if (outline != null && inRange) {
                OnItemCollected?.Invoke(this);
                sphereCollider.enabled = false;
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Checks if the Player entered the Area it can be collected
    /// </summary>
    /// <param name="other">The Collider that entered</param>
    private void OnTriggerEnter(Collider other) {
        if (other.GetComponent<Player>()) {
            foreach (Outline outline in outlines) {
                if (outline != null) {
                    outline.enabled = true;
                    inRange = true;
                }
            }

        }
    }

    /// <summary>
    /// Checks if the Player exited the Area
    /// </summary>
    /// <param name="other">The Collider that exited</param>
    private void OnTriggerExit(Collider other) {
        if (other.GetComponent<Player>()) {
            foreach (Outline outline in outlines) {
                if (outline != null) {
                    outline.enabled = false;
                    inRange = false;
                }
            }
        }
    }

    /// <summary>
    /// Sets the item to a certain Type
    /// </summary>
    /// <typeparam name="T">The Type this item is</typeparam>
    /// <param name="itemType">The Type of item</param>
    public void SetItemType<T>(T itemType) {
        if (itemType is MeleeType meleeType) this.meleeType = meleeType;
        if (itemType is GunType gunType) this.gunType = gunType;
        if (itemType is ItemType.Grenade) grenade = ItemType.Grenade;
        if (itemType is HealthItemType healthItemType) this.healthItemType = healthItemType;
        if (itemType is AmmunitionType ammunitionType) this.ammunitionType = ammunitionType;
    }

    /// <summary>
    /// Getter for what type this item is
    /// </summary>
    /// <typeparam name="T">The Tyoe of this item</typeparam>
    /// <returns>The Type</returns>
    public T GetItemType<T>() {
        if (typeof(T) == typeof(MeleeType) && meleeType != MeleeType.None) return (T)(object)meleeType;
        if (typeof(T) == typeof(GunType) && gunType != GunType.None) return (T)(object)gunType;
        if (typeof(T) == typeof(ItemType) && grenade == ItemType.Grenade) return (T)(object)grenade;
        if (typeof(T) == typeof(HealthItemType) && healthItemType != HealthItemType.None) return (T)(object)healthItemType;
        if (typeof(T) == typeof(AmmunitionType) && ammunitionType != AmmunitionType.None) return (T)(object)ammunitionType;

        return default;
    }
}

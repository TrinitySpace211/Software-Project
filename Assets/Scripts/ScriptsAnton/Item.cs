using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    private void Start() {
        sphereCollider = GetComponent<SphereCollider>();
        outlines = GetComponentsInChildren<Outline>();

        foreach (Outline outline in outlines) {
            outline.enabled = false;
        }
    }

    public void SetOutlineState(bool isVisible) {
        foreach (Outline outline in outlines) {
            if (outline != null) {
                outline.enabled = isVisible;
            }
        }
    }

    public void Collect() {
        foreach (Outline outline in outlines) {
            if (outline != null && inRange) {
                OnItemCollected?.Invoke(this);
                sphereCollider.enabled = false;
                Destroy(gameObject);
            }
        }
    }

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

    public void SetItemType<T>(T itemType) {
        if (itemType is MeleeType meleeType) this.meleeType = meleeType;
        if (itemType is GunType gunType) this.gunType = gunType;
        if (itemType is ItemType.Grenade) grenade = ItemType.Grenade;
        if (itemType is HealthItemType healthItemType) this.healthItemType = healthItemType;
        if (itemType is AmmunitionType ammunitionType) this.ammunitionType = ammunitionType;
    }

    public T GetItemType<T>() {
        if (typeof(T) == typeof(MeleeType) && meleeType != MeleeType.None) return (T)(object)meleeType;
        if (typeof(T) == typeof(GunType) && gunType != GunType.None) return (T)(object)gunType;
        if (typeof(T) == typeof(ItemType) && grenade == ItemType.Grenade) return (T)(object)grenade;
        if (typeof(T) == typeof(HealthItemType) && healthItemType != HealthItemType.None) return (T)(object)healthItemType;
        if (typeof(T) == typeof(AmmunitionType) && ammunitionType != AmmunitionType.None) return (T)(object)ammunitionType;

        return default;
    }
}

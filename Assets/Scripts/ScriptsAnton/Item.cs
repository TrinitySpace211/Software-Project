using System;
using UnityEngine;

public class Item : MonoBehaviour {

    private bool isCollectable = true;
    private Outline outline;
    private SphereCollider sphereCollider;
    private MeleeType meleeType;
    private GunType gunType;
    private ItemType grenade;
    private HealthItemType healthItemType;

    //Event
    public static event Action<Item> OnItemCollected;

    private void Start() {
        sphereCollider = GetComponent<SphereCollider>();
        outline = GetComponent<Outline>();
        outline.enabled = false;
    }

    public void Collect() {
        if (outline != null && outline.enabled && isCollectable) {
            isCollectable = false;
            OnItemCollected?.Invoke(this);
            sphereCollider.enabled = false;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!isCollectable) return;

        if (other.GetComponent<Player>()) {
            if (outline != null) {
                outline.enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!isCollectable) return;

        if (other.GetComponent<Player>()) {
            if (outline != null) {
                outline.enabled = false;
            }
        }
    }

    public void SetItemType<T>(T itemType) {
        if (itemType is MeleeType meleeType) this.meleeType = meleeType;
        if (itemType is GunType gunType) this.gunType = gunType;
        if (itemType is ItemType.Grenade) grenade = ItemType.Grenade;
        if (itemType is HealthItemType healthItemType) this.healthItemType = healthItemType;
    }

    public T GetItemType<T>() {
        if (typeof(T) == typeof(MeleeType) && meleeType != MeleeType.None) return (T)(object)meleeType;
        if (typeof(T) == typeof(GunType) && gunType != GunType.None) return (T)(object)gunType;
        if (typeof(T) == typeof(ItemType) && grenade == ItemType.Grenade) return (T)(object)grenade;
        if (typeof(T) == typeof(HealthItemType) && healthItemType != HealthItemType.None) return (T)(object)healthItemType;

        return default;
    }
}

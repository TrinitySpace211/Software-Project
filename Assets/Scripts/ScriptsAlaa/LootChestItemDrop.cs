using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Opens a chest visually and spawns random loot items when the player presses E nearby.
/// This script can be added next to LootChest without changing LootChest itself.
/// </summary>
public class LootChestItemDrop : MonoBehaviour {
    [Header("References")]
    // Player can be assigned in the Inspector.
    // If it is empty, the script tries to find it automatically.
    [SerializeField] private Transform player;

    [Header("Interaction")]
    // Use the same distance and key as the LootChest script.
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private Key interactionKey = Key.E;

    [Header("Chest Opening")]
    // Optional lid object. If assigned, it rotates when the chest opens.
    [SerializeField] private Transform lid;
    [SerializeField] private Vector3 openRotation = new Vector3(-70f, 0f, 0f);
    [SerializeField] private float openSpeed = 8f;

    [Header("Loot Spawn")]
    // Add item prefabs here. The script chooses random prefabs from this list.
    [SerializeField] private ItemSO[] itemPrefabs;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int minItems = 1;
    [SerializeField] private int maxItems = 3;
    [SerializeField] private float itemSpacing = 0.45f;
    [SerializeField] private float spawnHeight = 0.75f;
    [SerializeField] private float landingForwardOffset = 1f;
    [SerializeField] private float revealHeight = 0.55f;
    [SerializeField] private float revealDuration = 0.65f;

    [Header("Loot Light")]
    // Short light effect that appears when loot comes out of the chest.
    [SerializeField] private Color revealLightColor = new Color(1f, 0.92f, 0.45f);
    [SerializeField] private float revealLightRange = 2.5f;
    [SerializeField] private float revealLightIntensity = 4f;
    [SerializeField] private float revealLightDuration = 1.2f;

    private Quaternion closedRotation;
    private Quaternion targetOpenRotation;
    private bool playerInRange;
    private bool waitsForPlayerToLeave;
    private bool hasSpawnedLoot;
    private Coroutine lidRoutine;
    private Light revealLight;

    private void Awake() {
        // Prepare all runtime references and effects before the first frame.
        FindMissingReferences();
        SaveLidRotations();
        CreateRevealLight();
    }

    private void Update() {
        // Keep checking the player state and the interaction key every frame.
        FindMissingReferences();
        CheckPlayerDistance();
        ResetAfterPlayerLeaves();
        CheckInteractionInput();
    }

    private void FindMissingReferences() {
        // Automatically find the player by using the "Player" tag.
        if (player == null) {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) {
                player = playerObject.transform;
            }
        }
    }

    private void SaveLidRotations() {
        // Store the closed and opened lid rotations.
        if (lid == null) {
            return;
        }

        closedRotation = lid.localRotation;
        targetOpenRotation = closedRotation * Quaternion.Euler(openRotation);
    }

    private void CheckPlayerDistance() {
        // Checks whether the player is close enough to the chest.
        if (player == null) {
            playerInRange = false;
            return;
        }

        playerInRange = Vector3.Distance(player.position, transform.position) <= interactionDistance;
    }

    private void ResetAfterPlayerLeaves() {
        // The chest can be opened again after the player has left its range once.
        if (waitsForPlayerToLeave && !playerInRange) {
            waitsForPlayerToLeave = false;
            CloseLid();
        }
    }

    private void CheckInteractionInput() {
        // Only spawn loot once per visit, then wait until the player leaves.
        if (waitsForPlayerToLeave || !playerInRange || Keyboard.current == null) {
            return;
        }

        KeyControl keyControl = Keyboard.current[interactionKey];
        if (keyControl != null && keyControl.wasPressedThisFrame) {
            OpenAndDropItems();
        }
    }

    private void OpenAndDropItems() {
        // Prevent another open action until the player leaves the chest area.
        waitsForPlayerToLeave = true;
        OpenLid();

        // The chest can be opened multiple times, but the loot should only spawn once.
        if (!hasSpawnedLoot) {
            hasSpawnedLoot = true;
            StartCoroutine(SpawnItemsWithReveal());
        }
    }

    private void OpenLid() {
        // If no lid was assigned, only the loot items spawn.
        if (lid == null) {
            return;
        }

        StartLidRotation(targetOpenRotation);
    }

    private void CloseLid() {
        // The lid closes again when the player leaves the chest.
        if (lid == null) {
            return;
        }

        StartLidRotation(closedRotation);
    }

    private void StartLidRotation(Quaternion targetRotation) {
        // Stop the old lid animation before starting a new one.
        if (lidRoutine != null) {
            StopCoroutine(lidRoutine);
        }

        lidRoutine = StartCoroutine(RotateLid(targetRotation));
    }

    private IEnumerator RotateLid(Quaternion targetRotation) {
        // Smoothly rotate the lid until it reaches the target rotation.
        while (Quaternion.Angle(lid.localRotation, targetRotation) > 0.5f) {
            lid.localRotation = Quaternion.Slerp(lid.localRotation, targetRotation, openSpeed * Time.deltaTime);
            yield return null;
        }

        lid.localRotation = targetRotation;
    }

    private void CreateRevealLight() {
        // Create a light that is only used for the short loot reveal effect.
        GameObject lightObject = new GameObject("LootRevealLight");
        lightObject.transform.SetParent(transform);
        lightObject.transform.localPosition = spawnPoint != null
            ? transform.InverseTransformPoint(spawnPoint.position)
            : Vector3.up * spawnHeight;
        lightObject.transform.localRotation = Quaternion.identity;

        revealLight = lightObject.AddComponent<Light>();
        revealLight.type = LightType.Point;
        revealLight.color = revealLightColor;
        revealLight.range = revealLightRange;
        revealLight.intensity = 0f;
        revealLight.shadows = LightShadows.None;
        revealLight.enabled = false;
    }

    private IEnumerator SpawnItemsWithReveal() {
        // Stop here if no loot prefabs were assigned in the Inspector.
        if (itemPrefabs == null || itemPrefabs.Length == 0) {
            Debug.LogWarning($"Loot chest '{name}' has no item prefabs assigned.");
            yield break;
        }

        // Choose how many items should appear and start the short light flash.
        int itemCount = UnityEngine.Random.Range(minItems, maxItems + 1);
        StartCoroutine(PlayRevealLight());

        // Spawn items with a small delay so the reveal feels less instant.
        for (int i = 0; i < itemCount; i++) {
            SpawnSingleItem(i, itemCount);
            yield return new WaitForSeconds(0.12f);
        }
    }

    private IEnumerator PlayRevealLight() {
        // Fade the reveal light out over time.
        if (revealLight == null) {
            yield break;
        }

        revealLight.enabled = true;
        float elapsedTime = 0f;

        while (elapsedTime < revealLightDuration) {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / revealLightDuration);
            revealLight.intensity = revealLightIntensity * (1f - progress);
            yield return null;
        }

        revealLight.intensity = 0f;
        revealLight.enabled = false;
    }

    private void SpawnSingleItem(int itemIndex, int itemCount) {
        // Pick one random prefab from the loot list.
        ItemSO itemSO = itemPrefabs[UnityEngine.Random.Range(0, itemPrefabs.Length)];
        if (itemSO == null) {
            return;
        }

        // Spawn the item at the chest, then animate it to its final landing position.
        Vector3 startPosition = GetSpawnPosition();
        Vector3 landingPosition = GetLandingPosition(itemIndex, itemCount);

        Item spawnedItem = Instantiate(itemSO.itemPrefab, startPosition, itemSO.itemPrefab.transform.rotation).GetComponentInChildren<Item>();

        if (itemSO.itemType == ItemType.Gun) {
            SetItemType(() => spawnedItem.SetItemType(itemSO.gunType), itemSO.itemType, itemSO.gunType);
        } else if (itemSO.itemType == ItemType.Melee) {
            SetItemType(() => spawnedItem.SetItemType(itemSO.meleeType), itemSO.itemType, itemSO.meleeType);
        } else if (itemSO.itemType == ItemType.Grenade) {
            SetItemType(() => spawnedItem.SetItemType(itemSO.itemType), itemSO.itemType, itemSO.itemType);
        } else if (itemSO.itemType == ItemType.Consumable) {
            SetItemType(() => spawnedItem.SetItemType(itemSO.healthItemType), itemSO.itemType, itemSO.healthItemType);
        } else if (itemSO.itemType == ItemType.Ammunition) {
            SetItemType(() => spawnedItem.SetItemType(itemSO.ammunitionType), itemSO.itemType, itemSO.ammunitionType);
        }

        StartCoroutine(RevealItem(spawnedItem.gameObject, startPosition, landingPosition));
    }

    private Vector3 GetSpawnPosition() {
        // Use the custom spawn point if one exists; otherwise spawn above the chest.
        return spawnPoint != null
            ? spawnPoint.position
            : transform.position + (Vector3.up * spawnHeight);
    }

    private Vector3 GetLandingPosition(int itemIndex, int itemCount) {
        // Spread multiple items next to each other in front of the chest.
        float centerOffset = (itemCount - 1) * 0.5f;
        float sideOffset = (itemIndex - centerOffset) * itemSpacing;

        return transform.position
            + (transform.forward * landingForwardOffset)
            + (transform.right * sideOffset)
            + (Vector3.up * 0.1f);
    }

    private IEnumerator RevealItem(GameObject spawnedItem, Vector3 startPosition, Vector3 landingPosition) {
        // Animate one spawned item from the chest to the landing position.
        if (spawnedItem == null) {
            yield break;
        }

        // Disable physics during the reveal animation so the item does not fall too early.
        Rigidbody itemRigidbody = spawnedItem.GetComponent<Rigidbody>();

        bool hadRigidbody = itemRigidbody != null;

        if (hadRigidbody) {
            itemRigidbody.isKinematic = true;
        }

        // Move the item upward first, then down to the final landing position.
        Vector3 peakPosition = startPosition + (Vector3.up * revealHeight);
        float elapsedTime = 0f;

        while (elapsedTime < revealDuration) {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / revealDuration);

            Vector3 firstHalf = Vector3.Lerp(startPosition, peakPosition, progress);
            Vector3 secondHalf = Vector3.Lerp(peakPosition, landingPosition, progress);
            spawnedItem.transform.position = Vector3.Lerp(firstHalf, secondHalf, progress);
            spawnedItem.transform.Rotate(Vector3.up, 120f * Time.deltaTime, Space.World);

            yield return null;
        }

        spawnedItem.transform.position = landingPosition;

        // Re-enable physics after the item has landed.
        if (hadRigidbody) {
            itemRigidbody.isKinematic = false;
        }
    }

    private void SetItemType<T>(Action action, ItemType itemType, T getItemType) {
        bool isValid = itemType switch {
            ItemType.Gun => getItemType is GunType.AssaultRifle or GunType.Pistol or GunType.Shotgun or GunType.Sniper,
            ItemType.Melee => getItemType is MeleeType.Knife or MeleeType.Baseball_Bat or MeleeType.Crowbar or MeleeType.Hatchet or MeleeType.Sword or MeleeType.Tomahawk,
            ItemType.Grenade => getItemType is ItemType.Grenade,
            ItemType.Consumable => getItemType is HealthItemType.Bandage or HealthItemType.HealthBottle or HealthItemType.Syringe or HealthItemType.HealthPack,
            ItemType.Ammunition => getItemType is AmmunitionType.Ammo556x45mm or AmmunitionType.Ammo9mm or AmmunitionType.Ammo12Gauge or AmmunitionType.AmmoSniper,
            _ => false
        };

        if (isValid) {
            action?.Invoke();
        }
    }

    /* private void OnDrawGizmosSelected() {
        // Shows the interaction range and approximate spawn area in the editor.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        Gizmos.color = Color.green;
        Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position + (Vector3.up * spawnHeight);
        Gizmos.DrawWireSphere(center, 0.2f);
    } */
}

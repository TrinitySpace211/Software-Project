using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Opens a chest and creates random loot when F is pressed.
/// Works together with the LootChest script.
/// </summary>
public class LootChestItemDrop : MonoBehaviour {
    [Header("References")]
    // The player can be assigned in the Inspector.
    // An empty field is found automatically.
    [SerializeField] private Transform player;

    [Header("Interaction")]
    // Uses the same distance and key as LootChest.
    [SerializeField] private float interactionDistance = 2.5f;
    [SerializeField] private Key interactionKey = Key.F;

    [Header("Chest Opening")]
    // Optional lid that rotates when the chest is opened.
    [SerializeField] private Transform lid;
    [SerializeField] private Vector3 openRotation = new Vector3(-70f, 0f, 0f);
    [SerializeField] private float openSpeed = 8f;

    [Header("Loot Spawn")]
    // Random loot is selected from this list.
    [SerializeField] private ItemSO[] itemPrefabs;
    [SerializeField] private ItemSO scrapItem;
    [SerializeField, Range(0f, 1f)] private float scrapDropChance = 0.65f;
    [SerializeField] private float scrapIconWorldSize = 0.35f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private int minItems = 1;
    [SerializeField] private int maxItems = 3;
    [SerializeField] private float itemSpacing = 0.45f;
    [SerializeField] private float spawnHeight = 0.75f;
    [SerializeField] private float landingForwardOffset = 1f;
    [SerializeField] private float revealHeight = 0.55f;
    [SerializeField] private float revealDuration = 0.65f;

    [Header("Loot Light")]
    // Short light effect when loot comes out of the chest.
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
        // Prepares references and effects on startup.
        FindMissingReferences();
        SaveLidRotations();
        CreateRevealLight();

        // Automatically ensures that the chest receives new loot every day.
        if (GetComponent<DailyLootChestReset>() == null) {
            gameObject.AddComponent<DailyLootChestReset>();
        }
    }

    private void Update() {
        // Continuously checks the player and the key.
        FindMissingReferences();
        CheckPlayerDistance();
        ResetAfterPlayerLeaves();
        CheckInteractionInput();
    }

    private void FindMissingReferences() {
        // Finds the player automatically by using the Player tag.
        if (player == null) {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) {
                player = playerObject.transform;
            }
        }
    }

    private void SaveLidRotations() {
        // Stores the closed and open lid rotations.
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
        // The chest can be opened again after the player leaves.
        if (waitsForPlayerToLeave && !playerInRange) {
            waitsForPlayerToLeave = false;
            CloseLid();
        }
    }

    private void CheckInteractionInput() {
        // Waits after opening until the player leaves the area.
        if (waitsForPlayerToLeave || !playerInRange || Keyboard.current == null) {
            return;
        }

        KeyControl keyControl = Keyboard.current[interactionKey];
        if (keyControl != null && keyControl.wasPressedThisFrame) {
            OpenAndDropItems();
        }
    }

    private void OpenAndDropItems() {
        // Prevents another opening until the player leaves.
        waitsForPlayerToLeave = true;
        OpenLid();

        // Loot appears only once per available day.
        if (!hasSpawnedLoot) {
            hasSpawnedLoot = true;
            StartCoroutine(SpawnItemsWithReveal());
        }
    }

    /// <summary>
    /// Makes the loot available again for the new day.
    /// </summary>
    public void ResetForNewDay() {
        hasSpawnedLoot = false;
        waitsForPlayerToLeave = false;
        CloseLid();
    }

    private void OpenLid() {
        // Only the loot appears if no lid is assigned.
        if (lid == null) {
            return;
        }

        StartLidRotation(targetOpenRotation);
    }

    private void CloseLid() {
        // The lid closes when the player leaves.
        if (lid == null) {
            return;
        }

        StartLidRotation(closedRotation);
    }

    private void StartLidRotation(Quaternion targetRotation) {
        // Stops the previous lid movement before starting a new one.
        if (lidRoutine != null) {
            StopCoroutine(lidRoutine);
        }

        lidRoutine = StartCoroutine(RotateLid(targetRotation));
    }

    private IEnumerator RotateLid(Quaternion targetRotation) {
        // Smoothly rotates the lid to the target position.
        while (Quaternion.Angle(lid.localRotation, targetRotation) > 0.5f) {
            lid.localRotation = Quaternion.Slerp(lid.localRotation, targetRotation, openSpeed * Time.deltaTime);
            yield return null;
        }

        lid.localRotation = targetRotation;
    }

    private void CreateRevealLight() {
        // Creates the light for the short loot effect.
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
        // Stops if no loot items are assigned.
        if (itemPrefabs == null || itemPrefabs.Length == 0) {
            Debug.LogWarning($"Loot chest '{name}' has no item prefabs assigned.");
            yield break;
        }

        // Determines the item count and starts the light effect.
        int itemCount = UnityEngine.Random.Range(minItems, maxItems + 1);
        StartCoroutine(PlayRevealLight());

        // Creates the items with a short delay.
        for (int i = 0; i < itemCount; i++) {
            SpawnSingleItem(i, itemCount);
            yield return new WaitForSeconds(0.12f);
        }
    }

    private IEnumerator PlayRevealLight() {
        // Slowly fades out the loot light.
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
        // Scrap is common. Healing items are selected less often.
        ItemSO itemSO = ChooseRandomLootItem();
        if (itemSO == null) {
            return;
        }

        // Creates the item and moves it to its final position.
        Vector3 startPosition = GetSpawnPosition();
        Vector3 landingPosition = GetLandingPosition(itemIndex, itemCount);

        // Scrap has no world prefab, so it is created here.
        if (itemSO == scrapItem) {
            SpawnScrapPickup(startPosition, landingPosition);
            return;
        }

        if (itemSO.itemPrefab == null) {
            Debug.LogWarning($"Loot item '{itemSO.itemName}' has no item prefab assigned.");
            return;
        }

        Item spawnedItem = Instantiate(itemSO.itemPrefab, startPosition, itemSO.itemPrefab.transform.rotation).GetComponentInChildren<Item>();

        if (spawnedItem == null) {
            Debug.LogWarning($"Loot item '{itemSO.itemName}' has no Item component.");
            return;
        }

        if (itemSO.itemType == ItemType.Gun) {
            SetItemType(() => spawnedItem.SetItemType(itemSO.gunType), itemSO.itemType, itemSO.gunType);
        } else if (itemSO.itemType == ItemType.Melee) {
            SetItemType(() => spawnedItem.SetItemType(itemSO.meleeType), itemSO.itemType, itemSO.meleeType);
        } else if (itemSO.itemType == ItemType.Grenade) {
            SetItemType(() => spawnedItem.SetItemType(itemSO.itemType), itemSO.itemType, itemSO.itemType);
        } else if (itemSO.itemType == ItemType.Consumable) {
            SetItemType(() => spawnedItem.SetItemType(itemSO.healthItemType), itemSO.itemType, itemSO.healthItemType);
        }

        StartCoroutine(RevealItem(spawnedItem.gameObject, startPosition, landingPosition));
    }

    private ItemSO ChooseRandomLootItem() {
        // Scrap has a high chance of appearing from the chest.
        if (scrapItem != null && UnityEngine.Random.value < scrapDropChance) {
            return scrapItem;
        }

        // Counts only healing items and excludes scrap from this selection.
        int healingItemCount = 0;
        foreach (ItemSO item in itemPrefabs) {
            if (item != null && item != scrapItem) {
                healingItemCount++;
            }
        }

        // Uses scrap if no healing items are assigned.
        if (healingItemCount == 0) {
            return scrapItem;
        }

        int selectedHealingItem = UnityEngine.Random.Range(0, healingItemCount);
        foreach (ItemSO item in itemPrefabs) {
            if (item == null || item == scrapItem) {
                continue;
            }

            if (selectedHealingItem == 0) {
                return item;
            }

            selectedHealingItem--;
        }

        return scrapItem;
    }

    private void SpawnScrapPickup(Vector3 startPosition, Vector3 landingPosition) {
        // Creates a visible scrap pickup using the existing scrap icon.
        GameObject pickupObject = new GameObject("Scrap Pickup");
        pickupObject.transform.position = startPosition;

        SphereCollider pickupCollider = pickupObject.AddComponent<SphereCollider>();
        pickupCollider.isTrigger = true;
        pickupCollider.radius = 0.5f;

        GameObject iconObject = new GameObject("Scrap Icon");
        iconObject.transform.SetParent(pickupObject.transform, false);

        SpriteRenderer spriteRenderer = iconObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = scrapItem.icon;
        spriteRenderer.sortingOrder = 5;

        // Adjusts the icon to match the size of the healing items.
        if (spriteRenderer.sprite != null) {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            float largestSide = Mathf.Max(spriteSize.x, spriteSize.y);

            if (largestSide > 0f) {
                iconObject.transform.localScale = Vector3.one * (scrapIconWorldSize / largestSide);
            }
        }

        ScrapPickup pickup = pickupObject.AddComponent<ScrapPickup>();
        pickup.SetAmount(1);

        StartCoroutine(RevealItem(pickupObject, startPosition, landingPosition));
    }

    private Vector3 GetSpawnPosition() {
        // Uses the spawn point or a position above the chest.
        return spawnPoint != null
            ? spawnPoint.position
            : transform.position + (Vector3.up * spawnHeight);
    }

    private Vector3 GetLandingPosition(int itemIndex, int itemCount) {
        // Spreads multiple items next to each other in front of the chest.
        float centerOffset = (itemCount - 1) * 0.5f;
        float sideOffset = (itemIndex - centerOffset) * itemSpacing;

        return transform.position
            + (transform.forward * landingForwardOffset)
            + (transform.right * sideOffset)
            + (Vector3.up * 0.1f);
    }

    private IEnumerator RevealItem(GameObject spawnedItem, Vector3 startPosition, Vector3 landingPosition) {
        // Moves an item from the chest to its final position.
        if (spawnedItem == null) {
            yield break;
        }

        // Temporarily disables physics during the movement.
        Rigidbody itemRigidbody = spawnedItem.GetComponent<Rigidbody>();

        bool hadRigidbody = itemRigidbody != null;

        if (hadRigidbody) {
            itemRigidbody.isKinematic = true;
        }

        // Moves the item upward first and then downward.
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

        // Re-enables physics after landing.
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
            _ => false
        };

        if (isValid) {
            action?.Invoke();
        }
    }

    /* private void OnDrawGizmosSelected() {
        // Shows the range and spawn area in the editor.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        Gizmos.color = Color.green;
        Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position + (Vector3.up * spawnHeight);
        Gizmos.DrawWireSphere(center, 0.2f);
    } */
}

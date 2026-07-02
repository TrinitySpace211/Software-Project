using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Opens a chest and creates random loot when F is pressed.
/// Works together with the LootChest script.
/// </summary>
public class LootChestItemDrop : MonoBehaviour {
    [Header("References")]
    // The player can be assigned in the Inspector.
    // An empty field is found automatically.
    [SerializeField] private Player player;

    [Header("Chest Opening")]
    // Optional lid that rotates when the chest is opened.
    [SerializeField] private MeshRenderer lidMeshRenderer;
    [SerializeField] private Transform lidPivot;
    [SerializeField] private Transform lid;

    [Header("Loot Spawn")]
    // Random loot is selected from this list.
    [SerializeField] private List<ItemSO> itemPrefabs;
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

    private Material lidMaterial;
    private bool playerInRange;
    private bool waitsForPlayerToLeave;
    private bool hasSpawnedLoot;
    private Light revealLight;

    private List<ItemSO> itemToSpawn;

    //Dissolve
    private bool dissolveLid = false;
    private float dissolveMeterMin;
    private float dissolveMeterMax;
    private float dissolveMeter;
    private float dissolveSpeed = 1f;

    private void Start() {
        CreateRevealLight();

        // Automatically ensures that the chest receives new loot every day.
        if (GetComponent<DailyLootChestReset>() == null) {
            gameObject.AddComponent<DailyLootChestReset>();
        }

        if (lidMeshRenderer != null) {
            lidMaterial = lidMeshRenderer.material;
        }

        Shader shader = lidMaterial.shader;
        int propertyIndex = shader.FindPropertyIndex("_DissolveMeter");
        dissolveMeterMin = lidMaterial.shader.GetPropertyRangeLimits(propertyIndex).x;
        dissolveMeterMax = lidMaterial.shader.GetPropertyRangeLimits(propertyIndex).y;
        dissolveMeter = dissolveMeterMax;
    }

    private void Update() {
        // Continuously checks the player and the key.
        CheckInteractionInput();

        if (dissolveLid) {
            dissolveMeter -= Time.deltaTime * dissolveSpeed;
            if (dissolveMeter > dissolveMeterMin) {
                lidMaterial.SetFloat("_DissolveMeter", dissolveMeter);
            } else {
                dissolveLid = false;
                lid.gameObject.SetActive(false);
            }
        }
    }

    private void CheckInteractionInput() {
        // Waits after opening until the player leaves the area.
        if (waitsForPlayerToLeave || !playerInRange) {
            return;
        }

        if (player.GetPlayerInputHandler().InteractTriggered) {
            OpenAndDropItems();
            player.GetPlayerInputHandler().SetInteractInput(false);
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
        Rigidbody rb = lid.GetComponent<Rigidbody>();

        if (rb != null) {
            rb.isKinematic = false;
            rb.linearVelocity = new Vector3(UnityEngine.Random.Range(0, 3f), 5f, UnityEngine.Random.Range(0, 3f));
            rb.angularVelocity = new Vector3(
                UnityEngine.Random.Range(-10f, 10f),
                UnityEngine.Random.Range(-10f, 10f),
                UnityEngine.Random.Range(-10f, 10f));

            StartCoroutine(DissolveLid());
        } else {
            Debug.LogError("Lid has no Rigidbody!");
            return;
        }
    }

    private void CloseLid() {
        Rigidbody rb = lid.GetComponent<Rigidbody>();

        lidMaterial.SetFloat("_DissolveMeter", 1);

        lid.gameObject.SetActive(true);

        if (rb != null) {
            rb.isKinematic = true;
        }

        lid.position = lidPivot.position;
        lid.localRotation = Quaternion.Euler(0, 0, 0);
    }

    private IEnumerator DissolveLid() {
        yield return new WaitForSeconds(2f);

        dissolveLid = true;
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
        if (itemPrefabs == null || itemPrefabs.Count == 0) {
            Debug.LogWarning($"Loot chest '{name}' has no item prefabs assigned.");
            yield break;
        }

        itemToSpawn = new List<ItemSO>(itemPrefabs);

        // Creates the items with a short delay.
        for (int i = 0; i < itemToSpawn.Count; i++) {
            int index = UnityEngine.Random.Range(i, itemToSpawn.Count);

            (itemToSpawn[i], itemToSpawn[index]) = (itemToSpawn[index], itemToSpawn[i]);
        }

        int itemCount = Mathf.Min(UnityEngine.Random.Range(minItems, maxItems + 1), itemToSpawn.Count);

        // Determines the item count and starts the light effect.
        StartCoroutine(PlayRevealLight());

        for (int i = 0; i < itemCount; i++) {

            ItemSO itemSO = GetWeightedRandomItem();
            if (scrapItem != null && UnityEngine.Random.value < scrapDropChance) {
                itemSO = scrapItem;
            }

            SpawnSingleItem(itemSO, i, itemCount);
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

    private void SpawnSingleItem(ItemSO itemSO, int itemIndex, int itemCount) {
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

        if (itemSO.itemType == ItemType.Gun) {//Doesn't do anything
            SetItemType(() => spawnedItem.SetItemType(itemSO.gunType), itemSO.itemType, itemSO.gunType);
        } else if (itemSO.itemType == ItemType.Melee) {//Doesn't do anything
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

    private ItemSO GetWeightedRandomItem() {
        int totalWeight = 0;

        foreach (ItemSO item in itemToSpawn)
            totalWeight += item.spawnWeight;

        int random = UnityEngine.Random.Range(0, totalWeight);

        foreach (ItemSO item in itemToSpawn) {
            random -= item.spawnWeight;

            if (random < 0) {
                itemToSpawn.Remove(item);
                return item;
            }
        }

        return null;
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
            if (spawnedItem != null) {
                spawnedItem.transform.position = Vector3.Lerp(firstHalf, secondHalf, progress);
                spawnedItem.transform.Rotate(Vector3.up, 120f * Time.deltaTime, Space.World);
            }

            yield return null;
        }


        if (spawnedItem != null)
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
            ItemType.Ammunition => getItemType is AmmunitionType.Ammo556x45mm or AmmunitionType.Ammo9mm or AmmunitionType.Ammo12Gauge or AmmunitionType.AmmoSniper,
            _ => false
        };

        if (isValid) {
            action?.Invoke();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.GetComponentInParent<Player>()) {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.GetComponentInParent<Player>()) {
            playerInRange = false;
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

using UnityEngine;

public enum ZombieLootDeliveryMode {
    // Scrap is added directly to the player's wallet.
    AddDirectlyToPlayer,

    // Scrap appears as a collectible object.
    SpawnPickup
}

/// <summary>
/// Gives the player scrap when a zombie dies.
/// </summary>
public class ZombieLootDrop : MonoBehaviour {
    [Header("Scrap")]
    // Optional scrap item from the project.
    [SerializeField] private ItemSO scrapItem;

    // Amount of scrap given by a zombie.
    [SerializeField] private int scrapAmount = 1;

    // Determines whether scrap is awarded directly or spawned as a pickup.
    [SerializeField] private ZombieLootDeliveryMode deliveryMode = ZombieLootDeliveryMode.AddDirectlyToPlayer;

    [Header("Pickup")]
    // Optional prefab used when scrap should appear as a pickup.
    [SerializeField] private GameObject scrapPickupPrefab;

    // Icon used when no scrap item with an icon was assigned.
    [SerializeField] private Sprite fallbackScrapIcon;

    // Height at which the pickup is spawned.
    [SerializeField] private float pickupSpawnHeight = 0.35f;

    private ZombieAI zombieAI;
    private Animator animator;

    // Prevents a zombie from giving scrap more than once.
    private bool lootGiven;

    private void Awake() {
        // Finds the zombie and animator components on the same object.
        zombieAI = GetComponent<ZombieAI>();
        animator = GetComponent<Animator>();
    }

    private void Update() {
        // Does nothing if the loot was already awarded.
        if (lootGiven) {
            return;
        }

        // Checks whether ZombieAI reports that the zombie is dead.
        if (zombieAI != null && zombieAI.IsDead()) {
            GiveLootToCurrentPlayer();
            return;
        }

        // Checks whether the animator marks the zombie as dead.
        if (animator != null && animator.GetBool("isDead")) {
            GiveLootToCurrentPlayer();
        }
    }

    public void GiveLoot(Transform player) {
        // Safety check: loot may only be awarded once.
        if (lootGiven) {
            return;
        }

        lootGiven = true;

        // Gives at least 1 scrap.
        int amountToGive = Mathf.Max(1, scrapAmount);
        if (amountToGive <= 0) {
            return;
        }

        // Awards scrap directly or spawns a pickup depending on the mode.
        if (deliveryMode == ZombieLootDeliveryMode.SpawnPickup) {
            SpawnScrapPickup(amountToGive);
            return;
        }

        AddScrapToPlayer(player, amountToGive);
    }

    public void GiveLootToCurrentPlayer() {
        // First looks directly for the player's scrap wallet.
        PlayerScrapWallet wallet = FindFirstObjectByType<PlayerScrapWallet>();
        if (wallet != null) {
            GiveLoot(wallet.transform);
            return;
        }

        // Looks for the player by tag if no wallet was found.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GiveLoot(player != null ? player.transform : null);
    }

    private void AddScrapToPlayer(Transform player, int scrapAmount) {
        // Finds the scrap wallet and adds the scrap.
        PlayerScrapWallet wallet = FindPlayerWallet(player);

        if (wallet == null) {
            Debug.LogWarning("Zombie dropped scrap, but no player wallet could be found.");
            return;
        }

        wallet.AddScrap(scrapAmount);
    }

    private PlayerScrapWallet FindPlayerWallet(Transform player) {
        // Looks for the player by tag if no player was provided.
        if (player == null) {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject != null ? playerObject.transform : null;
        }

        // Looks for any wallet in the scene if no player was found.
        if (player == null) {
            PlayerScrapWallet sceneWallet = FindFirstObjectByType<PlayerScrapWallet>();
            return sceneWallet;
        }

        // Looks for the wallet on the player, its parents, or its children.
        PlayerScrapWallet wallet = player.GetComponentInParent<PlayerScrapWallet>();
        if (wallet == null) {
            wallet = player.GetComponentInChildren<PlayerScrapWallet>();
        }

        // Performs a final search throughout the scene.
        if (wallet == null) {
            wallet = FindFirstObjectByType<PlayerScrapWallet>();
        }

        // Creates a wallet on the player if none exists.
        if (wallet == null) {
            wallet = player.gameObject.AddComponent<PlayerScrapWallet>();
        }

        return wallet;
    }

    private void SpawnScrapPickup(int scrapAmount) {
        // Creates either the assigned prefab or a simple default pickup.
        GameObject pickupObject = scrapPickupPrefab != null
            ? Instantiate(scrapPickupPrefab, GetPickupPosition(), Quaternion.identity)
            : CreateDefaultPickup();

        // Ensures that the pickup has the correct script.
        ScrapPickup pickup = pickupObject.GetComponent<ScrapPickup>();
        if (pickup == null) {
            pickup = pickupObject.AddComponent<ScrapPickup>();
        }

        pickup.SetAmount(scrapAmount);
    }

    private GameObject CreateDefaultPickup() {
        // Builds a simple pickup if no prefab was assigned.
        GameObject pickupObject = new GameObject("Scrap Pickup");
        pickupObject.transform.position = GetPickupPosition();

        // Trigger that allows the player to collect the pickup.
        SphereCollider collider = pickupObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        // Displays an icon for the scrap pickup.
        SpriteRenderer spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetScrapIcon();
        spriteRenderer.sortingOrder = 5;

        return pickupObject;
    }

    private Vector3 GetPickupPosition() {
        // Positions the pickup slightly above the zombie.
        return transform.position + Vector3.up * pickupSpawnHeight;
    }

    private Sprite GetScrapIcon() {
        // Uses the scrap item's icon first, otherwise the fallback icon.
        if (scrapItem != null && scrapItem.icon != null) {
            return scrapItem.icon;
        }

        return fallbackScrapIcon;
    }
}

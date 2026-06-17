using UnityEngine;

public enum ZombieLootDeliveryMode {
    // Schrott geht direkt auf das Konto vom Spieler.
    AddDirectlyToPlayer,

    // Schrott erscheint als aufsammelbares Objekt.
    SpawnPickup
}

/// <summary>
/// Gibt dem Spieler Schrott, wenn ein Zombie stirbt.
/// </summary>
public class ZombieLootDrop : MonoBehaviour {
    [Header("Scrap")]
    // Optionales Scrap-Item aus dem Projekt.
    [SerializeField] private ItemSO scrapItem;

    // Wie viel Schrott ein Zombie gibt.
    [SerializeField] private int scrapAmount = 1;

    // Legt fest, ob Schrott direkt kommt oder als Pickup gespawnt wird.
    [SerializeField] private ZombieLootDeliveryMode deliveryMode = ZombieLootDeliveryMode.AddDirectlyToPlayer;

    [Header("Pickup")]
    // Optionales Prefab, wenn Schrott als Pickup erscheinen soll.
    [SerializeField] private GameObject scrapPickupPrefab;

    // Icon, falls kein Scrap-Item mit Icon gesetzt wurde.
    [SerializeField] private Sprite fallbackScrapIcon;

    // Höhe, in der der Pickup gespawnt wird.
    [SerializeField] private float pickupSpawnHeight = 0.35f;

    private ZombieAI zombieAI;
    private Animator animator;

    // Verhindert, dass ein Zombie mehrmals Schrott gibt.
    private bool lootGiven;

    private void Awake() {
        // Sucht die Zombie- und Animator-Komponenten auf demselben Objekt.
        zombieAI = GetComponent<ZombieAI>();
        animator = GetComponent<Animator>();
    }

    private void Update() {
        // Wenn Loot schon gegeben wurde, passiert nichts mehr.
        if (lootGiven) {
            return;
        }

        // Prüft, ob der Zombie über ZombieAI tot ist.
        if (zombieAI != null && zombieAI.IsDead()) {
            GiveLootToCurrentPlayer();
            return;
        }

        // Prüft, ob der Animator den Zombie als tot markiert.
        if (animator != null && animator.GetBool("isDead")) {
            GiveLootToCurrentPlayer();
        }
    }

    public void GiveLoot(Transform player) {
        // Sicherheit: Loot darf nur einmal gegeben werden.
        if (lootGiven) {
            return;
        }

        lootGiven = true;

        // Mindestens 1 Schrott geben.
        int amountToGive = Mathf.Max(1, scrapAmount);
        if (amountToGive <= 0) {
            return;
        }

        // Je nach Modus direkt geben oder als Pickup spawnen.
        if (deliveryMode == ZombieLootDeliveryMode.SpawnPickup) {
            SpawnScrapPickup(amountToGive);
            return;
        }

        AddScrapToPlayer(player, amountToGive);
    }

    public void GiveLootToCurrentPlayer() {
        // Sucht zuerst direkt nach dem Schrott-Konto vom Spieler.
        PlayerScrapWallet wallet = FindFirstObjectByType<PlayerScrapWallet>();
        if (wallet != null) {
            GiveLoot(wallet.transform);
            return;
        }

        // Falls kein Konto gefunden wurde, wird der Player über den Tag gesucht.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GiveLoot(player != null ? player.transform : null);
    }

    private void AddScrapToPlayer(Transform player, int scrapAmount) {
        // Sucht das Schrott-Konto und schreibt den Schrott gut.
        PlayerScrapWallet wallet = FindPlayerWallet(player);

        if (wallet == null) {
            Debug.LogWarning("Zombie dropped scrap, but no player wallet could be found.");
            return;
        }

        wallet.AddScrap(scrapAmount);
    }

    private PlayerScrapWallet FindPlayerWallet(Transform player) {
        // Wenn kein Player übergeben wurde, suchen wir ihn über den Player-Tag.
        if (player == null) {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            player = playerObject != null ? playerObject.transform : null;
        }

        // Wenn kein Player gefunden wird, suchen wir irgendein Wallet in der Szene.
        if (player == null) {
            PlayerScrapWallet sceneWallet = FindFirstObjectByType<PlayerScrapWallet>();
            return sceneWallet;
        }

        // Sucht das Wallet am Player oder an seinen Eltern/Kindern.
        PlayerScrapWallet wallet = player.GetComponentInParent<PlayerScrapWallet>();
        if (wallet == null) {
            wallet = player.GetComponentInChildren<PlayerScrapWallet>();
        }

        // Letzte Suche in der ganzen Szene.
        if (wallet == null) {
            wallet = FindFirstObjectByType<PlayerScrapWallet>();
        }

        // Wenn wirklich keins existiert, wird eins am Player erstellt.
        if (wallet == null) {
            wallet = player.gameObject.AddComponent<PlayerScrapWallet>();
        }

        return wallet;
    }

    private void SpawnScrapPickup(int scrapAmount) {
        // Erstellt entweder ein gesetztes Prefab oder einen einfachen Standard-Pickup.
        GameObject pickupObject = scrapPickupPrefab != null
            ? Instantiate(scrapPickupPrefab, GetPickupPosition(), Quaternion.identity)
            : CreateDefaultPickup();

        // Sorgt dafür, dass der Pickup auch das richtige Script hat.
        ScrapPickup pickup = pickupObject.GetComponent<ScrapPickup>();
        if (pickup == null) {
            pickup = pickupObject.AddComponent<ScrapPickup>();
        }

        pickup.SetAmount(scrapAmount);
    }

    private GameObject CreateDefaultPickup() {
        // Baut einen einfachen Pickup, falls kein Prefab gesetzt wurde.
        GameObject pickupObject = new GameObject("Scrap Pickup");
        pickupObject.transform.position = GetPickupPosition();

        // Trigger, damit der Spieler den Pickup einsammeln kann.
        SphereCollider collider = pickupObject.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        // Zeigt ein Icon für den Scrap-Pickup.
        SpriteRenderer spriteRenderer = pickupObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetScrapIcon();
        spriteRenderer.sortingOrder = 5;

        return pickupObject;
    }

    private Vector3 GetPickupPosition() {
        // Position leicht über dem Zombie.
        return transform.position + Vector3.up * pickupSpawnHeight;
    }

    private Sprite GetScrapIcon() {
        // Nimmt zuerst das Icon vom Scrap-Item, sonst das Fallback-Icon.
        if (scrapItem != null && scrapItem.icon != null) {
            return scrapItem.icon;
        }

        return fallbackScrapIcon;
    }
}

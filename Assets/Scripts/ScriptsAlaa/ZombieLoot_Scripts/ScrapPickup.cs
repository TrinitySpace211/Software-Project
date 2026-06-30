using UnityEngine;

/// <summary>
/// Scrap that can be collected by the player.
/// </summary>
[RequireComponent(typeof(Collider), typeof(Item), typeof(Outline))]
public class ScrapPickup : MonoBehaviour {
    // Amount of scrap provided by this pickup.
    [SerializeField] private int scrapAmount = 1;

    // Rotation speed of the pickup.
    [SerializeField] private float rotationSpeed = 90f;

    private Item item;
    private PlayerScrapWallet playerWallet;

    public void SetAmount(int amount) {
        // Sets the amount, but never below 1.
        scrapAmount = Mathf.Max(1, amount);
    }

    private void Awake() {
        // The collider must be a trigger so the player can collect it.
        Collider pickupCollider = GetComponent<Collider>();
        pickupCollider.isTrigger = true;
        item = GetComponent<Item>();
    }

    private void OnEnable() {
        // Waits for the player to collect the pickup with F.
        Item.OnItemCollected += HandleItemCollected;
    }

    private void OnDisable() {
        Item.OnItemCollected -= HandleItemCollected;
    }

    private void Reset() {
        // Used in the editor when the script is newly added.
        Collider pickupCollider = GetComponent<Collider>();
        pickupCollider.isTrigger = true;
    }

    private void Update() {
        // Slowly rotates the pickup to make it easier to see.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other) {
        // Remembers the player without awarding scrap automatically.
        Player player = other.GetComponentInParent<Player>();
        if (player == null) {
            return;
        }

        playerWallet = player.GetComponent<PlayerScrapWallet>();
        if (playerWallet == null) {
            playerWallet = player.gameObject.AddComponent<PlayerScrapWallet>();
        }
    }

    private void HandleItemCollected(Item collectedItem) {
        // Reacts only when this exact scrap pickup is collected with F.
        if (collectedItem != item) {
            return;
        }

        FindPlayerWalletIfMissing();
        if (playerWallet != null) {
            playerWallet.AddScrap(scrapAmount);
        }
    }

    private void FindPlayerWalletIfMissing() {
        if (playerWallet != null) {
            return;
        }

        Player player = FindFirstObjectByType<Player>();
        if (player == null) {
            return;
        }

        playerWallet = player.GetComponent<PlayerScrapWallet>();
        if (playerWallet == null) {
            playerWallet = player.gameObject.AddComponent<PlayerScrapWallet>();
        }
    }
}

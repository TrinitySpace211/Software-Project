using UnityEngine;

/// <summary>
/// Scrap that can be collected by the player.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ScrapPickup : MonoBehaviour {
    // Amount of scrap provided by this pickup.
    [SerializeField] private int scrapAmount = 1;

    // Rotation speed of the pickup.
    [SerializeField] private float rotationSpeed = 90f;

    public void SetAmount(int amount) {
        // Sets the amount, but never below 1.
        scrapAmount = Mathf.Max(1, amount);
    }

    private void Awake() {
        // The collider must be a trigger so the player can collect it.
        Collider pickupCollider = GetComponent<Collider>();
        pickupCollider.isTrigger = true;
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
        // Looks for the player's scrap wallet.
        PlayerScrapWallet wallet = other.GetComponentInParent<PlayerScrapWallet>();

        // Creates a wallet if the player does not have one yet.
        if (wallet == null && other.CompareTag("Player")) {
            wallet = other.gameObject.AddComponent<PlayerScrapWallet>();
        }

        // Does nothing if no player wallet was found.
        if (wallet == null) {
            return;
        }

        // Gives scrap and removes the pickup.
        wallet.AddScrap(scrapAmount);
        Destroy(gameObject);
    }
}
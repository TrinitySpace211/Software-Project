using UnityEngine;

/// <summary>
/// Aufsammelbarer Schrott für den Spieler.
/// </summary>
[RequireComponent(typeof(Collider))]
public class ScrapPickup : MonoBehaviour {
    // Wie viel Schrott dieser Pickup gibt.
    [SerializeField] private int scrapAmount = 1;

    // Drehgeschwindigkeit vom Pickup.
    [SerializeField] private float rotationSpeed = 90f;

    public void SetAmount(int amount) {
        // Die Menge wird gesetzt, aber nie kleiner als 1.
        scrapAmount = Mathf.Max(1, amount);
    }

    private void Awake() {
        // Der Collider muss ein Trigger sein, damit der Spieler ihn einsammeln kann.
        Collider pickupCollider = GetComponent<Collider>();
        pickupCollider.isTrigger = true;
    }

    private void Reset() {
        // Wird im Editor benutzt, wenn das Script neu hinzugefügt wird.
        Collider pickupCollider = GetComponent<Collider>();
        pickupCollider.isTrigger = true;
    }

    private void Update() {
        // Dreht den Pickup leicht, damit man ihn besser sieht.
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other) {
        // Sucht das Schrott-Konto vom Spieler.
        PlayerScrapWallet wallet = other.GetComponentInParent<PlayerScrapWallet>();

        // Falls der Player noch kein Konto hat, wird eins erstellt.
        if (wallet == null && other.CompareTag("Player")) {
            wallet = other.gameObject.AddComponent<PlayerScrapWallet>();
        }

        // Wenn kein Spieler-Konto gefunden wurde, passiert nichts.
        if (wallet == null) {
            return;
        }

        // Schrott geben und Pickup löschen.
        wallet.AddScrap(scrapAmount);
        Destroy(gameObject);
    }
}
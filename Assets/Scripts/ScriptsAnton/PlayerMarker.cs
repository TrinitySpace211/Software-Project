using UnityEngine;

/// <summary>
/// Updates the Position of the Player Marker to the Position of the Player
/// </summary>
public class PlayerMarker : MonoBehaviour {
    [SerializeField] private GameObject player;

    private void LateUpdate() {
        transform.position = new Vector3(player.transform.position.x, 40f, player.transform.position.z);
    }
}

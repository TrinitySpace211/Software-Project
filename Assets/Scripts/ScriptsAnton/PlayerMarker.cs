using UnityEngine;

public class PlayerMarker : MonoBehaviour {
    [SerializeField] private GameObject player;

    private void LateUpdate() {
        transform.position = new Vector3(player.transform.position.x, 40f, player.transform.position.z);
    }
}

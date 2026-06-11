using UnityEngine;

/// <summary>
/// UpdateCursor is used to bring Objects to the Mouse Position so the Weapon of the Player has a target to look at
/// </summary>
public class UpdateCursor : MonoBehaviour {

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Player player;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private bool cursorVisible = false;
    [SerializeField] private float cursorMinDistanceFromPlayer = 1f;
    [SerializeField] private float minRange = 0.8f;
    [SerializeField] private float maxRange = 1.2f;

    private void Start() {
        Cursor.visible = cursorVisible;
    }

    private void LateUpdate() {
        LookAt();
    }

    /// <summary>
    /// The Position of the Object that has this Script attached will be updated to the position of the World Space Mouse Position on the X- and Z-Axis.
    /// After that the Crosshair that is being updated on the Canvas will be placed on the Mouse position.
    /// </summary>
    private void LookAt() {
        //Bewegung des Objekts zum 3D-Raum der Maus
        Vector3 mouseWorldPos = player.GetMouseDirection();
        transform.position = UpdatePositionAroundPlayer(new Vector3(mouseWorldPos.x, Mathf.Clamp(mouseWorldPos.y, minRange, maxRange), mouseWorldPos.z));

        //Crosshair position vom Canvas anpassen
        crosshair.GetComponent<RectTransform>().position = playerInputHandler.MousePosition;
    }

    private Vector3 UpdatePositionAroundPlayer(Vector3 targetPosition) {
        Vector3 playerPos = player.transform.position;
        Vector3 offset = targetPosition - playerPos;
        float distance = new Vector3(offset.x, 0f, offset.z).magnitude;

        //if distance threshold is reached then the object should not get closer
        if (distance < cursorMinDistanceFromPlayer && distance > 0.001f) {
            Vector3 direction = offset.normalized;
            Vector3 clamped = playerPos + direction * cursorMinDistanceFromPlayer; //position update of the object
            clamped.y = targetPosition.y;
            return clamped;
        }

        return targetPosition;
    }
}

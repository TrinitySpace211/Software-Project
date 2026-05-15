using Obscure.SDC;
using UnityEngine;

/// <summary>
/// UpdateCursor is used to bring Objects to the Mouse Position so the Weapon of the Player has a target to look at
/// </summary>
public class UpdateCursor : MonoBehaviour {

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Player player;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private Crosshair crosshair;
    [SerializeField] private bool cursorVisible = false;

    private void LateUpdate() {
        Cursor.visible = cursorVisible;
        LookAt();
    }

    /// <summary>
    /// The position of the Object that has this Script attached will be updated to the position of the World Space Mouse Position on the X- and Z-Axis.
    /// The Y-Axis will be always 1f.
    /// After that the Crosshair that is being updated on the Canvas will be placed on the Mouse position.
    /// </summary>
    private void LookAt() {
        //Bewegung des Objects auf das die Waffe zeigt
        Vector3 mouseWorldPos = player.GetMouseDirection();
        transform.position = new Vector3(mouseWorldPos.x, 1f, mouseWorldPos.z);

        //Crosshair position vom Canvas anpassen
        RectTransform crosshairPos = crosshair.GetComponent<RectTransform>();
        crosshairPos.GetComponent<RectTransform>().position = playerInputHandler.MousePosition;
    }
}

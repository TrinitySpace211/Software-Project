using UnityEngine;

/// <summary>
/// Rotates the Camera around the Player through Inputs, default "Q" and "E"
/// </summary>
public class CameraRotate : MonoBehaviour {
    [SerializeField] private Player player;
    [SerializeField] private float turnSpeed = 10f;
    private float currentYRotation = 0f;
    private PlayerInputHandler playerInputHandler;

    private void Start() {
        currentYRotation = transform.localEulerAngles.y;
        playerInputHandler = player.GetPlayerInputHandler();
    }

    /// <summary>
    /// If TurnRight/TurnLeft gets triggered then the playerCamera gets turned
    /// </summary>
    private void Update() {
        if (DebugController.Instance.GetConsoleVisibility())
            return;

        transform.position = player.transform.position;

        if (playerInputHandler.TurnRightInput) {
            currentYRotation -= turnSpeed * Time.deltaTime;
        }
        if (playerInputHandler.TurnLeftInput) {
            currentYRotation += turnSpeed * Time.deltaTime;
        }

        transform.rotation = Quaternion.Euler(0, currentYRotation, 0);
    }
}

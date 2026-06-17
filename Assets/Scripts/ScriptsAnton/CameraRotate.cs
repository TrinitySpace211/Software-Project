using UnityEngine;

public class CameraRotate : MonoBehaviour {
    [SerializeField] private Player player;
    [SerializeField] private float turnSpeed = 10f;
    private float currentYRotation = 0f;
    private PlayerInputHandler playerInputHandler;

    private void Start() {
        currentYRotation = transform.localEulerAngles.y;
        playerInputHandler = player.GetPlayerInputHandler();
    }

    private void Update() {
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

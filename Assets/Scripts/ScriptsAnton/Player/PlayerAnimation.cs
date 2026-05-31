using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The PlayerAnimation Script handles the animations for the Player by updating the float parameters inputX, inputY and inputMagnitude
/// </summary>
public class PlayerAnimation : MonoBehaviour {

    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private float locomotionBlendSpeed = 10f;

    private Vector3 currentBlendInput = Vector3.zero;
    private CurrentPlayerState playerState;
    private bool isSprinting;

    private int inputXHash = Animator.StringToHash("inputX");
    private int inputYHash = Animator.StringToHash("inputY");
    private int inputyMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private int rotationMismatchHash = Animator.StringToHash("rotationMismatch");
    private int getHitHash = Animator.StringToHash("GetHit");
    private int isWeaponAiming = Animator.StringToHash("IsWeaponAiming");

    private void Start() {
        playerState = GetComponent<CurrentPlayerState>();
    }

    private void Update() {
        UpdateAnimationState();
    }

    public void Construct(PlayerInputHandler playerInputHandler) {
        this.playerInputHandler = playerInputHandler;
    }

    /// <summary>
    /// Updates the Animations. First, it checks if the Player is Sprinting so that it can multiply the value of the "inputTarget" by 1.5f.
    /// Then it calculates the direction in the world and the dot products so that the animations are going to be played relative to the direction of the mouse and not the direction to the world
    /// At the end the float parameters will be updated
    /// </summary>
    private void UpdateAnimationState() {
        isSprinting = playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

        Vector2 inputTarget = isSprinting ? playerInputHandler.MovementInput * 1.5f : playerInputHandler.MovementInput;

        //World-Richtung aus Eingabe
        Vector3 worldDir = new Vector3(inputTarget.x, 0f, inputTarget.y);

        //In lokale Richtung bringen
        float inputX = Vector3.Dot(worldDir, transform.right);
        float inputY = Vector3.Dot(worldDir, transform.forward);

        currentBlendInput = Vector3.Lerp(currentBlendInput, new Vector2(inputX, inputY), locomotionBlendSpeed * Time.deltaTime);

        animator.SetFloat(inputXHash, currentBlendInput.x);
        animator.SetFloat(inputYHash, currentBlendInput.y);
        animator.SetFloat(inputyMagnitudeHash, currentBlendInput.magnitude);

        //Trigger die Hit Reaction wenn Space gedrückt wird
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            animator.SetTrigger(getHitHash);
        }
    }

    public void SetAimAnimation(bool state) {
        animator.SetBool(isWeaponAiming, state);
    }

    public void SetRotationMismatch(float mismatch) {
        animator.SetFloat(rotationMismatchHash, mismatch);
    }
}

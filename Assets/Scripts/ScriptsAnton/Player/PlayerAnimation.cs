using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The PlayerAnimation Script handles the animations for the Player by updating the float parameters inputX, inputY and inputMagnitude
/// </summary>
public class PlayerAnimation : MonoBehaviour {

    [SerializeField] private Animator _animator;
    [SerializeField] private PlayerInputHandler _playerInputHandler;
    [SerializeField] private float locomotionBlendSpeed = 10f;

    private Vector3 _currentBlendInput = Vector3.zero;
    private PlayerState _playerState;
    bool isSprinting;

    private int inputXHash = Animator.StringToHash("inputX");
    private int inputYHash = Animator.StringToHash("inputY");
    private int inputyMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private int getHitHash = Animator.StringToHash("GetHit");

    private void Start() {
        _playerState = GetComponent<PlayerState>();
    }

    private void Update() {
        UpdateAnimationState();
    }

    public void Construct(PlayerInputHandler playerInputHandler) {
        this._playerInputHandler = playerInputHandler;
    }

    /// <summary>
    /// Updates the Animations. First, it checks if the Player is Sprinting so that it can multiply the value of the "inputTarget" by 1.5f.
    /// Then it calculates the direction in the world and the dot products so that the animations are going to be played relative to the direction of the mouse and not the direction to the world
    /// At the end the float parameters will be updated
    /// </summary>
    private void UpdateAnimationState() {
        isSprinting = _playerState.CurrentPlayerMovementState == PlayerMovementState.Sprinting;

        Vector2 inputTarget = isSprinting ? _playerInputHandler.MovementInput * 1.5f : _playerInputHandler.MovementInput;

        //World-Richtung aus Eingabe
        Vector3 worldDir = new Vector3(inputTarget.x, 0f, inputTarget.y);

        //In lokale Richtung bringen
        float inputX = Vector3.Dot(worldDir, transform.right);
        float inputY = Vector3.Dot(worldDir, transform.forward);

        _currentBlendInput = Vector3.Lerp(_currentBlendInput, new Vector2(inputX, inputY), locomotionBlendSpeed * Time.deltaTime);

        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputyMagnitudeHash, _currentBlendInput.magnitude);

        //Trigger die Hit Reaction wenn Space gedrückt wird
        if (Keyboard.current.spaceKey.wasPressedThisFrame) {
            _animator.SetTrigger(getHitHash);
        }
    }
}

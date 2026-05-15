using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the inputs of the Player
/// </summary>
[DefaultExecutionOrder(-2)]
public class PlayerInputHandler : MonoBehaviour {
    public InputSystem_Actions playerInputActions { get; private set; }

    public Vector2 MovementInput { get; private set; }
    public bool AttackTriggered { get; private set; }
    public bool JumpTriggered { get; private set; }
    public bool SprintTriggered { get; private set; }
    public Vector2 MousePosition { get; private set; }

    //public event EventHandler OnInteractAction;
    //public event EventHandler OnPauseAction;

    private void OnEnable() {
        playerInputActions = new InputSystem_Actions();
        playerInputActions.Enable();
    }

    private void Start() {
        SubscribeToInputs();
    }

    private void OnDestroy() {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;

        playerInputActions.Dispose();
    }

    /// <summary>
    /// Subscribes and unsubscribes to all Actions that were made in the last frame
    /// </summary>
    private void SubscribeToInputs() {
        playerInputActions.Player.Move.performed += inputInfo => MovementInput = inputInfo.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += _ => MovementInput = Vector2.zero;

        playerInputActions.Player.Look.performed += _ => MousePosition = Mouse.current.position.ReadValue();

        playerInputActions.Player.Attack.performed += _ => AttackTriggered = true;
        playerInputActions.Player.Attack.canceled += _ => AttackTriggered = false;

        playerInputActions.Player.Jump.performed += _ => JumpTriggered = true;
        playerInputActions.Player.Jump.canceled += _ => JumpTriggered = false;

        playerInputActions.Player.Sprint.performed += _ => SprintTriggered = true;
        playerInputActions.Player.Sprint.canceled += _ => SprintTriggered = false;

        playerInputActions.Player.Interact.performed += Interact_performed;
        playerInputActions.Player.Pause.performed += Pause_performed;
    }

    /// <summary>
    /// Sends an Event when the interact key is triggered
    /// </summary>
    /// <param name="context">The context that got send from the input key</param>
    private void Interact_performed(InputAction.CallbackContext context) {
        //OnInteractAction?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Sends an Event when the pause key is triggered
    /// </summary>
    /// <param name="context">The context that got send from the input key</param>
    private void Pause_performed(InputAction.CallbackContext context) {
        //OnPauseAction?.Invoke(this, EventArgs.Empty);
    }
    
    public void SetMovementInput(Vector2 MovementInput) {
        this.MovementInput = MovementInput;
    }

    public void SetMousePosition(Vector2 MousePosition) {
        this.MousePosition = MousePosition;
    }

    public void SetAttackTriggered(bool AttackTriggered) {
        this.AttackTriggered = AttackTriggered;
    }

    public void SetSprintInput(bool SprintTriggered) {
        this.SprintTriggered = SprintTriggered;
    }

}

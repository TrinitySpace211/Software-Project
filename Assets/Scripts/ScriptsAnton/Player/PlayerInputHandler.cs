using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles the inputs of the Player
/// </summary>
[DefaultExecutionOrder(-2)]
public class PlayerInputHandler : MonoBehaviour {
    public InputSystem_Actions playerInputActions { get; private set; }

    public Vector2 MousePosition { get; private set; }
    public Vector2 MovementInput { get; private set; }
    public bool AttackTriggered { get; private set; }
    public bool AimingTriggered { get; private set; }
    public bool SprintTriggered { get; private set; }
    public bool InteractTriggered { get; private set; }
    public bool ThrowTriggered { get; private set; }
    public bool UseTriggered { get; private set; }
    public bool TurnRightInput { get; private set; }
    public bool TurnLeftInput { get; private set; }
    public bool DeselectWeaponTriggered { get; private set; }

    //Hotbar Key Events
    public event Action<int> OnHotbarSlotPressed;

    public static event Action OnInteractAction;
    public static event Action OnPauseAction;
    public static event Action OnReloadAction;
    public static event Action OnToggleDebugAction;
    public static event Action OnReturnAction;

    private void OnEnable() {
        playerInputActions = new InputSystem_Actions();
        playerInputActions.Enable();
    }

    private void Start() {
        SubscribeToPlayerInputs();
        SubscribeToUIInputs();
    }

    private void OnDestroy() {
        playerInputActions.Player.Interact.performed -= Interact_performed;
        playerInputActions.Player.Pause.performed -= Pause_performed;
        playerInputActions.Player.Reloading.performed -= Reloading_performed;
        playerInputActions.Player.ToggleDebug.performed -= ToggleDebug_performed;
        playerInputActions.Player.Return.performed -= Return_performed;

        playerInputActions.UI.One.performed -= HotbarKey_Pressed;
        playerInputActions.UI.Two.performed -= HotbarKey_Pressed;
        playerInputActions.UI.Three.performed -= HotbarKey_Pressed;
        playerInputActions.UI.Four.performed -= HotbarKey_Pressed;
        playerInputActions.UI.Five.performed -= HotbarKey_Pressed;

        playerInputActions.Dispose();
    }

    /// <summary>
    /// Subscribes and unsubscribes to all Actions that were made in the last frame
    /// </summary>
    private void SubscribeToPlayerInputs() {
        playerInputActions.Player.Move.performed += inputInfo => MovementInput = inputInfo.ReadValue<Vector2>();
        playerInputActions.Player.Move.canceled += _ => MovementInput = Vector2.zero;

        playerInputActions.Player.TurnRight.performed += _ => TurnRightInput = true;
        playerInputActions.Player.TurnRight.canceled += _ => TurnRightInput = false;

        playerInputActions.Player.TurnLeft.performed += _ => TurnLeftInput = true;
        playerInputActions.Player.TurnLeft.canceled += _ => TurnLeftInput = false;

        playerInputActions.Player.Look.performed += _ => MousePosition = Mouse.current.position.ReadValue();

        playerInputActions.Player.Attack.performed += _ => AttackTriggered = true;
        playerInputActions.Player.Attack.canceled += _ => AttackTriggered = false;

        playerInputActions.Player.Aiming.performed += _ => AimingTriggered = true;
        playerInputActions.Player.Aiming.canceled += _ => AimingTriggered = false;

        playerInputActions.Player.Sprint.performed += _ => SprintTriggered = true;
        playerInputActions.Player.Sprint.canceled += _ => SprintTriggered = false;

        playerInputActions.Player.Interact.performed += _ => InteractTriggered = true;
        playerInputActions.Player.Interact.canceled += _ => InteractTriggered = false;

        playerInputActions.Player.Throw.performed += _ => ThrowTriggered = true;
        playerInputActions.Player.Throw.canceled += _ => ThrowTriggered = false;

        playerInputActions.Player.Use.performed += _ => UseTriggered = true;
        playerInputActions.Player.Use.canceled += _ => UseTriggered = false;

        playerInputActions.Player.DeselectWeapon.performed += _ => DeselectWeaponTriggered = true;
        playerInputActions.Player.DeselectWeapon.canceled += _ => DeselectWeaponTriggered = false;

        playerInputActions.Player.ToggleDebug.performed += ToggleDebug_performed;
        playerInputActions.Player.Return.performed += Return_performed;

        playerInputActions.Player.Pause.performed += Pause_performed;
        playerInputActions.Player.Reloading.performed += Reloading_performed;
    }

    /// <summary>
    /// Key Inputs 1-5 subscribe to the HotbarKey_Pressed function
    /// If a button gets pressed then the function will be executed
    /// </summary>
    private void SubscribeToUIInputs() {
        playerInputActions.UI.One.performed += HotbarKey_Pressed;
        playerInputActions.UI.Two.performed += HotbarKey_Pressed;
        playerInputActions.UI.Three.performed += HotbarKey_Pressed;
        playerInputActions.UI.Four.performed += HotbarKey_Pressed;
        playerInputActions.UI.Five.performed += HotbarKey_Pressed;

    }

    /// <summary>
    /// Sends an Event when the interact Key is triggered
    /// </summary>
    /// <param name="context">The context that got send from the input key</param>
    private void Interact_performed(InputAction.CallbackContext context) {
        OnInteractAction?.Invoke();
    }

    /// <summary>
    /// Sends an Event when the pause Key is triggered
    /// </summary>
    /// <param name="context">The context that got send from the input key</param>
    private void Pause_performed(InputAction.CallbackContext context) {
        OnPauseAction?.Invoke();
    }

    /// <summary>
    /// Sends an Event when the Reload Key is triggered
    /// </summary>
    /// <param name="context">The context that got send from the input key</param>
    private void Reloading_performed(InputAction.CallbackContext context) {
        OnReloadAction?.Invoke();
    }

    /// <summary>
    /// Sends an Event when the Toggle Debug Input Key is triggered
    /// </summary>
    /// <param name="context">The context that got send from the input key</param>
    private void ToggleDebug_performed(InputAction.CallbackContext context) {
        OnToggleDebugAction?.Invoke();
    }

    /// <summary>
    /// Sends an Event when the Toggle Debug Input Key is triggered
    /// </summary>
    /// <param name="context">The context that got send from the input key</param>
    private void Return_performed(InputAction.CallbackContext context) {
        OnReturnAction?.Invoke();
    }

    /// <summary>
    /// Sends an Event when the Keys 1-5 are pressed
    /// </summary>
    /// <param name="context">The context that got send from the input key</param>
    private void HotbarKey_Pressed(InputAction.CallbackContext context) {
        int slot = context.action switch {
            InputAction action when action == playerInputActions.UI.One => 1,
            InputAction action when action == playerInputActions.UI.Two => 2,
            InputAction action when action == playerInputActions.UI.Three => 3,
            InputAction action when action == playerInputActions.UI.Four => 4,
            InputAction action when action == playerInputActions.UI.Five => 5,
            _ => -1
        };

        if (slot >= 0)
            OnHotbarSlotPressed?.Invoke(slot - 1);
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

    public void SetAimingInput(bool AimingTriggered) {
        this.AimingTriggered = AimingTriggered;
    }

    public void SetSprintInput(bool SprintTriggered) {
        this.SprintTriggered = SprintTriggered;
    }

    public void SetInteractInput(bool InteractTriggered) {
        this.InteractTriggered = InteractTriggered;
    }

    public void SetUseTriggered(bool UseTriggered) {
        this.UseTriggered = UseTriggered;
    }

    public void SetDeselectWeaponTriggered(bool DeselectWeaponTriggered) {
        this.DeselectWeaponTriggered = DeselectWeaponTriggered;
    }
}

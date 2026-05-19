using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// This is the Player Class. The Player Movement, Jumping and Rotation will be calculated.
/// The Method HandleJumping() doesn't have any animations yet.
/// The Player needs the Component "CharacterController" to work properly.
/// The inputs will be handled in the "PlayerInputHandler" Script.
/// </summary>
[DefaultExecutionOrder(-1)]
public class Player : MonoBehaviour {

    #region Class Variables
    [Header("References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Rig aimLayer;

    [Header("UI")]
    [SerializeField] private HealthBar healthBar;
    private PlayerStats playerStats;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintMultipier = 1.5f;
    [SerializeField] private float movingThreshold = 0.01f;

    [Header("Attack Parameters")]
    [SerializeField] private float aimDuration = 0.3f;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravityMultiplier = 1f;

    [Header("Look Parameter")]
    [SerializeField] private float rotationMouseDirAmount = 10f;
    [SerializeField] private float rotationMoveDirAmount = 10f;

    private Vector3 worldMousePos;
    private Vector3 currentMovement;
    private float currentSpeed => walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultipier : 1f);

    private PlayerState _playerState;
    private bool isWalking;
    private bool isMovingLaterally;
    private bool isSprinting;
    #endregion

    private void Start() {

        _playerState = GetComponent<PlayerState>();

        BaseStats baseStats = ScriptableObject.CreateInstance<BaseStats>();

        baseStats.health = 100;
        baseStats.armor = 10;

        StatsMediator mediator = new StatsMediator();

        playerStats = new PlayerStats(mediator, baseStats);

        healthBar.Initialize(playerStats);
    }

    private void Update() {
        UpdateMovementState();
        HandleMovement();
        HandleAiming();

        if (UnityEngine.InputSystem.Keyboard.current
        .altKey.wasPressedThisFrame) {
            playerStats.ChangeHealth(-10);
            healthBar.UpdateHealthBar();
        }
    }

    public void Construct(PlayerInputHandler playerInputHandler, Camera playerCamera) {
        this.playerInputHandler = playerInputHandler;
        this.playerCamera = playerCamera;
    }

    #region Movement

    /// <summary>
    /// Calculates the normalized Direction of the input.
    /// </summary>
    /// <returns>The normalized Direction of the Movement input</returns>
    private Vector3 CalculateWorldDirection() {
        Vector3 inputDirection = new Vector3(playerInputHandler.MovementInput.x, 0f, playerInputHandler.MovementInput.y);
        return inputDirection.normalized;
    }

    /// <summary>
    /// Calculates the Position of the Mouse in 3D Space. 
    /// It shoots out a Ray from the Mouse Position on the Screen on the Ground Layer, so that it only hits the ground
    /// If it hit something, that new point will be the position the Player has to rotate to.
    /// </summary>
    /// <returns>A Vector3 with the Position where the Mouse ist pointing at </returns>
    private Vector3 CalculateMouseDirection() {
        Ray ray = playerCamera.ScreenPointToRay(playerInputHandler.MousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer)) {
            worldMousePos = hit.point;
            return worldMousePos;
        }

        return transform.position;
    }

    /// <summary>
    /// Calculates both the Move direction and Mouse direction the Player has to turn to
    /// When the Player is sprinting then the Player will turn to the Move direction otherwise to Mouse direction
    /// The Movement for the next frame will be calculated via CharacterController.Move()
    /// </summary>
    private void HandleMovement() {
        Vector3 worldDirectionNorm = CalculateWorldDirection();
        currentMovement.x = worldDirectionNorm.x * currentSpeed;
        currentMovement.z = worldDirectionNorm.z * currentSpeed;

        Vector3 mouseDir = CalculateMouseDirection();

        //Blickrichtung ist die Position von der Maus subtrahiert mit der Position des Players 
        Vector3 lookDir = mouseDir - transform.position;
        lookDir.y = 0;

        //Blickrichtung zur Bewegungsrichtung
        Vector3 moveDir = new Vector3(currentMovement.x, 0f, currentMovement.z);

        //Wenn der Spieler sprintet, dann dreht sich der Spieler zum Bewegungsrichtung, sonst zur Mausrichtung
        if (lookDir.sqrMagnitude > 0.1f && !isSprinting) {
            Quaternion targetRotation = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationMouseDirAmount * Time.deltaTime);
        } else if (moveDir.sqrMagnitude > 0.1f && isSprinting) {
            Quaternion targetRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationMoveDirAmount * Time.deltaTime);
        }

        //HandleJumping();
        characterController.Move(currentMovement * Time.deltaTime);
    }

    /// <summary>
    /// This Method handles the Jumping, but due to the Player not having animations it is marked as deprecated for now
    /// </summary>
    [Obsolete("There are no Animations right now for the Player Jumping", true)]
    private void HandleJumping() {
        if (characterController.isGrounded) {
            currentMovement.y = -0.5f;
            if (playerInputHandler.JumpTriggered) {
                currentMovement.y = jumpForce;
            }
        } else {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
    }

    /// <summary>
    /// Handles the Aiming of the Player by increasing/decreasing the weight of the Rig Layer
    /// It switches from RigLayer_WeaponPose to RigLayer_WeaponAiming
    /// </summary>
    private void HandleAiming() {
        if (playerInputHandler.AttackTriggered && !isSprinting) {
            aimLayer.weight += Time.deltaTime / aimDuration;
        } else {
            aimLayer.weight -= Time.deltaTime / aimDuration;
        }
    }

    /// <summary>
    /// Updates the State of the Player Movement
    /// </summary>
    private void UpdateMovementState() {
        //Wenn der Input sich ändert, dann bewegt sich der Spieler
        isWalking = playerInputHandler.MovementInput != Vector2.zero;
        isMovingLaterally = IsMovingLaterally();
        isSprinting = playerInputHandler.SprintTriggered && isMovingLaterally;

        PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting : isMovingLaterally || isWalking ? PlayerMovementState.Walking : PlayerMovementState.Idling;

        _playerState.SetPlayerMovementState(lateralState);
    }
    #endregion

    #region State Checks

    /// <summary>
    /// Checks if the Player moves
    /// </summary>
    /// <returns>True when the Player moves, false if he stands still</returns>
    public bool IsWalking() {
        return isWalking;
    }

    /// <summary>
    /// Checks the Players current Movement
    /// </summary>
    /// <returns>True if the Player moved in the last frame and false if he stands still</returns>
    public bool IsMovingLaterally() {
        Vector3 lateralMovement = new Vector3(currentMovement.x, 0f, currentMovement.z);
        return lateralMovement.magnitude > movingThreshold;
    }

    /// <summary>
    /// Checks if the Player is pressing the Sprint key and if he moves
    /// </summary>
    /// <returns>True while he is in the Sprinting state, false otherwise</returns>
    public bool IsSprinting() {
        return isSprinting;
    }
    #endregion

    #region Setter and Getter

    public Vector3 GetMouseDirection() {
        return worldMousePos;
    }

    public Vector3 GetCurrentPlayerMovement() {
        return currentMovement;
    }

    public float GetCurrentSpeed() {
        return currentSpeed;
    }

    public float GetAimLayerWeight() {
        return aimLayer.weight;
    }

    //Wichtig für die HealthBarTests
    public PlayerStats GetStats() => playerStats;
    public void SetStats(PlayerStats stats) {
        playerStats = stats;
    }

    #endregion

}

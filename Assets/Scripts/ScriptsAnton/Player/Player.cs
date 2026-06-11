using UnityEngine;
using UnityEngine.Animations.Rigging;

/// <summary>
/// This is the Player Class. The Player Movement, Jumping and Rotation will be calculated.
/// The Method HandleJumping() doesn't have any animations yet.
/// The Player needs the Component "CharacterController" to work properly.
/// The inputs will be handled in the "PlayerInputHandler" Script.
/// </summary>
[DefaultExecutionOrder(-1), DisallowMultipleComponent]
public class Player : MonoBehaviour {

    #region Class Variables
    [Header("References")]
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerAnimation playerAnimation;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Rig aimLayer;
    [SerializeField] private BaseStats baseStats;
    [SerializeField] private PlayerWeaponSelector weaponSelector;
    [SerializeField] private CurrentPlayerState playerState;
    [SerializeField] private PlayerIK playerIK;

    [Header("Footsteps Reference")]
    [SerializeField] private ImpactType impactType;
    [SerializeField, Range(0, 1)] private float footstepSoundSpeed;
    [SerializeField] private float footstepSprintOffset = 0.5f;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintMultipier = 1.5f;
    [SerializeField] private float movingThreshold = 0.01f;

    [Header("Aim Parameters")]
    [SerializeField] private float aimDuration = 0.3f;

    [Header("Animation")]
    [SerializeField] private float rotationMouseDirAmount = 10f;
    [SerializeField] private float rotationMoveDirAmount = 10f;

    [Header("Debugging")]
    [SerializeField] private bool alwaysAiming = false;

    //Define Stats for this Player Instance
    public PlayerStats currentPlayerStats { get; private set; }

    //Player Movement Variables
    private Vector3 worldMousePos;
    private Vector3 currentMovement;
    private float currentSpeed => walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultipier : 1f);

    //Movement Checks
    private bool isWalking;
    private bool isMovingLaterally;
    private bool isSprinting;
    private float lastFootPlaced = 0f;
    #endregion

    private void Start() {
        currentPlayerStats = new PlayerStats {
            maxHealth = baseStats.health,
            armor = baseStats.armor
        };
    }

    private void Update() {
        if (!playerHealth.GetIsDead()) {
            UpdateMovementState();
            HandleMovement();
            HandleAiming();

            HandleShooting();
            HandleMeleeAttack();
        }

    }

    /// <summary>
    /// Constructor for testing purposes
    /// </summary>
    /// <param name="playerInputHandler">Needs the PlayerInputHandler class so the Movement can be calculated</param>
    /// <param name="playerCamera">Needs the Main Camera to calculate the Mouse Direction</param>
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
    /// <returns>A Vector3 with the Position where the Mouse is pointing at </returns>
    private Vector3 CalculateMouseDirection() {
        Ray ray = playerCamera.ScreenPointToRay(playerInputHandler.MousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 50f, layerMask)) {
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
        //Calculation of the current Player Direction multiplied with the current Speed (Walk Speed or Sprint Speed)
        Vector3 worldDirectionNorm = CalculateWorldDirection();
        currentMovement.x = worldDirectionNorm.x * currentSpeed;
        currentMovement.z = worldDirectionNorm.z * currentSpeed;

        //Mouse Position in the World
        Vector3 mousePos = CalculateMouseDirection();

        //Direction of the mouse subtracted from the player position
        Vector3 lookDir = mousePos - transform.position;
        lookDir.y = 0f;

        //Direction of the Movement Input
        Vector3 moveDir = new Vector3(currentMovement.x, 0f, currentMovement.z);

        //If the Player is sprinting, then the player will turn to the move direction, otherwise the player will look to the mouse
        UpdatePlayerRotation(moveDir, lookDir);

        //Moves the Character
        characterController.Move(currentMovement * Time.deltaTime);

        // Footstep Sound
        float walkFootstepDelay = 1f / walkSpeed * footstepSoundSpeed;
        float sprintFootstepDelay = 1f / (walkSpeed * sprintMultipier) * footstepSprintOffset;

        if (playerInputHandler.MovementInput != Vector2.zero) {
            if (!isSprinting && Time.time > walkFootstepDelay + lastFootPlaced) {
                lastFootPlaced = Time.time;
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.2f, layerMask)) {
                    SurfaceManager.Instance.HandleImpact(hit.transform.gameObject, hit.point, hit.normal, impactType, hit.triangleIndex);
                }
            } else if (isSprinting && Time.time > sprintFootstepDelay + lastFootPlaced) {
                lastFootPlaced = Time.time;
                if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.2f, layerMask)) {
                    SurfaceManager.Instance.HandleImpact(hit.transform.gameObject, hit.point, hit.normal, impactType, hit.triangleIndex);
                }
            }
        }
    }

    private void UpdatePlayerRotation(Vector3 moveDir, Vector3 lookDir) {
        if (moveDir.sqrMagnitude > 0.1f && (isSprinting || (!playerIK.GetHasWeapon() && !playerIK.GetHasOneHanded()))) {
            RotatePlayerToTarget(moveDir, rotationMoveDirAmount);
        } else if (lookDir.sqrMagnitude > 0.1f && !isSprinting && (playerIK.GetHasWeapon() || playerIK.GetHasOneHanded())) {
            RotatePlayerToTarget(lookDir, rotationMouseDirAmount);
        }
    }

    private void RotatePlayerToTarget(Vector3 direction, float dirAmount) {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dirAmount * Time.deltaTime);
    }
    #endregion

    #region Weapon Aiming, Shooting & Melee Attack 
    /// <summary>
    /// Handles the Aiming of the Player by increasing/decreasing the weight of the Rig Layer
    /// It switches from RigLayer_WeaponPose to RigLayer_WeaponAiming
    /// </summary>
    private void HandleAiming() {
        if (alwaysAiming) {
            //Debug
            aimLayer.weight = 1;
            playerAnimation.SetAimAnimation(true);
        } else {
            if (playerInputHandler.AimingTriggered && !isSprinting && weaponSelector.activeGun != null) {
                aimLayer.weight += Time.deltaTime / aimDuration;
                playerAnimation.SetAimAnimation(true);
            } else {
                aimLayer.weight -= Time.deltaTime / aimDuration;
                playerAnimation.SetAimAnimation(false);
            }
        }
    }

    /// <summary>
    /// When the Player has a gun and is pressing right click (aim) and left click (shoot) the gun will shoot
    /// </summary>
    private void HandleShooting() {
        if (playerInputHandler.AttackTriggered && playerInputHandler.AimingTriggered && weaponSelector.activeGun != null) {
            if (weaponSelector.activeGun != null) {
                weaponSelector.activeGun.Shoot();
            }
        }
    }

    /// <summary>
    /// When the Player has a melee and is pressing left click (attack) the player swings the weapon 
    /// based on if it is a one handed or two handed
    /// </summary>
    private void HandleMeleeAttack() {
        if (playerInputHandler.AttackTriggered) {
            MeleeSO melee = weaponSelector.activeMelee;
            if (melee != null) {
                if (melee.weaponSlot == WeaponSlot.MeleeOneHanded && melee.CanSwing()) {
                    melee.RecordSwing();
                    playerAnimation.SetOneHandMeleeAttack();
                    playerAnimation.SetMeleeAttackSpeed(melee.attackSpeed);
                } else if (melee.weaponSlot == WeaponSlot.MeleeTwoHanded && melee.CanSwing()) {
                    melee.RecordSwing();
                    playerAnimation.SetTwoHandMeleeAttack();
                    playerAnimation.SetMeleeAttackSpeed(melee.attackSpeed);
                }
            }
        }
    }
    #endregion

    #region State Checks
    /// <summary>
    /// Updates the State of the Player Movement not the Movement itself
    /// </summary>
    private void UpdateMovementState() {
        //Wenn der Input sich ändert, dann bewegt sich der Spieler
        isWalking = playerInputHandler.MovementInput != Vector2.zero;
        isMovingLaterally = IsMovingLaterally();
        isSprinting = playerInputHandler.SprintTriggered && isMovingLaterally;

        PlayerMovementState lateralState = isSprinting ? PlayerMovementState.Sprinting : isMovingLaterally || isWalking ? PlayerMovementState.Walking : PlayerMovementState.Idling;

        playerState.SetPlayerMovementState(lateralState);
    }

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

    #region Animation Event
    private void MeleeIsAttacking() {
        weaponSelector.activeMelee.SetMeleeModelAttacking(true);
    }

    private void MeleeIsNotAttacking() {
        weaponSelector.activeMelee.SetMeleeModelAttacking(false);
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

    public CurrentPlayerState GetCurrentPlayerState() {
        return playerState;
    }

    public PlayerIK GetPlayerIK() {
        return playerIK;
    }
    public PlayerWeaponSelector GetPlayerGunSelector() {
        return weaponSelector;
    }
    #endregion

}

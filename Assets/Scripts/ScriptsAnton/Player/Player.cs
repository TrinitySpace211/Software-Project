using System;
using System.Collections.Generic;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

/// <summary>
/// This is the Player Class. The Player Movement and Rotation will be calculated.
/// The Player needs the Component "CharacterController" to work properly.
/// The Inputs will be handled in the "PlayerInputHandler" Script.
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
    [SerializeField] private Inventory inventory;

    [Header("Footsteps Reference")]
    [SerializeField] private ImpactType impactType;
    [SerializeField, Range(0, 1)] private float footstepSoundSpeed;
    [SerializeField] private float footstepSprintOffset = 0.5f;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintMultipier = 1.5f;
    [SerializeField] private float movingThreshold = 0.01f;
    [SerializeField] private float gravityMultiplier = 1f;

    [Header("Aim Parameters")]
    [SerializeField] private float aimDuration = 0.3f;

    [Header("Animation")]
    [SerializeField] private float rotationMouseDirAmount = 10f;
    [SerializeField] private float rotationMoveDirAmount = 10f;

    [Header("Collect Radius")]
    [SerializeField] private float collectRadius = 10f;

    [Header("Grenade Throw")]
    [SerializeField] private float grenadeThrowForce = 10f;

    [Header("Debugging")]
    [SerializeField] private bool alwaysAiming = false;

    private Transform setupWeaponParent;

    //Player Movement Variables
    private Vector3 worldMousePos;
    private Vector3 currentMovement;
    private float currentSpeed => walkSpeed * (playerInputHandler.SprintTriggered ? sprintMultipier : 1f);

    //Movement Checks
    private bool isWalking;
    private bool isMovingLaterally;
    private bool isSprinting;
    private float lastFootPlaced = 0f;

    //Items in Players Range ready to be collected
    private List<Item> itemsInRange = new();

    //Event
    public static event Action<Vector3, GunSO> OnReload;
    public static event Action<Vector3> OnGrenadeThrow;
    public static event Action<Vector3> OnHeal;

    //Other Checks
    private bool isMeleeAttacking = false;
    #endregion

    private void Start() {
        PlayerInputHandler.OnReloadAction += PlayerInputHandler_OnReloadAction;
        Item.OnItemCollected += Item_OnItemCollected;
    }

    private void Update() {
        if (!playerHealth.GetIsDead()) {
            UpdateMovementState();
            HandleMovement();

            HandleShooting();
            HandleAiming();
            if (!EventSystem.current.IsPointerOverGameObject() && !inventory.GetIsDragging()) {
                HandleMeleeAttack();
            }

            HandleReloadFinished();
            HandleGrenade();
            HandleHealingKits();
        }

    }

    private void FixedUpdate() {
        if (playerInputHandler != null && !playerHealth.GetIsDead()) {
            if (playerInputHandler.InteractTriggered) {
                Collider[] hits = Physics.OverlapSphere(transform.position, collectRadius);

                foreach (Collider hit in hits) {
                    Item item = hit.GetComponent<Item>();
                    if (item != null) {
                        itemsInRange.Add(item);
                    }
                }

                HandleInteract();
                playerInputHandler.SetInteractInput(false);
            }
        }
    }

    /// <summary>
    /// Constructor for testing
    /// </summary>
    /// <param name="playerInputHandler">Needs the PlayerInputHandler class so the Movement can be calculated</param>
    /// <param name="playerCamera">Needs the Main Camera to calculate the Mouse Direction</param>
    public void Construct(PlayerInputHandler playerInputHandler, Camera playerCamera) {
        this.playerInputHandler = playerInputHandler;
        this.playerCamera = playerCamera;
    }

    /// <summary>
    /// If the Player pressed the "R" Key and is not Dead then HandleReloadButton() will be called
    /// and the Player is going to Reload
    /// </summary>
    private void PlayerInputHandler_OnReloadAction() {
        if (!playerHealth.GetIsDead()) {
            HandleReloadStart();
        }
    }

    /// <summary>
    /// If the Item got collected it will be removed from the List of collectable Items
    /// </summary>
    /// <param name="item">The Item that has been collected</param>
    private void Item_OnItemCollected(Item item) {
        if (itemsInRange.Contains(item)) {
            itemsInRange.Remove(item);
        }
    }

    #region Movement
    /// <summary>
    /// Calculates the normalized Direction of the input relative to the camera's orientation.
    /// This ensures that "forward" always means towards where the camera is looking.
    /// </summary>
    /// <returns>The normalized Direction of the Movement input in world space</returns>
    public Vector3 CalculateWorldDirection() {
        // Get camera's forward and right directions
        Vector3 cameraForward = playerCamera.transform.forward;
        Vector3 cameraRight = playerCamera.transform.right;

        // Remove vertical component to keep movement on the ground plane
        cameraForward.y = 0f;
        cameraRight.y = 0f;

        // Normalize to avoid diagonal movement being faster
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        // Build movement direction relative to camera orientation
        Vector3 inputDirection = (cameraRight * playerInputHandler.MovementInput.x +
                                  cameraForward * playerInputHandler.MovementInput.y);

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
        HandleFall();

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

    /// <summary>
    /// Updates the Player Rotation to the Movement Input direction 
    /// if the Player is sprinting or has no Weapon
    /// </summary>
    /// <param name="moveDir">The Direction of the Movement Input</param>
    /// <param name="lookDir">The Direction to the Mouse Position in World Space</param>
    private void UpdatePlayerRotation(Vector3 moveDir, Vector3 lookDir) {
        if (moveDir.sqrMagnitude > 0.1f && (isSprinting || (!playerIK.GetHasWeapon() && !playerIK.GetHasOneHanded()))) {
            RotatePlayerToTarget(moveDir, rotationMoveDirAmount);
        } else if (lookDir.sqrMagnitude > 0.1f && !isSprinting && (playerIK.GetHasWeapon() || playerIK.GetHasOneHanded())) {
            RotatePlayerToTarget(lookDir, rotationMouseDirAmount);
        }
    }

    /// <summary>
    /// Rotates the Player to a given target direction
    /// </summary>
    /// <param name="direction">The direction to turn to</param>
    /// <param name="dirAmount">for smoother transition</param>
    private void RotatePlayerToTarget(Vector3 direction, float dirAmount) {
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, dirAmount * Time.deltaTime);
    }

    /// <summary>
    /// If the Player goes over a ledge it checks if the Player is Grounded.
    /// If not then gravity will pull him down.
    /// </summary>
    private void HandleFall() {
        if (characterController.isGrounded) {
            currentMovement.y = -0.5f;
        } else {
            currentMovement.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }
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
            if (playerInputHandler.AimingTriggered && !isSprinting && weaponSelector.activeGun != null && !playerAnimation.GetIsReloading()) {
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
        GunSO activeGun = weaponSelector.activeGun;

        if (playerInputHandler.AttackTriggered && playerInputHandler.AimingTriggered && !isSprinting && activeGun != null && !playerAnimation.GetIsReloading() && !weaponSelector.IsSelecting()) {
            activeGun.Shoot();

            if (activeGun.GetEmptyMagazine() && inventory.GetAmmoAvailable(activeGun, out int ammoNeed)) {
                setupWeaponParent = playerIK.GetParent();
                weaponSelector.ClearSetupCurrentWeapon();
                Debug.Log(ammoNeed);
                playerAnimation.StartReloading(ammoNeed);
                OnReload?.Invoke(transform.position, weaponSelector.activeGun);
            }
        }
    }

    /// <summary>
    /// Handles the Start of the Reload Animation
    /// </summary>
    private void HandleReloadStart() {
        GunSO activeGun = weaponSelector.activeGun;

        if (activeGun != null) {
            if (!activeGun.MagazineIsFull() && !playerAnimation.GetIsReloading() && inventory.GetAmmoAvailable(activeGun, out int ammoNeed)) {
                setupWeaponParent = playerIK.GetParent();
                weaponSelector.ClearSetupCurrentWeapon();
                Debug.Log(ammoNeed);
                playerAnimation.StartReloading(ammoNeed);
                OnReload?.Invoke(transform.position, activeGun);
            }
        }
    }

    /// <summary>
    /// Handles the Setup of the Weapon after Reloading
    /// </summary>
    private void HandleReloadFinished() {
        GunSO activeGun = weaponSelector.activeGun;

        if (activeGun != null) {
            if (setupWeaponParent != null) {
                if ((!activeGun.GetEmptyMagazine() || !activeGun.MagazineIsFull()) && !playerAnimation.GetIsReloading()) {
                    weaponSelector.SetupCurrentWeapon(setupWeaponParent);
                    setupWeaponParent = null;
                }
            }
        }
    }

    /// <summary>
    /// When the Player has a melee and is pressing left click (attack) the player swings the weapon 
    /// based on if it is a one handed or two handed
    /// </summary>
    private void HandleMeleeAttack() {
        if (playerInputHandler.AttackTriggered && !weaponSelector.IsSelecting()) {
            MeleeSO melee = weaponSelector.activeMelee;
            if (melee != null) {
                if (melee.weaponSlot == WeaponSlot.MeleeOneHanded && melee.CanSwing()) {
                    isMeleeAttacking = true;
                    melee.RecordSwing();
                    playerAnimation.SetOneHandMeleeAttack();
                    playerAnimation.SetMeleeAttackSpeed(melee.attackSpeed);
                } else if (melee.weaponSlot == WeaponSlot.MeleeTwoHanded && melee.CanSwing()) {
                    isMeleeAttacking = true;
                    melee.RecordSwing();
                    playerAnimation.SetTwoHandMeleeAttack();
                    playerAnimation.SetMeleeAttackSpeed(melee.attackSpeed);
                }
            }
        }
    }
    #endregion

    #region Item Interact, Grenade Throw & use Heal
    /// <summary>
    /// Handles the Interact Input to collect Items that are dropped via the Loot Chests.
    /// The Item that is closest to the Player is going to be collected first.
    /// </summary>
    private void HandleInteract() {
        if (itemsInRange.Count > 0) {
            Item closestItem = null;
            float closestDistance = float.MaxValue;

            foreach (Item item in itemsInRange) {
                if (item == null) continue;

                float distance = Vector3.Distance(transform.position, item.transform.position);

                if (distance < closestDistance) {
                    closestDistance = distance;
                    closestItem = item;
                }
            }

            if (closestItem != null) {
                closestItem.Collect();
            }
        }
    }

    /// <summary>
    /// Handles the Input of the Grenade Throw, starts the animation and Sound Effect
    /// </summary>
    private void HandleGrenade() {
        Grenade grenade = weaponSelector.activeGrenade;
        if (grenade != null) {
            if (playerInputHandler.ThrowTriggered && !playerAnimation.GetIsThrowingGrenade()) {
                playerAnimation.SetGrenadeAnimation();
                playerAnimation.SetIsThrowingGrenade(true);

                OnGrenadeThrow?.Invoke(grenade.transform.position);
            }
        }
    }

    /// <summary>
    /// Is getting activated via Animation Event.
    /// The Player cuts the connection to the inverse Kinematics so that the Grenade can be moved.
    /// It plays the Throw-Animation and consumes it in the Inventory
    /// </summary>
    private void ThrowGrenade() {
        Grenade grenade = weaponSelector.activeGrenade;
        if (grenade != null) {
            Vector3 throwDirection = (transform.forward + Vector3.up * 0.25f).normalized * grenadeThrowForce;
            grenade.Throw(throwDirection);
            playerIK.ClearSetup();
            weaponSelector.SetGrenadeNull();
            playerAnimation.SetIsThrowingGrenade(false);

            Inventory.Instance.ConsumeEquippedItem(1);
        }
    }

    /// <summary>
    /// Is getting activated via Animation Event.
    /// After the grenade got thrown if the Player still has 
    /// some grenades left in the Inventor it will spawn more.
    /// </summary>
    private void TakeNewGrenade() {
        if (Inventory.Instance.GetSelectedItemAmount() > 0) {
            weaponSelector.SelectGrenade();
        }
    }

    /// <summary>
    /// Player is getting healed when the "Use" Key is pressed, default "F"
    /// </summary>
    private void HandleHealingKits() {
        HealthItemSO healthItem = weaponSelector.activeHealthItem;
        if (healthItem != null) {
            if (playerInputHandler.UseTriggered && playerHealth.stats.currentHealth <= playerHealth.stats.maxHealth) {
                healthItem.Heal(this);

                OnHeal?.Invoke(transform.position);

                Inventory.Instance.ConsumeEquippedItem(1);
                if (Inventory.Instance.GetSelectedItemAmount() <= 0) {
                    weaponSelector.ResetItem();
                    healthItem.DestroySelf();
                }

                playerInputHandler.SetUseTriggered(false);
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

    /// <summary>
    /// Used to allow a hit only at a certain point in the melee animations
    /// </summary>
    private void MeleeIsAttacking() {
        if (weaponSelector.activeMelee != null) {
            weaponSelector.activeMelee.SetMeleeModelAttacking(true);
        }
    }

    /// <summary>
    /// Does not allow to hit the enemy anymore if the animation was finished
    /// </summary>
    private void MeleeIsNotAttacking() {
        isMeleeAttacking = false;
        if (weaponSelector.activeMelee != null) {
            weaponSelector.activeMelee.SetMeleeModelAttacking(false);
        }
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

    public bool GetIsMeleeAttacking() {
        return isMeleeAttacking;
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

    public PlayerAnimation GetPlayerAnimation() {
        return playerAnimation;
    }

    public PlayerInputHandler GetPlayerInputHandler() {
        return playerInputHandler;
    }

    public PlayerHealth GetPlayerHealth() {
        return playerHealth;
    }

    public Inventory GetInventory() {
        return inventory;
    }

    public Camera GetMainCamera() {
        return playerCamera;
    }
    #endregion

    #region Save and Load
    /* public object Save() {
        return new PlayerData {
            position = transform.position
        };
    }

    public void Load(object data) {
        characterController.enabled = false;
        PlayerData playerData = (PlayerData)data;
        transform.position = playerData.position;
        characterController.enabled = true;
    }

    private class PlayerData {
        public Vector3 position;
    } */
    #endregion
}

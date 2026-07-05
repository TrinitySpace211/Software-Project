using System;
using UnityEngine;

/// <summary>
/// The PlayerAnimation Script handles the animations for the Player by updating the float parameters inputX, inputY and inputMagnitude
/// </summary>
public class PlayerAnimation : MonoBehaviour {

    [SerializeField] private Animator animator;
    [SerializeField] private float locomotionBlendSpeed = 10f;

    private PlayerInputHandler playerInputHandler;
    private Camera mainCamera;

    private Vector3 currentBlendInput = Vector3.zero;
    private Player player;
    private bool isSprinting;
    private bool isReloading = false;
    private bool isThrowingGrenade = false;

    private int inputXHash = Animator.StringToHash("inputX");
    private int inputYHash = Animator.StringToHash("inputY");
    private int inputyMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private int getHitHash = Animator.StringToHash("GetHit");
    private int isDeadHash = Animator.StringToHash("IsDead");
    private int isDeadWithWeaponHash = Animator.StringToHash("IsDeadWithWeapon");
    private int isReloadingHash = Animator.StringToHash("IsReloading");
    private int isWeaponAimingHash = Animator.StringToHash("IsWeaponAiming");
    private int meleeAttack1Hash = Animator.StringToHash("MeleeAttack1");
    private int meleeAttack2Hash = Animator.StringToHash("MeleeAttack2");
    private int grenadeThrowHash = Animator.StringToHash("ThrowGrenade");
    private int meleeAttckSpeedMultHash = Animator.StringToHash("MeleeAttackSpeedMult");

    private int ammoAmount = 0;

    private void Start() {
        player = GetComponent<Player>();
        playerInputHandler = player.GetPlayerInputHandler();
        mainCamera = player.GetMainCamera();
    }

    private void Update() {
        UpdateAnimationState();
    }

    /// <summary>
    /// The Construct for the player Input Handler
    /// </summary>
    /// <param name="playerInputHandler">The Player Input Handler class</param>
    public void Construct(PlayerInputHandler playerInputHandler) {
        this.playerInputHandler = playerInputHandler;
    }

    /// <summary>
    /// Updates the Animations. First, it checks if the Player is Sprinting so that it can multiply the value of the "inputTarget" by 1.5f.
    /// Then it calculates the direction relative to the camera orientation and transforms it into the Player's local space
    /// for animation blending. At the end the float parameters will be updated.
    /// </summary>
    private void UpdateAnimationState() {
        if (DebugController.Instance != null && DebugController.Instance.GetConsoleVisibility())
            return;

        isSprinting = player.GetCurrentPlayerState().CurrentPlayerMovementState == PlayerMovementState.Sprinting;

        // Get raw input and apply sprint multiplier
        float inputX = playerInputHandler.MovementInput.x;
        float inputY = playerInputHandler.MovementInput.y;

        if (isSprinting) {
            inputX *= 1.5f;
            inputY *= 1.5f;
        }

        // Get camera's forward and right directions
        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        // Remove vertical component
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward = cameraForward.normalized;
        cameraRight = cameraRight.normalized;

        // Build world direction relative to camera
        Vector3 worldDir = cameraRight * inputX + cameraForward * inputY;

        // Transform into player's local space
        float localInputX = Vector3.Dot(worldDir, transform.right);
        float localInputY = Vector3.Dot(worldDir, transform.forward);

        currentBlendInput = Vector3.Lerp(currentBlendInput, new Vector2(localInputX, localInputY), locomotionBlendSpeed * Time.deltaTime);

        animator.SetFloat(inputXHash, currentBlendInput.x);
        animator.SetFloat(inputYHash, currentBlendInput.y);
        animator.SetFloat(inputyMagnitudeHash, currentBlendInput.magnitude);
    }

    /// <summary>
    /// Starts the aim Animation
    /// </summary>
    /// <param name="state">the state of the animation</param>
    public void SetAimAnimation(bool state) {
        animator.SetBool(isWeaponAimingHash, state);
    }

    /// <summary>
    /// Starts the hit Animation
    /// </summary>
    public void SetHitTrigger() {
        animator.SetTrigger(getHitHash);
    }

    /// <summary>
    /// Starts the dying animation
    /// </summary>
    public void SetDyingTrigger() {
        animator.SetTrigger(isDeadHash);
    }

    /// <summary>
    /// Starts the dying animation with a weapon in the hand
    /// </summary>
    public void SetDyingWithWeaponTrigger() {
        animator.SetTrigger(isDeadWithWeaponHash);
    }

    /// <summary>
    /// starts the reloading Animation
    /// </summary>
    /// <param name="ammoAmount">the amount of Ammunition to set the current gun</param>
    public void StartReloading(int ammoAmount) {
        isReloading = true;
        this.ammoAmount = ammoAmount;

        animator.SetBool(isReloadingHash, isReloading);
    }

    /// <summary>
    /// After the Animation Ends, an Animation Event will execute this function
    /// It removes the necessery amount needed from the inventory
    /// </summary>
    public void FinishedReloading() {
        isReloading = false;
        animator.SetBool(isReloadingHash, isReloading);

        GunSO activeGun = player.GetPlayerGunSelector().activeGun;
        if (activeGun != null && ammoAmount > 0) {
            activeGun.SetAmmoAmount(ammoAmount);
            Inventory inventory = player.GetInventory();
            ItemSO ammunitionItem = inventory.GetItemSOWithGunType(activeGun.ammunitionType);
            if (ammunitionItem != null) {
                inventory.RemoveItem(ammunitionItem, ammoAmount);
            }
        }
    }

    /// <summary>
    /// Getter if the player plays the Reload Animation right now
    /// </summary>
    /// <returns>true if the Animation is playing false otherwise</returns>
    public bool GetIsReloading() {
        return isReloading;
    }

    /// <summary>
    /// Setter for the boolean if the Throwing-Animation is currently playing
    /// </summary>
    /// <param name="isThrowingGrenade">true if the animation is playing false otherwise</param>
    public void SetIsThrowingGrenade(bool isThrowingGrenade) {
        this.isThrowingGrenade = isThrowingGrenade;
    }

    /// <summary>
    /// Getter if the player is playing the throw Animation for the grenade right now
    /// </summary>
    /// <returns>true if the animation is playing false otherwise</returns>
    public bool GetIsThrowingGrenade() {
        return isThrowingGrenade;
    }

    /// <summary>
    /// Starts the throw Animation for the player
    /// </summary>
    public void SetGrenadeAnimation() {
        animator.SetTrigger(grenadeThrowHash);
    }

    /// <summary>
    /// Starts the attack animation for the one handed melee weapon
    /// Has a 50/50 chance to use one of two attack animations
    /// </summary>
    public void SetOneHandMeleeAttack() {
        int attackVariant = UnityEngine.Random.Range(1, 3); // 1 oder 2
        if (attackVariant == 1) {
            animator.SetTrigger(meleeAttack1Hash);
        } else if (attackVariant == 2) {
            animator.SetTrigger(meleeAttack2Hash);
        }
    }

    /// <summary>
    /// Starts the two handed melee attack Animation
    /// </summary>
    public void SetTwoHandMeleeAttack() {
        animator.SetTrigger(meleeAttack1Hash);
    }

    /// <summary>
    /// Sets the animation speed for the melee weapons
    /// </summary>
    /// <param name="value"></param>
    public void SetMeleeAttackSpeed(float value) {
        animator.SetFloat(meleeAttckSpeedMultHash, value);
    }

}

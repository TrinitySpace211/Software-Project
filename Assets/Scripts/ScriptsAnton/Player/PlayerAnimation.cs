using UnityEngine;

/// <summary>
/// The PlayerAnimation Script handles the animations for the Player by updating the float parameters inputX, inputY and inputMagnitude
/// </summary>
public class PlayerAnimation : MonoBehaviour {

    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInputHandler playerInputHandler;
    [SerializeField] private float locomotionBlendSpeed = 10f;
    [SerializeField] private Camera mainCamera;

    private Vector3 currentBlendInput = Vector3.zero;
    private Player player;
    private bool isSprinting;
    private bool isReloading = false;

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
    private int meleeAttckSpeedMultHash = Animator.StringToHash("MeleeAttackSpeedMult");

    private void Start() {
        player = GetComponent<Player>();
    }
    private void Update() {
        UpdateAnimationState();
    }

    public void Construct(PlayerInputHandler playerInputHandler) {
        this.playerInputHandler = playerInputHandler;
    }

    /// <summary>
    /// Updates the Animations. First, it checks if the Player is Sprinting so that it can multiply the value of the "inputTarget" by 1.5f.
    /// Then it calculates the direction relative to the camera orientation and transforms it into the Player's local space
    /// for animation blending. At the end the float parameters will be updated.
    /// </summary>
    private void UpdateAnimationState() {
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

    public void SetAimAnimation(bool state) {
        animator.SetBool(isWeaponAimingHash, state);
    }

    public void SetHitTrigger() {
        animator.SetTrigger(getHitHash);
    }

    public void SetDyingTrigger() {
        animator.SetTrigger(isDeadHash);
    }

    public void SetDyingWithWeaponTrigger() {
        animator.SetTrigger(isDeadWithWeaponHash);
    }

    public void StartReloading() {
        isReloading = true;
        animator.SetBool(isReloadingHash, isReloading);
    }

    public void FinishedReloading() {
        isReloading = false;
        animator.SetBool(isReloadingHash, isReloading);

        GunSO activeGun = player.GetPlayerGunSelector().activeGun;
        if (activeGun != null) {
            activeGun.SetFullMagazine();
        }
    }

    public bool GetIsReloading() {
        return isReloading;
    }

    public void SetOneHandMeleeAttack() {
        int attackVariant = UnityEngine.Random.Range(1, 3); // 1 oder 2
        if (attackVariant == 1) {
            animator.SetTrigger(meleeAttack1Hash);
        } else if (attackVariant == 2) {
            animator.SetTrigger(meleeAttack2Hash);
        }
    }

    public void SetTwoHandMeleeAttack() {
        animator.SetTrigger(meleeAttack1Hash);
    }

    public void SetMeleeAttackSpeed(float value) {
        animator.SetFloat(meleeAttckSpeedMultHash, value);
    }

}

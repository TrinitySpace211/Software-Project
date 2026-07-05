using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// The Player Inverse Kinematics are set in this class for every weapon and item
/// </summary>
public class PlayerIK : MonoBehaviour {

    [Header("Runtime Filled")]
    public Transform leftHandIKTarget;
    public Transform rightHandIKTarget;
    public Transform leftElbowIKTarget;
    public Transform rightElbowIKTarget;


    [Range(0f, 1f)]
    public float handIKAmount = 1f;
    [Range(0f, 1f)]
    public float elbowIKAmount = 1f;

    private Animator animator;
    private Transform gunParent;
    private int gunLayer;
    private int meleeLayer;
    private int consumableLayer;
    private bool hasWeapon = false;
    private bool hasOneHanded = false;
    private bool hasTwoHanded = false;

    private readonly int switchWeaponHash = Animator.StringToHash("SwitchWeapon");
    private readonly int switchMeleeHash = Animator.StringToHash("SwitchMelee");
    private readonly int hasWeaponHash = Animator.StringToHash("HasWeapon");
    private readonly int hasMeleeOneHandedHash = Animator.StringToHash("HasMeleeOneHand");
    private readonly int hasMeleeTwoHandedHash = Animator.StringToHash("HasMeleeTwoHand");
    private readonly int hasGrenadeHash = Animator.StringToHash("HasGrenade");
    private readonly int hasHealthPackHash = Animator.StringToHash("HasHealthPack");


    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        gunLayer = animator.GetLayerIndex("GunLayer");
        meleeLayer = animator.GetLayerIndex("MeleeLayer");
        consumableLayer = animator.GetLayerIndex("Consumable");
    }

    /// <summary>
    /// Puts the Hands at there positions and sets their weights
    /// If the targets dont exist then the weight will be 0
    /// The Players Hands and Elbows wont stick to the items afterwards
    /// </summary>
    /// <param name="layerIndex">What Animation layer they are set</param>
    private void OnAnimatorIK(int layerIndex) {
        if (leftHandIKTarget != null) {
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handIKAmount);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handIKAmount);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
        } else {
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
        }
        if (rightHandIKTarget != null) {
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, handIKAmount);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handIKAmount);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
        } else {
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
        }
        if (leftElbowIKTarget != null) {
            animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowIKTarget.position);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, elbowIKAmount);
        } else {
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
        }
        if (rightElbowIKTarget != null) {
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowIKTarget.position);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, elbowIKAmount);
        } else {
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
        }
    }

    /// <summary>
    /// Sets up the inverse Kinematic targets
    /// </summary>
    /// <param name="gunParent">The Item</param>
    /// <param name="weaponSlotIndex">Some Items dont need both hands to be set (Two Handed/One Handed Items)</param>
    public void Setup(Transform gunParent, int weaponSlotIndex) {
        this.gunParent = gunParent;
        Transform[] allChildren = gunParent.GetComponentsInChildren<Transform>();
        if (weaponSlotIndex == (int)WeaponSlot.Primary || weaponSlotIndex == (int)WeaponSlot.Secondary || weaponSlotIndex == (int)WeaponSlot.MeleeTwoHanded) {
            leftElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbow");
            rightElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "RightElbow");
            leftHandIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftHand");
            rightHandIKTarget = allChildren.FirstOrDefault(child => child.name == "RightHand");
        } else if (weaponSlotIndex == (int)WeaponSlot.MeleeOneHanded || weaponSlotIndex == (int)WeaponSlot.Consumable) {
            rightElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "RightElbow");
            rightHandIKTarget = allChildren.FirstOrDefault(child => child.name == "RightHand");
        }
    }

    /// <summary>
    /// Clears the IK Targets
    /// </summary>
    public void ClearSetup() {
        leftElbowIKTarget = null;
        rightElbowIKTarget = null;
        leftHandIKTarget = null;
        rightHandIKTarget = null;
    }

    /// <summary>
    /// Starts the Weapon switch Animation 
    /// where the player puts the Weapon above his shoulder and behind his back
    /// </summary>
    public void SwitchWeapon() {
        animator.SetTrigger(switchWeaponHash);
    }

    /// <summary>
    /// Starts the Weapon switch Animation
    /// where the player puts the item behind his back
    /// </summary>
    public void SwitchMelee() {
        animator.SetTrigger(switchMeleeHash);
    }

    /// <summary>
    /// Starts the idle Animation for the One Handed Weapon or the Two Handed Weapon
    /// </summary>
    /// <param name="state">state of the animation</param>
    /// <param name="weaponSlotIndex">One Handed or Two Handed?</param>
    public void SetMelee(bool state, int weaponSlotIndex) {
        animator.SetLayerWeight(meleeLayer, state ? 1 : 0);

        if (state) {
            if (weaponSlotIndex == (int)WeaponSlot.MeleeOneHanded) {
                hasTwoHanded = !state;
                hasOneHanded = state;
                animator.SetBool(hasMeleeTwoHandedHash, !state);
                animator.SetBool(hasMeleeOneHandedHash, state);
            } else if (weaponSlotIndex == (int)WeaponSlot.MeleeTwoHanded) {
                hasOneHanded = !state;
                hasTwoHanded = state;
                animator.SetBool(hasMeleeOneHandedHash, !state);
                animator.SetBool(hasMeleeTwoHandedHash, state);
            }
        } else {
            hasOneHanded = state;
            hasTwoHanded = state;
            animator.SetBool(hasMeleeOneHandedHash, state);
            animator.SetBool(hasMeleeTwoHandedHash, state);
        }
    }

    /// <summary>
    /// Sets the Layer in the Animator so that the Gun Movement Animations can be played
    /// </summary>
    /// <param name="state">state of the Animations</param>
    public void SetGun(bool state) {
        hasWeapon = state;

        animator.SetLayerWeight(gunLayer, state ? 1 : 0);
        animator.SetBool(hasWeaponHash, state);
    }

    /// <summary>
    /// Sets the Consumable Layer in the Animator and sets which consumable item is active
    /// </summary>
    /// <typeparam name="T">HealthItemSO or Grenade Type</typeparam>
    /// <param name="state">state of the Animation</param>
    /// <param name="item">The item of which the type should be switched to</param>
    public void SetConsumable<T>(bool state, T item) {
        animator.SetLayerWeight(consumableLayer, state ? 1 : 0);

        if (item is not null) {
            if (item.GetType() == typeof(HealthItemSO)) {
                animator.SetBool(hasGrenadeHash, !state);
                animator.SetBool(hasHealthPackHash, state);
            } else if (item.GetType() == typeof(Grenade)) {
                animator.SetBool(hasHealthPackHash, !state);
                animator.SetBool(hasGrenadeHash, state);
            } else {
                Debug.LogError("Type not found!");
                return;
            }
        }
    }

    /// <summary>
    /// Getter if the player has a gun
    /// </summary>
    /// <returns>true if he has a gun equiped false otherwise</returns>
    public bool GetHasWeapon() {
        return hasWeapon;
    }

    /// <summary>
    /// Getter for the Item Transform where the IK targets are
    /// </summary>
    /// <returns></returns>
    public Transform GetParent() {
        return gunParent;
    }

    /// <summary>
    /// Getter for if the player has a One Handed Melee Weapon
    /// </summary>
    /// <returns>true if he has a One Handed Melee equiped false otherwise</returns>
    public bool GetHasOneHanded() {
        return hasOneHanded;
    }

    /// <summary>
    /// Getter for if the player has a Two Handed Melee Weapon
    /// </summary>
    /// <returns>true if he has a Two Handed Melee equiped false otherwise</returns>
    public bool GetHasTwoHanded() {
        return hasTwoHanded;
    }

}

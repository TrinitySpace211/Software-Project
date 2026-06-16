using System.Collections;
using System.Linq;
using UnityEngine;

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

    public void ClearSetup() {
        leftElbowIKTarget = null;
        rightElbowIKTarget = null;
        leftHandIKTarget = null;
        rightHandIKTarget = null;
    }

    public void SwitchWeapon() {
        animator.SetTrigger(switchWeaponHash);
    }

    public void SwitchMelee() {
        animator.SetTrigger(switchMeleeHash);
    }

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

    public void SetGun(bool state) {
        hasWeapon = state;

        animator.SetLayerWeight(gunLayer, state ? 1 : 0);
        animator.SetBool(hasWeaponHash, state);
    }

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

    public bool GetHasWeapon() {
        return hasWeapon;
    }

    public Transform GetParent() {
        return gunParent;
    }
    public bool GetHasOneHanded() {
        return hasOneHanded;
    }

    public bool GetHasTwoHanded() {
        return hasTwoHanded;
    }

}

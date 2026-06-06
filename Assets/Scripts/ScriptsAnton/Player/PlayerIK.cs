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
    private bool hasWeapon = false;

    private readonly int switchWeaponHash = Animator.StringToHash("SwitchWeapon");
    private readonly int hasWeaponHash = Animator.StringToHash("HasWeapon");

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        gunLayer = animator.GetLayerIndex("GunLayer");
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

    public void Setup(Transform gunParent) {
        this.gunParent = gunParent;
        Transform[] allChildren = gunParent.GetComponentsInChildren<Transform>();
        leftElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbow");
        rightElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "RightElbow");
        leftHandIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftHand");
        rightHandIKTarget = allChildren.FirstOrDefault(child => child.name == "RightHand");
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

    public void SetGun(bool state) {
        hasWeapon = state;

        animator.SetLayerWeight(gunLayer, state ? 1 : 0);
        animator.SetBool(hasWeaponHash, state);
    }

    public bool GetHasWeapon() {
        return hasWeapon;
    }

    public Transform GetParent() {
        return gunParent;
    }

}

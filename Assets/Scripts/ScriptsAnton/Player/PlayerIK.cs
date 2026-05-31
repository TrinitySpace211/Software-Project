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
    private int gunLayer;
    private bool hasWeapon = false;

    private readonly int isWeaponHash = Animator.StringToHash("HasWeapon");

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        gunLayer = animator.GetLayerIndex("GunLayer");
        SetNoWeapon();
    }

    private void OnAnimatorIK(int layerIndex) {
        if (leftHandIKTarget != null) {
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handIKAmount);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handIKAmount);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
        }
        if (rightHandIKTarget != null) {
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, handIKAmount);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handIKAmount);
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
        }
        if (leftElbowIKTarget != null) {
            animator.SetIKHintPosition(AvatarIKHint.LeftElbow, leftElbowIKTarget.position);
            animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, elbowIKAmount);
        }
        if (rightElbowIKTarget != null) {
            animator.SetIKHintPosition(AvatarIKHint.RightElbow, rightElbowIKTarget.position);
            animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, elbowIKAmount);
        }
    }

    public void Setup(Transform gunParent) {
        Transform[] allChildren = gunParent.GetComponentsInChildren<Transform>();
        leftElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbow");
        rightElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "RightElbow");
        leftHandIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftHand");
        rightHandIKTarget = allChildren.FirstOrDefault(child => child.name == "RightHand");
    }

    public void SetNoWeapon() {
        SetWeaponState(false);
    }

    public void SetWeapon() {
        SetWeaponState(true);
    }

    private void SetWeaponState(bool state) {
        hasWeapon = state;
        animator.SetLayerWeight(gunLayer, state ? 1 : 0);
        animator.SetBool(isWeaponHash, state);
        //animator.SetBool(isWeaponHash, state == WeaponState.Sniper);
        //animator.SetBool(isWeaponHash, state == WeaponState.Shotgun);
    }

    public bool GetHasWeapon() {
        return hasWeapon;
    }

}

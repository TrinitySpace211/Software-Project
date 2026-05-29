using System.Linq;
using UnityEngine;

public class PlayerIK : MonoBehaviour {

    public Transform leftHandIKTarget;
    public Transform rightHandIKTarget;
    public Transform leftElbowIKTarget;
    public Transform rightElbowIKTarget;


    [Range(0f, 1f)]
    public float handIKAmount = 1f;
    [Range(0f, 1f)]
    public float elbowIKAmount = 1f;

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
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

    public void SetGunAssault() {
        animator.SetBool("IsAssault", true);
    }

    public void SetGunToPistol() {
        animator.SetBool("IsPistol", true);
    }

    public void SetGunToSniper() {
        animator.SetBool("IsSniper", true);
    }

    public void SetGunToShotgun() {
        animator.SetBool("IsShotgun", true);
    }

    public void Setup(Transform gunParent) {
        Transform[] allChildren = gunParent.GetComponentsInChildren<Transform>();
        leftElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftElbow");
        rightElbowIKTarget = allChildren.FirstOrDefault(child => child.name == "RightElbow");
        leftHandIKTarget = allChildren.FirstOrDefault(child => child.name == "LeftHand");
        rightHandIKTarget = allChildren.FirstOrDefault(child => child.name == "RightHand");
    }
}

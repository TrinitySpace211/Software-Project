using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour {

    [SerializeField] private Transform[] gunParent;
    [SerializeField] private List<GunSO> guns;
    [SerializeField] private PlayerIK inverseKinematics;
    [SerializeField] private Rig switchLayer;
    [SerializeField] private Rig poseLayer;
    [SerializeField] private float switchDuration;
    [SerializeField] private float poseDuration;

    private GunType gunType;

    [Space]
    [Header("Runtime Filled")]
    [SerializeField] public GunSO activeGun;

    private void Update() {
        /* if (Keyboard.current.digit1Key.wasPressedThisFrame && activeGun != null) {
            //Idle no Weapon
            StartCoroutine(SelectNoWeapon());
        } else if (Keyboard.current.digit2Key.wasPressedThisFrame) {
            //Assault
            SelectAssaultRifle();
        } else if (Keyboard.current.digit3Key.wasPressedThisFrame) {
            //Pistol
            SelectPistol();
        } */
    }

    public void DequipWeapon() {
        StartCoroutine(SelectNoWeapon());
    }

    private IEnumerator SelectNoWeapon() {
        inverseKinematics.ClearSetup();

        switchLayer.weight = 1;
        poseLayer.weight = 0;

        inverseKinematics.SwitchWeapon();

        yield return new WaitForSeconds(0.75f);

        if (activeGun == null) {
            Debug.LogError("There is no active gun in the players hand");
            yield break;
        }

        DespawnActiveGun();

        yield return new WaitForSeconds(0.3f);

        inverseKinematics.SetGun(false);
    }

    public void SelectAssaultRifle() {
        GunSO gun = guns.Find(gun => gun.type == GunType.AssaultRifle);
        int weaponSlotIndex = (int)GunType.AssaultRifle;

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {gunType}");
            return;
        }

        if (activeGun != null) {
            if (activeGun.type == GunType.AssaultRifle) {
                return;
            }
        }

        StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }

    public void SelectPistol() {
        GunSO gun = guns.Find(gun => gun.type == GunType.Pistol);
        int weaponSlotIndex = (int)GunType.Pistol;

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {gunType}");
            return;
        }

        if (activeGun != null) {
            if (activeGun.type == GunType.Pistol) {
                return;
            }
        }

        StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }

    private IEnumerator SelectGun(GunSO gun, int weaponSlotIndex) {
        switchLayer.weight = 1;
        poseLayer.weight = 0;


        inverseKinematics.ClearSetup();
        inverseKinematics.SwitchWeapon();

        yield return new WaitForSeconds(0.75f);

        if (activeGun != null) {
            DespawnActiveGun();
        }

        activeGun = gun;
        gun.Spawn(gunParent[weaponSlotIndex], this);

        yield return new WaitForSeconds(0.3f);

        inverseKinematics.SetGun(true);

        switchLayer.weight = 0;
        poseLayer.weight = 1;

        inverseKinematics.Setup(gunParent[weaponSlotIndex]);
    }

    public void DespawnActiveGun() {
        activeGun.Despawn();
        activeGun = null;
    }

}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour {

    [SerializeField] private Transform[] gunParent;
    [SerializeField] private List<GunSO> guns;
    [SerializeField] private PlayerIK inverseKinematics;

    private GunType gunType;

    [Space]
    [Header("Runtime Filled")]
    [SerializeField] public GunSO activeGun;

    private void Update() {
        if (Keyboard.current.digit1Key.wasPressedThisFrame && activeGun != null) {
            //Idle no Weapon
            inverseKinematics.SetNoWeapon();
            DespawnActiveGun();
        } else if (Keyboard.current.digit2Key.wasPressedThisFrame) {
            //Assault
            SelectAssaultRifle();
        } else if (Keyboard.current.digit3Key.wasPressedThisFrame) {
            //Pistol
            SelectPistol();
        }
    }

    public void SelectAssaultRifle() {
        GunSO gun = guns.Find(gun => gun.type == GunType.AssaultRifle);
        int weaponSlotIndex = (int)GunType.AssaultRifle;

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {gunType}");
            return;
        }

        if (activeGun != null) {
            DespawnActiveGun();
        }

        activeGun = gun;
        gun.Spawn(gunParent[weaponSlotIndex], this);

        inverseKinematics.SetWeapon();
        inverseKinematics.Setup(gunParent[weaponSlotIndex]);
    }

    public void SelectPistol() {
        GunSO gun = guns.Find(gun => gun.type == GunType.Pistol);
        int weaponSlotIndex = (int)GunType.Pistol;

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {gunType}");
            return;
        }

        if (activeGun != null) {
            DespawnActiveGun();
        }

        activeGun = gun;
        gun.Spawn(gunParent[weaponSlotIndex], this);

        inverseKinematics.SetWeapon();
        inverseKinematics.Setup(gunParent[weaponSlotIndex]);
    }

    public void DespawnActiveGun() {
        if (activeGun != null) {
            activeGun.Despawn();
            activeGun = null;
        }
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour {

    [SerializeField] private Transform[] gunParent;
    [SerializeField] private List<GunSO> guns;
    [SerializeField] private Rig switchLayer;
    [SerializeField] private Rig poseLayer;

    private Player player;
    private PlayerIK playerIK;
    private GunType gunType;
    private Coroutine selectCoroutine;

    [Space]
    [Header("Runtime Filled")]
    [SerializeField] public GunSO activeGun;

    private void Start() {
        player = GetComponent<Player>();
        playerIK = player.GetPlayerIK();
    }

    private void StopActiveSelectionCoroutine() {
        if (selectCoroutine != null) {
            StopCoroutine(selectCoroutine);
            selectCoroutine = null;
        }
    }

    public void DequipWeapon() {
        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectNoWeapon());
    }

    public IEnumerator SelectNoWeapon() {
        playerIK.ClearSetup();

        switchLayer.weight = 1;
        poseLayer.weight = 0;

        playerIK.SwitchWeapon();

        yield return new WaitForSeconds(0.75f);

        if (activeGun == null) {
            Debug.LogError("There is no active gun in the players hand");
            selectCoroutine = null;
            yield break;
        }

        DespawnActiveGun();

        yield return new WaitForSeconds(0.3f);

        playerIK.SetGun(false);
        selectCoroutine = null;
    }

    public void SelectAssaultRifle() {
        GunSO gun = guns.Find(gun => gun.type == GunType.AssaultRifle);
        int weaponSlotIndex = (int)GunSlot.Primary;

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {gunType}");
            return;
        }

        if (activeGun != null) {
            if (activeGun.type == GunType.AssaultRifle) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }

    public void SelectPistol() {
        GunSO gun = guns.Find(gun => gun.type == GunType.Pistol);
        int weaponSlotIndex = (int)GunSlot.Secondary;

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {gunType}");
            return;
        }

        if (activeGun != null) {
            if (activeGun.type == GunType.Pistol) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }

    public void SelectShotgun() {
        GunSO gun = guns.Find(gun => gun.type == GunType.Shotgun);
        int weaponSlotIndex = (int)GunSlot.Primary;

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {gunType}");
            return;
        }

        if (activeGun != null) {
            if (activeGun.type == GunType.Shotgun) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }

    public void SelectSniper() {
        GunSO gun = guns.Find(gun => gun.type == GunType.Sniper);
        int weaponSlotIndex = (int)GunSlot.Primary;

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {gunType}");
            return;
        }

        if (activeGun != null) {
            if (activeGun.type == GunType.Sniper) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }

    private IEnumerator SelectGun(GunSO gun, int weaponSlotIndex) {
        switchLayer.weight = 1;
        poseLayer.weight = 0;

        playerIK.ClearSetup();
        playerIK.SwitchWeapon();

        yield return new WaitForSeconds(0.75f);

        if (activeGun != null) {
            DespawnActiveGun();
        }

        activeGun = gun;
        gun.Spawn(gunParent[weaponSlotIndex], this);

        yield return new WaitForSeconds(0.3f);

        playerIK.SetGun(true);

        switchLayer.weight = 0;
        poseLayer.weight = 1;

        playerIK.Setup(gunParent[weaponSlotIndex]);
        selectCoroutine = null;
    }

    public void DieWithWeapon() {
        playerIK.ClearSetup();
        switchLayer.weight = 1;
        poseLayer.weight = 0;
    }

    public void DespawnActiveGun() {
        activeGun.Despawn();
        activeGun = null;
    }

}

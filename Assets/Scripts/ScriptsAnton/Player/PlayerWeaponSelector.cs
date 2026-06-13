using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
public class PlayerWeaponSelector : MonoBehaviour {

    [SerializeField] private Transform[] gunParent;
    [SerializeField] private List<GunSO> guns;
    [SerializeField] private List<MeleeSO> melees;
    [SerializeField] private Rig switchLayer;
    [SerializeField] private Rig poseLayer;
    [SerializeField] private Rig meleeLayer;

    private Player player;
    private PlayerIK playerIK;
    private Coroutine selectCoroutine;

    [Space]
    [Header("Runtime Filled")]
    [SerializeField] public GunSO activeGun;
    [SerializeField] public MeleeSO activeMelee;

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

    #region Dequip Weapons
    public void DequipWeapon() {
        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectNoWeapon());
    }

    public void DequipMelee() {
        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectNoMelee());
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

        playerIK.SetWeapon(false);
        selectCoroutine = null;
    }

    public IEnumerator SelectNoMelee() {
        playerIK.ClearSetup();

        int weaponSlotIndex = CheckWeaponSlotIndex();

        meleeLayer.weight = 0;

        playerIK.SwitchMelee();

        yield return new WaitForSeconds(0.75f);

        if (activeMelee == null) {
            Debug.LogError("There is no active gun in the players hand");
            selectCoroutine = null;
            yield break;
        }

        DespawnActiveMelee();

        yield return new WaitForSeconds(0.3f);

        playerIK.SetMelee(false, weaponSlotIndex);
        playerIK.SetWeapon(false);
        selectCoroutine = null;
    }
    #endregion

    #region Select Melees
    public void SelectKnife() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Knife);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Knife}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        if (activeMelee != null) {
            if (activeMelee.type == MeleeType.Knife) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    public void SelectBaseball() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Baseball_Bat);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Baseball_Bat}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        if (activeMelee != null) {
            if (activeMelee.type == MeleeType.Baseball_Bat) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    public void SelectCrowbar() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Crowbar);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Crowbar}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        if (activeMelee != null) {
            if (activeMelee.type == MeleeType.Crowbar) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    public void SelectHatchet() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Hatchet);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Hatchet}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        if (activeMelee != null) {
            if (activeMelee.type == MeleeType.Hatchet) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    public void SelectSword() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Sword);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Sword}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        if (activeMelee != null) {
            if (activeMelee.type == MeleeType.Sword) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    public void SelectTomahawk() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Tomahawk);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Tomahawk}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        if (activeMelee != null) {
            if (activeMelee.type == MeleeType.Tomahawk) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }
    #endregion

    #region Select Guns
    public void SelectAssaultRifle() {
        GunSO gun = guns.Find(gun => gun.type == GunType.AssaultRifle);

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {GunType.AssaultRifle}");
            return;
        }

        int weaponSlotIndex = (int)gun.weaponSlot;

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

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {GunType.Pistol}");
            return;
        }

        int weaponSlotIndex = (int)gun.weaponSlot;

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

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {GunType.Shotgun}");
            return;
        }

        int weaponSlotIndex = (int)gun.weaponSlot;

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

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {GunType.Sniper}");
            return;
        }

        int weaponSlotIndex = (int)gun.weaponSlot;

        if (activeGun != null) {
            if (activeGun.type == GunType.Sniper) {
                return;
            }
        }

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }
    #endregion

    private IEnumerator SelectMelee(MeleeSO melee, int weaponSlotIndex) {
        switchLayer.weight = 1;
        meleeLayer.weight = 0;

        playerIK.ClearSetup();
        playerIK.SwitchMelee();

        //Wait for Switch Animation until the Player reached his back
        yield return new WaitForSeconds(0.75f);

        //Despawn Current Weapon
        if (activeGun != null) {
            DespawnActiveGun();
        }
        if (activeMelee != null) {
            DespawnActiveMelee();
        }

        //Spawn New Weapon
        activeMelee = melee;
        activeGun = null;
        melee.Spawn(gunParent[weaponSlotIndex]);

        //Continue waiting before snapping the IKs to the Weapon again
        yield return new WaitForSeconds(0.3f);

        playerIK.SetMelee(true, weaponSlotIndex);
        playerIK.SetWeapon(true);

        switchLayer.weight = 0;
        meleeLayer.weight = 1;

        playerIK.Setup(gunParent[weaponSlotIndex], weaponSlotIndex);
        selectCoroutine = null;
    }

    private IEnumerator SelectGun(GunSO gun, int weaponSlotIndex) {
        switchLayer.weight = 1;
        poseLayer.weight = 0;

        playerIK.ClearSetup();
        playerIK.SwitchWeapon();

        //Wait for Switch Animation until the Player reached his back
        yield return new WaitForSeconds(0.75f);

        //Despawn Current Weapon
        if (activeGun != null) {
            DespawnActiveGun();
        }
        if (activeMelee != null) {
            DespawnActiveMelee();
        }

        //Spawn New Weapon
        activeMelee = null;
        activeGun = gun;
        gun.Spawn(gunParent[weaponSlotIndex], this);

        //Continue waiting before snapping the IKs to the Weapon again
        yield return new WaitForSeconds(0.3f);

        playerIK.SetWeapon(true);

        switchLayer.weight = 0;
        poseLayer.weight = 1;

        playerIK.Setup(gunParent[weaponSlotIndex], weaponSlotIndex);
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

    public void DespawnActiveMelee() {
        activeMelee.Despawn();
        activeMelee = null;
    }

    private int CheckWeaponSlotIndex() {
        if (activeMelee.type == MeleeType.Knife) {
            return (int)WeaponSlot.MeleeOneHanded;
        } else {
            return (int)WeaponSlot.MeleeTwoHanded;
        }
    }

}

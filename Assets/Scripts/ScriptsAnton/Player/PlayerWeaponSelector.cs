using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerWeaponSelector : MonoBehaviour {

    [SerializeField] private Transform[] gunParent;
    //[SerializeField] private List<GunSO> guns;
    [SerializeField] private List<MeleeSO> melees;
    [SerializeField] private List<HealthItemSO> healthItems;
    [SerializeField] private Grenade grenade;

    [Header("Rig Layers")]
    [SerializeField] private Rig switchLayer;
    [SerializeField] private Rig poseLayer;
    [SerializeField] private Rig meleeLayer;
    [SerializeField] private Rig consumableLayer;

    [Header("Runtime Filled")]
    [SerializeField] public GunSO activeGun { get; private set; }
    [SerializeField] public MeleeSO activeMelee { get; private set; }
    [SerializeField] public Grenade activeGrenade { get; private set; }
    [SerializeField] public HealthItemSO activeHealthItem { get; private set; }

    private Player player;
    private PlayerIK playerIK;
    private Coroutine selectCoroutine;
    private Grenade grenadeInstance;

    //Events
    public static Action<Vector3> OnShoot;
    public static Action<Vector3> OnReload;

    private void Start() {
        player = GetComponent<Player>();
        playerIK = player.GetPlayerIK();
    }

    /// <summary>
    /// Stops the active Coroutine, so that a new one can be started.
    /// </summary>
    private void StopActiveSelectionCoroutine() {
        if (selectCoroutine != null) {
            ResetItem();
            try {
                StopCoroutine(selectCoroutine);
            } catch (UnityException) {
                // Objekt war bereits zerstört oder Coroutine nicht mehr gültig
            }
            selectCoroutine = null;
        }
    }

    #region Unequip Item
    /// <summary>
    /// Starts a Coroutine to Unequip a Gun.
    /// </summary>
    public void DequipWeapon() {
        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectNoGun());
    }

    /// <summary>
    /// Starts a Coroutine to Unequip a Melee.
    /// </summary>
    public void DequipMelee() {
        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectNoMelee());
    }

    /// <summary>
    /// Starts a Coroutine to Unequip a Grenade.
    /// </summary>
    public void DequipGrenade() {
        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectNoGrenade());
    }

    /// <summary>
    /// Starts a Coroutine to Unequip a Consumable.
    /// </summary>
    public void DequipHealthPack() {
        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectNoHealthPack());
    }

    /// <summary>
    /// Clears the players inverse Kinematic targets so that the IKs
    /// are not attached to the Gun anymore and the Gun can be moved by the switch Animation
    /// </summary>
    public IEnumerator SelectNoGun() {
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

    /// <summary>
    /// Clears the players inverse Kinematic targets so that the IKs
    /// are not attached to the Melee anymore and the Melee can be moved by the switch Animation
    /// </summary>
    public IEnumerator SelectNoMelee() {
        playerIK.ClearSetup();

        int weaponSlotIndex = CheckWeaponSlotIndex();

        meleeLayer.weight = 0;

        playerIK.SwitchMelee();

        yield return new WaitForSeconds(0.75f);

        if (activeMelee == null) {
            Debug.LogError("There is no active melee in the players hand");
            selectCoroutine = null;
            yield break;
        }

        DespawnActiveMelee();

        yield return new WaitForSeconds(0.3f);

        playerIK.SetMelee(false, weaponSlotIndex);
        playerIK.SetGun(false);
        selectCoroutine = null;
    }

    /// <summary>
    /// Clears the players inverse Kinematic targets so that the IKs
    /// are not attached to the Consumable anymore and the Consumable can be moved by the switch Animation
    /// </summary>
    public IEnumerator SelectNoGrenade() {
        playerIK.ClearSetup();

        consumableLayer.weight = 0;

        playerIK.SwitchMelee();

        yield return new WaitForSeconds(0.75f);

        if (activeGrenade == null) {
            Debug.LogError("There is no active grenade in the players hand");
            selectCoroutine = null;
            yield break;
        }

        DespawnActiveGrenade();

        yield return new WaitForSeconds(0.3f);

        playerIK.SetConsumable(false, grenade);
        playerIK.SetGun(false);
        selectCoroutine = null;
    }

    /// <summary>
    /// Clears the players inverse Kinematic targets so that the IKs
    /// are not attached to the Consumable anymore and the Consumable can be moved by the switch Animation
    /// </summary>
    public IEnumerator SelectNoHealthPack() {
        playerIK.ClearSetup();

        consumableLayer.weight = 0;

        playerIK.SwitchMelee();

        yield return new WaitForSeconds(0.75f);

        if (activeHealthItem == null) {
            Debug.LogError("There is no active health pack in the players hand");
            selectCoroutine = null;
            yield break;
        }

        DespawnActiveHealthPack();

        yield return new WaitForSeconds(0.3f);

        playerIK.SetConsumable(false, activeHealthItem);
        selectCoroutine = null;
    }
    #endregion

    #region Select Melees
    /// <summary>
    /// Selects the Knife and starts a Coroutine to select a MeleeWeapon
    /// </summary>
    public void SelectKnife() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Knife);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Knife}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    /// <summary>
    /// Selects the Baseball and starts a Coroutine to select a MeleeWeapon
    /// </summary>
    public void SelectBaseball() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Baseball_Bat);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Baseball_Bat}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    /// <summary>
    /// Selects the Crowbar and starts a Coroutine to select a MeleeWeapon
    /// </summary>
    public void SelectCrowbar() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Crowbar);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Crowbar}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    /// <summary>
    /// Selects the Hatchet and starts a Coroutine to select a MeleeWeapon
    /// </summary>
    public void SelectHatchet() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Hatchet);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Hatchet}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    /// <summary>
    /// Selects the Sword and starts a Coroutine to select a MeleeWeapon
    /// </summary>
    public void SelectSword() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Sword);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Sword}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }

    /// <summary>
    /// Selects the Tomahawk and starts a Coroutine to select a MeleeWeapon
    /// </summary>
    public void SelectTomahawk() {
        MeleeSO melee = melees.Find(melee => melee.type == MeleeType.Tomahawk);

        if (melee == null) {
            Debug.LogError($"No MeleeSO found for MeleeType: {MeleeType.Tomahawk}");
            return;
        }

        int weaponSlotIndex = (int)melee.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectMelee(melee, weaponSlotIndex));
    }
    #endregion

    #region Select Guns
    /// <summary>
    /// Selects the AssaultRifle and starts a Coroutine to select a Gun
    /// </summary>
    public void SelectAssaultRifle(GunSO gun) {
        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {GunType.AssaultRifle}");
            return;
        }

        int weaponSlotIndex = (int)gun.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }

    /// <summary>
    /// Selects the Pistol and starts a Coroutine to select a Gun
    /// </summary>
    public void SelectPistol(GunSO gun) {
        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {GunType.Pistol}");
            return;
        }

        int weaponSlotIndex = (int)gun.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }

    /// <summary>
    /// Selects the Shotgun and starts a Coroutine to select a Gun
    /// </summary>
    public void SelectShotgun(GunSO gun) {
        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {GunType.Shotgun}");
            return;
        }

        int weaponSlotIndex = (int)gun.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }

    /// <summary>
    /// Selects the Sniper and starts a Coroutine to select a Gun
    /// </summary>
    public void SelectSniper(GunSO gun) {
        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {GunType.Sniper}");
            return;
        }

        int weaponSlotIndex = (int)gun.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGun(gun, weaponSlotIndex));
    }
    #endregion

    #region Select Consumable

    /// <summary>
    /// Sets the Parent for the Grenade as a Weapon Slot
    /// </summary>
    public void SelectGrenade() {
        if (grenade == null) {
            Debug.LogError($"No Grenade found: {grenade}");
            return;
        }

        int weaponSlotIndex = (int)grenade.GetWeaponSlot();

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectGrenadeSwitch(grenade, weaponSlotIndex));
    }

    /// <summary>
    /// Sets the Parent for the HealthPack as a Weapon Slot
    /// </summary>
    public void SelectHealthPack(HealthItemType type) {
        HealthItemSO healthItem = healthItems.Find(healthItem => healthItem.type == type);

        if (healthItem == null) {
            Debug.LogError($"No Health Item found: {healthItem}");
            return;
        }

        int weaponSlotIndex = (int)healthItem.weaponSlot;

        StopActiveSelectionCoroutine();
        selectCoroutine = StartCoroutine(SelectHealthPackSwitch(healthItem, weaponSlotIndex));
    }
    #endregion

    #region Equip Item
    /// <summary>
    /// Starts the "Weapon Select" Animation for Melee Weapons and Spawns the Weapon afterwards.
    /// </summary>
    /// <param name="melee">The Weapon that should be selected</param>
    /// <param name="weaponSlotIndex">The Weapon Slot it should spawn at</param>
    private IEnumerator SelectMelee(MeleeSO melee, int weaponSlotIndex) {
        switchLayer.weight = 1;
        poseLayer.weight = 0;
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
        if (activeGrenade != null) {
            DespawnActiveGrenade();
        }
        if (activeHealthItem != null) {
            DespawnActiveHealthPack();
        }

        //Spawn New Weapon
        activeMelee = melee;
        activeGun = null;
        melee.Spawn(gunParent[weaponSlotIndex]);

        //Continue waiting before snapping the IKs to the Weapon again
        yield return new WaitForSeconds(0.3f);

        playerIK.SetMelee(true, weaponSlotIndex);
        playerIK.SetGun(true);
        playerIK.SetConsumable<Grenade>(false, null);

        switchLayer.weight = 0;
        meleeLayer.weight = 1;

        playerIK.Setup(gunParent[weaponSlotIndex], weaponSlotIndex);
        selectCoroutine = null;
    }

    /// <summary>
    /// Starts the "Weapon Select" Animation for Guns and Spawns the Weapon afterwards.
    /// </summary>
    /// <param name="gun">The Weapon that should be selected</param>
    /// <param name="weaponSlotIndex">The Weapon Slot it should spawn at</param>
    private IEnumerator SelectGun(GunSO gun, int weaponSlotIndex) {
        switchLayer.weight = 1;
        poseLayer.weight = 0;
        meleeLayer.weight = 0;

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
        if (activeGrenade != null) {
            DespawnActiveGrenade();
        }
        if (activeHealthItem != null) {
            DespawnActiveHealthPack();
        }

        //Spawn New Weapon
        activeMelee = null;
        activeGun = gun;
        gun.Spawn(gunParent[weaponSlotIndex], this);

        //Continue waiting before snapping the IKs to the Weapon again
        yield return new WaitForSeconds(0.3f);

        playerIK.SetGun(true);
        playerIK.SetMelee(false, -1);
        playerIK.SetConsumable<Grenade>(false, null);

        switchLayer.weight = 0;
        poseLayer.weight = 1;

        playerIK.Setup(gunParent[weaponSlotIndex], weaponSlotIndex);
        selectCoroutine = null;
    }

    /// <summary>
    /// Starts the "Weapon Select" Animation for Items and Spawns the Weapon afterwards.
    /// </summary>
    /// <param name="grenade">The Weapon that should be selected</param>
    /// <param name="weaponSlotIndex">The Weapon Slot it should spawn at</param>
    private IEnumerator SelectGrenadeSwitch(Grenade grenade, int weaponSlotIndex) {
        switchLayer.weight = 1;
        poseLayer.weight = 0;
        consumableLayer.weight = 0;

        playerIK.ClearSetup();
        playerIK.SwitchMelee();

        //Wait for Switch Animation until the Player reached his back
        yield return new WaitForSeconds(0.75f);

        //Despawn Current Item
        if (activeGun != null) {
            DespawnActiveGun();
        }
        if (activeMelee != null) {
            DespawnActiveMelee();
        }
        if (activeGrenade != null) {
            DespawnActiveGrenade();
        }
        if (activeHealthItem != null) {
            DespawnActiveHealthPack();
        }

        if (activeGrenade == null) {
            if (grenadeInstance == null) {
                grenadeInstance = grenade.Spawn(gunParent[weaponSlotIndex]);
            } else {
                grenadeInstance.gameObject.SetActive(true);
            }
        }

        //Spawn New Item
        activeGrenade = grenadeInstance;
        activeHealthItem = null;
        activeMelee = null;
        activeGun = null;

        //Continue waiting before snapping the IKs to the Weapon again
        yield return new WaitForSeconds(0.3f);

        playerIK.SetConsumable(true, grenade);
        playerIK.SetGun(true);
        playerIK.SetMelee(false, -1);

        switchLayer.weight = 0;
        consumableLayer.weight = 1;

        playerIK.Setup(gunParent[weaponSlotIndex], weaponSlotIndex);
        selectCoroutine = null;
    }

    /// <summary>
    /// Starts the "Weapon Select" Animation for Items and Spawns the Weapon afterwards.
    /// </summary>
    /// <param name="grenade"></param>
    /// <param name="weaponSlotIndex"></param>
    /// <returns></returns>
    private IEnumerator SelectHealthPackSwitch(HealthItemSO healthItem, int weaponSlotIndex) {
        switchLayer.weight = 1;
        poseLayer.weight = 0;
        consumableLayer.weight = 0;

        playerIK.ClearSetup();
        playerIK.SwitchMelee();

        //Wait for Switch Animation until the Player reached his back
        yield return new WaitForSeconds(0.75f);

        //Despawn Current Item
        if (activeGun != null) {
            DespawnActiveGun();
        }
        if (activeMelee != null) {
            DespawnActiveMelee();
        }
        if (activeGrenade != null) {
            DespawnActiveGrenade();
        }
        if (activeHealthItem != null) {
            DespawnActiveHealthPack();
        }

        healthItem.Spawn(gunParent[weaponSlotIndex]);

        //Spawn New Item
        activeHealthItem = healthItem;
        activeGrenade = null;
        activeMelee = null;
        activeGun = null;

        //Continue waiting before snapping the IKs to the Weapon again
        yield return new WaitForSeconds(0.3f);

        playerIK.SetConsumable(true, healthItem);
        playerIK.SetGun(false);
        playerIK.SetMelee(false, -1);

        switchLayer.weight = 0;
        consumableLayer.weight = 1;

        playerIK.Setup(gunParent[weaponSlotIndex], weaponSlotIndex);
        selectCoroutine = null;
    }
    #endregion

    /// <summary>
    /// Clears the inverse Kinematics for the current Weapon.
    /// </summary>
    public void ClearSetupCurrentWeapon() {
        playerIK.ClearSetup();
        switchLayer.weight = 1;
        poseLayer.weight = 0;
    }

    /// <summary>
    /// Sets the inverse Kinematic targets for the current Weapon.
    /// </summary>
    /// <param name="gunParent">The Parent the current Weapon was at before</param>
    public void SetupCurrentWeapon(Transform gunParent) {
        if (activeGun != null) {
            switch (activeGun.weaponSlot) {
                case WeaponSlot.Primary:
                    playerIK.Setup(gunParent, (int)WeaponSlot.Primary);
                    break;
                case WeaponSlot.Secondary:
                    playerIK.Setup(gunParent, (int)WeaponSlot.Secondary);
                    break;
            }
        }

        switchLayer.weight = 0;
        poseLayer.weight = 1;
    }

    /// <summary>
    /// Returns if a Coroutine is currently running
    /// </summary>
    /// <returns>The current Coroutine</returns>
    public bool IsSelecting() {
        return selectCoroutine != null;
    }

    #region Despawn Weapons
    /// <summary>
    /// Despawns the currently selected Gun
    /// </summary>
    public void DespawnActiveGun() {
        activeGun.Despawn();
        activeGun = null;
    }

    /// <summary>
    /// Despawns the currently selected Melee
    /// </summary>
    public void DespawnActiveMelee() {
        activeMelee.Despawn();
        activeMelee = null;
    }

    /// <summary>
    /// Despawns the currently selected Grenade
    /// </summary>
    public void DespawnActiveGrenade() {
        activeGrenade = null;
        if (grenadeInstance != null) {
            grenadeInstance.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Despawns the currently selected HealthPack
    /// </summary>
    public void DespawnActiveHealthPack() {
        activeHealthItem.Despawn();
        activeHealthItem = null;
    }

    /// <summary>
    /// Sets the active Grenade null
    /// </summary>
    public void SetGrenadeNull() {
        grenadeInstance = null;
    }
    #endregion

    /// <summary>
    /// Checks if the currently active Melee Weapon is a One Handed Melee Weapon 
    /// or if it is a Two Handed Melee Weapon
    /// </summary>
    /// <returns>The Weapon Slot as an int</returns>
    private int CheckWeaponSlotIndex() {
        if (activeMelee.type == MeleeType.Baseball_Bat
                    || activeMelee.type == MeleeType.Sword) {
            return (int)WeaponSlot.MeleeTwoHanded;
        } else {
            return (int)WeaponSlot.MeleeOneHanded;
        }
    }

    /// <summary>
    /// Sets all active Items null, sets the weight of the Rig Layers to zero,
    /// clears the inverse Kinematics and sets all Animations to false
    /// </summary>
    public void ResetItem() {
        if (activeGun != null) {
            DespawnActiveGun();
        } else if (activeMelee != null) {
            DespawnActiveMelee();
        } else if (activeGrenade != null) {
            DespawnActiveGrenade();
        } else if (activeHealthItem != null) {
            DespawnActiveHealthPack();
        }

        meleeLayer.weight = 0;
        poseLayer.weight = 0;
        consumableLayer.weight = 0;
        playerIK.ClearSetup();

        playerIK.SetGun(false);
        playerIK.SetMelee(false, 0);
        playerIK.SetConsumable<Grenade>(false, null);
        playerIK.SetConsumable<HealthItemSO>(false, null);
    }

    /// <summary>
    /// Destroys the Model of the Gun and sets everything to null
    /// </summary>
    private void OnDestroy() {
        if (selectCoroutine != null) {
            StopCoroutine(selectCoroutine);
            selectCoroutine = null;
        }

        if (activeGun != null) {
            activeGun.DestroyAll();
        }
    }
}

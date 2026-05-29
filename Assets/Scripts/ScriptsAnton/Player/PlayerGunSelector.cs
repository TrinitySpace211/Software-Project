using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour {
    [SerializeField] private GunType gunType;
    [SerializeField] private Transform gunParent;
    [SerializeField] private List<GunSO> guns;
    [SerializeField] private PlayerIK inverseKinematics;

    [Space]
    [Header("Runtime Filled")]
    [SerializeField] public GunSO activeGun;

    private void Start() {
        GunSO gun = guns.Find(gun => gun.type == gunType);

        if (gun == null) {
            Debug.LogError($"No GunSO found for GunType: {gunType}");
            return;
        }

        activeGun = gun;
        gun.Spawn(gunParent, this);

        inverseKinematics.SetGunAssault();
        inverseKinematics.Setup(gunParent);
    }

    public void DespawnActiveGun() {
        activeGun.Despawn();
        Destroy(activeGun);
    }

}

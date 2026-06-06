using UnityEngine;

[CreateAssetMenu(fileName = "ShootConfigSO", menuName = "Guns/Shoot Config SO", order = 2)]
public class ShootConfigSO : ScriptableObject {

    public LayerMask hitMask;
    public Vector3 spread = new Vector3(0.1f, 0.1f, 0.1f);
    public float fireRate = 0.25f;
    public float bulletsPerShoot;
    public int damage;
    public int maxAmmo;
}

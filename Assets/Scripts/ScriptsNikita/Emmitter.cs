using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates and shoots arrows at regular intervals.
/// </summary>
public class Emmitter : MonoBehaviour {
    /// <summary>
    /// The bullet-Prefab which should be spawn.
    /// </summary>
    public GameObject bullet;

    /// <summary>
    /// The Time between 2 arrows.
    /// </summary>
    public float timeInterval = 0.4f;

    /// <summary>
    /// Timer for the countdown until next shot.
    /// </summary>
    private float currentTime;

    /// <summary>
    /// // Reference to the FindEnemy script that stores the currently found enemy.
    /// </summary>
    public FindEnemy findEnemy;

    /// <summary>
    /// Initializes the shooting timer and gets the FindEnemy component from the parent GameObject.
    /// </summary>
    private void Start() {
        // To set the Timer from the inspector (timeInterval is global and public)
        currentTime = timeInterval;

        // Searches in the parent GameObejct the FindEnemy component
        findEnemy = GetComponentInParent<FindEnemy>();
    }

    /// <summary>
    /// Counts down the timer and shoots at the current enemy when the timer reaches zero.
    /// </summary>
    private void Update() {
        if (findEnemy == null) {
            return;
        }

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f) {
            if (findEnemy.zombie != null && !findEnemy.zombie.IsDead()) {
                Shoot(findEnemy.zombie);
                currentTime = timeInterval;
            } else if (findEnemy.sprinter != null && !findEnemy.sprinter.IsDead()) {
                Shoot(findEnemy.sprinter);
                currentTime = timeInterval;
            } else if (findEnemy.tank != null && !findEnemy.tank.IsDead()) {
                Shoot(findEnemy.tank);
                currentTime = timeInterval;
            }
        }
    }

    /// <summary>
    /// Spawns a bullet (arrow) and assigns the given target to it.
    /// </summary>
    private void Shoot<T>(T target) where T : Component, IDamageable {
        if (bullet == null) {
            Debug.LogError("Bullet Prefab fehlt beim Emmitter!");
            return;
        }

        GameObject bulletInstance = Instantiate(
            bullet,
            transform.position,
            transform.rotation
        );

        Bullet bulletScript = bulletInstance.GetComponent<Bullet>();

        if (bulletScript == null) {
            bulletScript = bulletInstance.GetComponentInChildren<Bullet>();
        }

        if (bulletScript != null) {
            bulletScript.SetTarget(target);
        }

        ExplosiveBullet explosiveBulletScript = bulletInstance.GetComponent<ExplosiveBullet>();

        if (explosiveBulletScript == null) {
            explosiveBulletScript = bulletInstance.GetComponentInChildren<ExplosiveBullet>();
        }

        if (explosiveBulletScript != null) {
            explosiveBulletScript.SetTarget(target);
        }
    }
}

using UnityEngine;

/// <summary>
/// Rotates a cannon or turret towards the currently found zombie.
/// This version supports an optional rotation offset for models with a wrong forward direction.
/// </summary>
public class LookAtEnemyCanon : MonoBehaviour {
    /// <summary>
    /// Reference to the FindEnemy script on the tower root.
    /// </summary>
    public FindEnemy enemy;

    /// <summary>
    /// Rotation speed in degrees per second.
    /// Higher values make the cannon rotate faster.
    /// </summary>
    public float rotationSpeed = 360f;

    /// <summary>
    /// Additional rotation offset for models whose forward direction is not correct.
    /// Use this if the cannon points sideways or backwards.
    /// </summary>
    public Vector3 rotationOffset;

    /// <summary>
    /// If true, the cannon only rotates left and right.
    /// </summary>
    public bool rotateOnlyY = true;

    private void Start() {
        if (enemy == null) {
            enemy = GetComponentInParent<FindEnemy>();
        }
    }

    private void Update() {
        if (enemy == null || enemy.zombie == null)
            return;

        Vector3 targetPosition = enemy.zombie.transform.position + Vector3.up * 1f;
        Vector3 direction = targetPosition - transform.position;

        if (rotateOnlyY) {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        targetRotation *= Quaternion.Euler(rotationOffset);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
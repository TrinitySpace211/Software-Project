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

    /// <summary>
    /// Gets the FindEnemy component from the parent object if none was assigned.
    /// </summary>
    private void Start() {
        // Automatically find the FindEnemy component on the parent tower.
        if (enemy == null) {
            enemy = GetComponentInParent<FindEnemy>();
        }
    }

    /// <summary>
    /// Rotates the cannon towards the currently selected enemy every frame.
    /// </summary>
    private void Update() {
        // Stop if there is no FindEnemy component or no active target.
        if (enemy == null || enemy.zombie == null)
            return;

        // Aim slightly above the enemy's origin so the cannon points towards the body.
        Vector3 targetPosition = enemy.zombie.transform.position + Vector3.up * 1f;

        // Calculate the direction from the cannon to the target.
        Vector3 direction = targetPosition - transform.position;

        // Ignore vertical movement if only horizontal rotation is allowed.
        if (rotateOnlyY) {
            direction.y = 0f;
        }

        // Stop if the direction vector is too small to calculate a valid rotation.
        if (direction.sqrMagnitude <= 0.001f)
            return;

        // Calculate the desired rotation that looks towards the target.
        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);

        // Apply an additional rotation offset if required by the model.
        targetRotation *= Quaternion.Euler(rotationOffset);

        // Smoothly rotate the cannon towards the desired rotation.
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
using Mono.Cecil.Cil;
using UnityEngine;

/// <summary>
/// Rotates the tower towards the currently found enemy.
/// </summary>
public class LookAtEnemy : MonoBehaviour 
{
    /// <summary>
    ///  // Reference to the FindEnemy script that stores the currently found enemy.
    /// </summary>
    public FindEnemy enemy;

    /// <summary>
    /// Rotation speed of the tower.
    /// </summary>
    public float rotationSpeed = 180f;

    /// <summary>
    /// Gets the FindEnemy component from the parent GameObject if no reference was assigned.
    /// </summary>
    private void Start()
    {
        if(enemy == null) {
            // Searches in the parent GameObejct the FindEnemy component
            enemy = GetComponentInParent<FindEnemy>(); 
        }
    }

    /// <summary>
    /// Rotates the tower towards the found enemy every frame.
    /// </summary>
    private void Update()
    {
        if(enemy == null || enemy.enemy == null) {

            // return if enemies not found, the tower doesn't rotate in this case
            return; 
        }

        // Calculate the distance between enemy and tower
        Vector3 direction = enemy.enemy.transform.position - transform.position;

        // The y position component should be 0, because tower should not rotate in y-direction
        direction.y = 0f;

        // If the direction variable is close to 0, the process is aborted
        // This prevents errors in Quaternion.LookRotation(direction)
        if (direction.sqrMagnitude <= 0.001f)
            return;

        // Creates a target rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, // Rotation of the tower
            targetRotation, // Desired rotation relative to the opponent
            rotationSpeed * Time.deltaTime // How much rotation is allowed per frame. 
        );
    }
 }

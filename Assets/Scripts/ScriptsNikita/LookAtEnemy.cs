using Mono.Cecil.Cil;
using UnityEngine;

public class LookAtEnemy : MonoBehaviour // This script rotates the tower to the enemy
{
    // Variable to have a reference to the FindEnemy script to story the found enemy
    public FindEnemy enemy;

    // Rotation speed of the tower
    public float rotationSpeed = 180f; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if(enemy == null) {
            enemy = GetComponentInParent<FindEnemy>(); // Searches in the parent GameObejct the FindEnemy component
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if(enemy == null || enemy.enemy == null) {

            // return if enemies not found, the tower doesnt rotate in this case
            return; 
        }

        // Calculate the distance between enemy and tower
        Vector3 direction = enemy.enemy.transform.position - transform.position;

        // The y position component should be 0, because tower should not rotate in y-direction
        direction.y = 0f;

        // If the direction is close to 0, the process is aborted
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

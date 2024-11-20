using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHopTowardsAndAway : MonoBehaviour
{
    public Transform player; // Reference to the player
    public float farDistance = 15f; // Distance at which NPC hops towards the player
    public float closeDistance = 5f; // Distance at which NPC hops away from the player
    public float speed = 5f; // Horizontal speed of the NPC
    public float hopForce = 5f; // Vertical force to simulate the hop
    public float gravity = -9.81f; // Gravity force affecting the NPC
    public float groundCheckDistance = 0.2f; // Raycast distance to check if NPC is grounded
    public float rotationSpeed = 10f; // Speed at which NPC rotates towards movement direction
    public float rotationOffset = -25f; // Angle offset to adjust NPC's forward direction (in degrees)

    private Vector3 velocity; // Current velocity of the NPC (including gravity)
    private bool isGrounded; // Whether or not the NPC is on the ground

    void Start()
    {
        // Find the player GameObject using its tag
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Check if the NPC is grounded using a raycast
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        // Apply gravity if the NPC is not grounded
        if (isGrounded)
        {
            velocity.y = 0f; // Reset vertical velocity when grounded
        }
        else
        {
            velocity.y += gravity * Time.deltaTime; // Apply gravity over time when in the air
        }

        // Calculate the distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Determine the direction towards or away from the player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // Ignore vertical direction for movement

        // Check if the NPC is too far and should hop towards the player
        if (distanceToPlayer > farDistance)
        {
            // Hop towards the player
            HopTowards(direction);
        }
        // Check if the NPC is too close and should hop away from the player
        else if (distanceToPlayer < closeDistance)
        {
            // Hop away from the player
            HopAway(direction);
        }

        // Apply the vertical velocity (gravity or hop force) to the NPC
        transform.position += velocity * Time.deltaTime;
    }

    void HopTowards(Vector3 direction)
    {
        // Only apply hop force if the NPC is grounded
        if (isGrounded)
        {
            velocity.y = hopForce; // Apply vertical hop force
        }

        // Move towards the player
        transform.position += direction * speed * Time.deltaTime;

        // Rotate towards the direction it's moving (towards the player) with offset
        RotateTowards(direction);
    }

    void HopAway(Vector3 direction)
    {
        // Only apply hop force if the NPC is grounded
        if (isGrounded)
        {
            velocity.y = hopForce; // Apply vertical hop force
        }

        // Move away from the player
        transform.position -= direction * 3 * speed * Time.deltaTime;

        // Rotate towards the direction it's moving (away from the player) with offset
        RotateTowards(-direction);
    }

    void RotateTowards(Vector3 direction)
    {
        // Apply rotation offset to the direction vector
        direction = Quaternion.Euler(0, rotationOffset, 0) * direction;

        // Smoothly rotate towards the movement direction, considering the rotation offset
        if (direction.magnitude > 0.1f) // Avoid zero-length direction
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}

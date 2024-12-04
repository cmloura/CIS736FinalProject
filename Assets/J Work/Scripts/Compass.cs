using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Compass : MonoBehaviour
{
    public Transform fwks; // The parent Transform that contains all firework objects
    public Transform player; // Reference to the player, to calculate distance from the player
    public float followSpeed = 50f; // Adjusted follow speed for smooth movement
    public float turnSpeed = 50f;
    public float offsetY = 2.5f; // Vertical offset to keep the compass above the player

    Quaternion defaultRotation = Quaternion.Euler(-90, 0, -45);

    // Update is called once per frame
    void Update()
    {
        // Find the closest firework object
        Transform closestFwk = null;
        float closestDistance = Mathf.Infinity;

        foreach (Transform fwk in fwks)
        {
            float distanceToPlayer = Vector3.Distance(player.position, fwk.position);

            if (distanceToPlayer < closestDistance)
            {
                closestDistance = distanceToPlayer;
                closestFwk = fwk;
            }
        }

        // If we found a closest firework, point the compass towards it
        if (closestFwk != null)
        {
            // Calculate the direction to the closest firework
            Vector3 directionToTarget = closestFwk.position - transform.position;
            directionToTarget.y = 0; // Keep compass rotation flat (only in X-Z plane)

            // Rotate the compass to point in the direction of the closest firework
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation * defaultRotation, turnSpeed * Time.deltaTime);

            // Position the compass directly above the player with the Y offset
            Vector3 targetPosition = player.position + new Vector3(0, offsetY, 0);
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        }
    }
}

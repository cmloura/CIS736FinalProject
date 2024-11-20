using UnityEngine;

public class FireworkInteractor : MonoBehaviour
{
    public string fireworkTag = "FireworkBox"; // Tag for firework objects
    private FireworkBox activeFirework; // To store the detected firework

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering is a firework box
        if (other.CompareTag(fireworkTag))
        {
            activeFirework = other.GetComponent<FireworkBox>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // If the object exiting is the currently active firework box
        if (other.CompareTag(fireworkTag) && other.GetComponent<FireworkBox>() == activeFirework)
        {
            activeFirework = null; // Clear the reference
        }
    }

    void Update()
    {
        // Check for mouse click and active firework box
        if (Input.GetMouseButtonDown(0) && activeFirework != null)
        {
            // Ensure the firework is in front of the character
            Vector3 directionToFirework = activeFirework.transform.position - transform.position;
            if (Vector3.Dot(transform.forward, directionToFirework.normalized) > 0.5f) // Adjust threshold if needed
            {
                activeFirework.ActivateParticles();
            }
        }
    }
}

using UnityEngine;
using TMPro;

public class FireworkInteractor : MonoBehaviour
{
    public string fireworkTag = "FireworkBox"; // Tag for firework objects
    public string chestTag = "TreasureChest"; // Tag for treasure chests

    private FireworkBox activeFirework; // To store the detected firework
    private TreasureChest activeChest; // To store the detected treasure chest

    private int treasure;
    private int fireworks;

    public TextMeshProUGUI treasureText;
    public TextMeshProUGUI fireworkText;

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering is a firework box
        if (other.CompareTag(fireworkTag))
        {
            activeFirework = other.GetComponent<FireworkBox>();
            Debug.Log($"Firework Box Found");
        }
        // Check if the object entering is a treasure chest
        else if (other.CompareTag(chestTag))
        {
            activeChest = other.GetComponent<TreasureChest>();
            Debug.Log($"Treasure Chest Found");
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Clear the firework reference if the exiting object is the active firework box
        if (other.CompareTag(fireworkTag) && other.GetComponent<FireworkBox>() == activeFirework)
        {
            activeFirework = null;
        }
        // Clear the chest reference if the exiting object is the active treasure chest
        else if (other.CompareTag(chestTag) && other.GetComponent<TreasureChest>() == activeChest)
        {
            activeChest = null;
        }
    }

    void Update()
    {
        // Check for mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Activate firework particles if near a firework box
            if (activeFirework != null)
            {
                Vector3 directionToFirework = activeFirework.transform.position - transform.position;
                if (Vector3.Dot(transform.forward, directionToFirework.normalized) > 0.5f) // Adjust threshold if needed
                {
                    activeFirework.ActivateParticles();
                    fireworks++;
                    fireworkText.text = "fireworks: " + fireworks;
                }
            }

            // Open the treasure chest if near a treasure chest
            if (activeChest != null)
            {
                Vector3 directionToChest = activeChest.transform.position - transform.position;
                if (Vector3.Dot(transform.forward, directionToChest.normalized) > 0.5f) // Adjust threshold if needed
                {
                    activeChest.OpenChest();
                    treasure++;
                    treasureText.text = "treasure: " + treasure;
                }
            }
        }
    }
}

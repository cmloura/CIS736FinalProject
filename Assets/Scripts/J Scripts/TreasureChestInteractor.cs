using UnityEngine;

public class TreasureChestInteractor : MonoBehaviour
{
    public string chestTag = "TreasureChest"; // Tag for treasure chest objects
    private TreasureChest activeChest; // Store the detected treasure chest

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering is a treasure chest
        if (other.CompareTag(chestTag))
        {
            activeChest = other.GetComponent<TreasureChest>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Clear the reference when the player leaves the chest area
        if (other.CompareTag(chestTag) && other.GetComponent<TreasureChest>() == activeChest)
        {
            activeChest = null;
        }
    }

    void Update()
    {
        // Trigger the chest's animation when the interact button is pressed
        if (Input.GetKeyDown(KeyCode.E) && activeChest != null)
        {
            activeChest.OpenChest();
        }
    }
}

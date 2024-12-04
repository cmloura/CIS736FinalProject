using UnityEngine;

public class TreasureChest : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Get the Animator component attached to the chest
        animator = GetComponent<Animator>();

        // Check if Animator exists
        if (animator == null)
        {
            Debug.LogError($"No Animator found on {gameObject.name}");
        }
    }

    // Method to trigger the chest opening animation
    public void OpenChest()
    {
        if (animator != null)
        {
            animator.SetTrigger("Open");
            Debug.Log($"{gameObject.name} is opening!");
        }
    }
}

using UnityEngine;

public class FireworkBox : MonoBehaviour
{
    private ParticleSystem ps;

    void Start()
    {
        // Look for the ParticleSystem in the child objects of FireworkBox
        ps = GetComponentInChildren<ParticleSystem>();

        // If no ParticleSystem is found, log an error
        if (ps == null)
        {
            Debug.LogError($"No ParticleSystem found on {gameObject.name} or its children!");
        }
        else
        {
            Debug.Log($"ParticleSystem found on {gameObject.name}");
        }
    }

    // Method to activate the particle generator
    public void ActivateParticles()
    {
        if (ps != null)
        {
            if (!ps.isPlaying)
            {
                ps.Play(); // Play the particle system
                Debug.Log($"{gameObject.name} particles activated!");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} particles are already playing.");
            }
        }
        else
        {
            Debug.LogError("No ParticleSystem attached to the object.");
        }
    }
}

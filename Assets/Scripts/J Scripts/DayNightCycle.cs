using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Light Settings")]
    public Light sunLight; // Directional light for the sun
    public Light moonLight; // Directional light for the moon
    public float dayLength = 60f; // Length of one full day (in seconds)

    private float timeOfDay = 0f; // 0 to 1, where 0 is midnight, 0.5 is noon, 1 is the next midnight

    void Start()
    {
        Time.timeScale = 1.0f; // Ensure normal game speed
    }

    void Update()
    {
        // Increment time of day
        timeOfDay += Time.deltaTime / dayLength;
        if (timeOfDay >= 1f)
        {
            timeOfDay -= 1f; // Loop to the next day
        }

        // Update light rotations
        UpdateLightRotations();

        // Optional: Log for debugging
        Debug.Log("Time of Day: " + timeOfDay);
    }

    void UpdateLightRotations()
    {
        // Calculate sun and moon angles
        float sunAngle = timeOfDay * 360f; // Full rotation in one day
        sunLight.transform.rotation = Quaternion.Euler(new Vector3(sunAngle - 90f, 0f, 0f)); // Sun rotation
        moonLight.transform.rotation = Quaternion.Euler(new Vector3(sunAngle - 270f, 0f, 0f)); // Moon rotation
    }
}

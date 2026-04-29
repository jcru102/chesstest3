using UnityEngine;

public class Lighting : MonoBehaviour
{
    [Header("Light Objects")]
    public GameObject directionalLight;
    public GameObject otherLight;

    [Header("UI Button (Optional)")]
    public UnityEngine.UI.Button toggleButton;

    private bool isDirectionalActive = true;

    void Start()
    {
        // Initialize lights - directional light starts active
        if (directionalLight != null)
            directionalLight.SetActive(true);
        if (otherLight != null)
            otherLight.SetActive(false);

        // Set up button click event if button is assigned
        if (toggleButton != null)
            toggleButton.onClick.AddListener(ToggleLights);
    }

    public void ToggleLights()
    {
        // Switch the active state
        isDirectionalActive = !isDirectionalActive;

        // Toggle the lights
        if (directionalLight != null)
            directionalLight.SetActive(isDirectionalActive);
        if (otherLight != null)
            otherLight.SetActive(!isDirectionalActive);

        // Optional: Debug message to confirm the switch
        Debug.Log($"Switched to: {(isDirectionalActive ? "Directional Light" : "Other Light")}");
    }

    // Alternative method for keyboard input
    void Update()
    {
        // Press 'L' key to toggle lights
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleLights();
        }
    }
}

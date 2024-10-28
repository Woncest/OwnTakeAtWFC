using UnityEngine;
using UnityEngine.UI;

public class ToggleHandler : MonoBehaviour
{
    private Toggle myToggle;  // Reference to your Toggle UI element
    public TileGridGenerator tileGridGenerator;

    void Start()
    {
        myToggle = GetComponent<Toggle>();

        // Set the initial state or default behavior when the scene loads
        myToggle.isOn = false;
        tileGridGenerator.showGenerationProcess = false;

        // Add listener to call a method when the toggle is changed
        myToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    // Method that is called when the toggle's value changes
    void OnToggleChanged(bool isOn)
    {
        if (isOn)
        {
            tileGridGenerator.showGenerationProcess = true;
        }
        else
        {
            tileGridGenerator.showGenerationProcess = false;
        }
    }

    void OnDestroy()
    {
        // Clean up listener to prevent memory leaks when the object is destroyed
        myToggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}

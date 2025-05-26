using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileSetLoader : MonoBehaviour
{
    public List<Toggle> toggles = new List<Toggle>();

    public TileGridGenerator tileGridGenerator;

    private Toggle currentlyActiveToggle;

    // Start is called before the first frame update
    void Start()
    {
        // Check which toggle is active by default and set it as the currently active toggle
        foreach (var toggle in toggles)
        {
            if (toggle.isOn)
            {
                currentlyActiveToggle = toggle;
                DoSomethingWithToggle(toggle); // Optional: call action if needed for initial setup
                break;
            }
        }

        // Subscribe to each toggle's onValueChanged event
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener(delegate { OnToggleChanged(toggle); });
        }
    }

    void OnToggleChanged(Toggle changedToggle)
    {
        // If the toggle is being turned on
        if (changedToggle.isOn)
        {
            // Set the currently active toggle
            currentlyActiveToggle = changedToggle;

            // Call the action you want to perform on the newly activated toggle
            DoSomethingWithToggle(changedToggle);

            // Deactivate all other toggles except the one that was activated
            foreach (var toggle in toggles)
            {
                if (toggle != changedToggle)
                {
                    toggle.isOn = false;
                }
            }
        }
        else
        {
            // Prevent the currently active toggle from being turned off
            if (changedToggle == currentlyActiveToggle)
            {
                changedToggle.isOn = true;  // Immediately re-enable it
            }
        }
    }

    void DoSomethingWithToggle(Toggle activeToggle)
    {
        tileGridGenerator.tilePrefabs = activeToggle.GetComponent<TileSetHolder>().tilePrefabs;
    }
}

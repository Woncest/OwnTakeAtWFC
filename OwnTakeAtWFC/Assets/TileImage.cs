using UnityEngine;
using UnityEngine.UI; // Required for working with UI components

public class TileImage : MonoBehaviour
{
    public GameObject tilePrefab; // The tile associated with this image

    private Button button; // Reference to the button component

    private void Awake()
    {
        // Get the Button component attached to the same GameObject
        button = GetComponent<Button>();

        if (button != null)
        {
            // Add a listener for click events
            button.onClick.AddListener(OnTileClicked);
        }
        else
        {
            Debug.LogError("No Button component found on the UI image.");
        }
    }

    /// <summary>
    /// Called when the image is clicked.
    /// </summary>
    private void OnTileClicked()
    {
        if (tilePrefab != null)
        {
            // Notify the TileManager of the selected tile
            TileManager.Instance.SetCurrentTile(tilePrefab);
        }
        else
        {
            Debug.LogError("TilePrefab is not assigned to the TileImage.");
        }
    }
}
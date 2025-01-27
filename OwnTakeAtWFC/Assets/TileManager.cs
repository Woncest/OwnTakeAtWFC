using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager Instance; // Singleton for easy access
    public GameObject currentTile; // The currently selected tile

    private void Awake()
    {
        // Ensure there's only one instance of the TileManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets the current tile to the one selected in the UI.
    /// </summary>
    /// <param name="tile">The tile GameObject to set as the current tile.</param>
    public void SetCurrentTile(GameObject tile)
    {
        currentTile = tile;
        Debug.Log($"Current tile set to: {tile.name}");
    }
}

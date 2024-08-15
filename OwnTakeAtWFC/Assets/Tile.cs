using UnityEngine;

public class Tile : MonoBehaviour
{
    // Arrays to hold references to allowed neighbors in each direction
    public GameObject[] allowedAbove;
    public GameObject[] allowedRight;
    public GameObject[] allowedBelow;
    public GameObject[] allowedLeft;

    // Optional: You can add additional properties or methods to customize behavior

    // Example method to check if a given tile can be placed in a specific direction
    public bool CanPlaceAbove(GameObject tile)
    {
        return System.Array.Exists(allowedAbove, allowedTile => allowedTile == tile);
    }

    public bool CanPlaceRight(GameObject tile)
    {
        return System.Array.Exists(allowedRight, allowedTile => allowedTile == tile);
    }

    public bool CanPlaceBelow(GameObject tile)
    {
        return System.Array.Exists(allowedBelow, allowedTile => allowedTile == tile);
    }

    public bool CanPlaceLeft(GameObject tile)
    {
        return System.Array.Exists(allowedLeft, allowedTile => allowedTile == tile);
    }
}

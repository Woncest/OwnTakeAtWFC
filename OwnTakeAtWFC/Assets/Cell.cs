using UnityEngine;

public class Cell
{
    public GameObject[] possibleTiles;

    public Cell(GameObject[] tilePrefabs)
    {
        // Initialize with all possible tiles (for future use)
        possibleTiles = tilePrefabs;
    }
}

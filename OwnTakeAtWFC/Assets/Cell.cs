using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public List<GameObject> possibleTiles;  // Replaced array with List<GameObject>

    public Cell(List<GameObject> tilePrefabs)
    {
        // Initialize with all possible tiles
        possibleTiles = new List<GameObject>(tilePrefabs);
    }
}

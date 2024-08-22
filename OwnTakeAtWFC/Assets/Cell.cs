using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public List<GameObject> possibleTiles;  // Possible tiles that can be placed in this cell
    public GameObject instantiatedTile;     // The tile that has been instantiated in this cell

    public Cell(List<GameObject> tiles)
    {
        possibleTiles = new List<GameObject>(tiles);
        instantiatedTile = null;  // Initially, no tile is instantiated
    }

    // Sets the tile and keeps track of the instantiated tile
    public void SetTile(GameObject tile)
    {
        instantiatedTile = tile;
    }

    // Checks if the tile is already instantiated
    public bool IsTileSet()
    {
        return instantiatedTile != null;
    }
}

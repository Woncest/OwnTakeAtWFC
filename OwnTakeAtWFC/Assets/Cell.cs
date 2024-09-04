using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public List<GameObject> possibleTiles;    // Possible tiles that can be placed in this cell
    public List<GameObject> notChosenTiles;   // Tiles that were not chosen
    public GameObject instantiatedTile;       // The tile that has been instantiated in this cell
    public bool tileSet = false;

    public Cell(List<GameObject> tiles)
    {
        possibleTiles = new List<GameObject>(tiles);
        notChosenTiles = new List<GameObject>(tiles);  // Initialize with all tiles as not chosen
        instantiatedTile = null;  // Initially, no tile is instantiated
    }

    // Sets the tile and keeps track of the instantiated tile
    public void SetTile(GameObject tile)
    {
        instantiatedTile = tile;
        tileSet = true;
        // Remove the chosen tile from the list of not chosen tiles
        notChosenTiles.Remove(tile);
    }

    // Checks if the tile is already instantiated
    public bool IsTileSet()
    {
        return instantiatedTile != null;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public List<GameObject> possibleTiles;
    public GameObject instantiatedTile;

    public Cell(List<GameObject> tiles)
    {
        possibleTiles = new List<GameObject>(tiles);
        instantiatedTile = null;
    }

    public void SetTile(GameObject tile)
    {
        instantiatedTile = tile;
    }
}

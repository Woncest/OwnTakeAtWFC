using UnityEngine;
using System.Collections.Generic;

public class Cell
{
    public List<GameObject> possibleTiles;

    public Cell(GameObject[] allTiles)
    {
        // Initially, the cell can hold any tile
        possibleTiles = new List<GameObject>(allTiles);
    }

    public bool IsCollapsed()
    {
        return possibleTiles.Count == 1;
    }

    public void Collapse()
    {
        if (possibleTiles.Count > 1)
        {
            // Choose a random tile from the possibilities
            GameObject chosenTile = possibleTiles[Random.Range(0, possibleTiles.Count)];
            possibleTiles = new List<GameObject> { chosenTile };
        }
    }

    public GameObject GetCollapsedTile()
    {
        return IsCollapsed() ? possibleTiles[0] : null;
    }

    // Eliminate tiles that are not allowed based on neighboring constraints
    public void ConstrainPossibleTiles(System.Predicate<GameObject> constraint)
    {
        possibleTiles.RemoveAll(tile => !constraint(tile));
    }
}

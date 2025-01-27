using UnityEngine;

public class CellClickHandler : MonoBehaviour
{
    public int gridX; // The x-coordinate of this cell
    public int gridY; // The y-coordinate of this cell
    public TileGridGenerator grid; // Reference to the main grid

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0)) // Left-click
        {
            grid.HandleCellClick(gridX, gridY, isRightClick: false);
        }
        else if (Input.GetMouseButtonDown(1)) // Right-click
        {
            grid.HandleCellClick(gridX, gridY, isRightClick: true);
        }
    }
}

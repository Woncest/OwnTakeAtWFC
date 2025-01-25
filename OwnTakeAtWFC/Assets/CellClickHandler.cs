using UnityEngine;

public class CellClickHandler : MonoBehaviour
{
    public Vector2Int coordinates; // The grid coordinates of this cell

    void OnMouseDown()
    {
        // Display the cell's coordinates when clicked
        Debug.Log($"Cell clicked at coordinates: {coordinates}");
    }
}
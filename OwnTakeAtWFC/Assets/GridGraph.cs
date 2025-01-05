using System.Collections.Generic;
using UnityEngine;

public class GridGraph : MonoBehaviour
{
    public int gridSizeX = 25; // Number of nodes in X direction
    public int gridSizeY = 25; // Number of nodes in Z direction
    public float nodeSpacing = 1.5f; // Distance between nodes
    public float nodeRadius = 0.2f; // Radius of the node circles
    public Color nodeColor = Color.blue; // Color of the node circles
    public Color edgeColor = Color.green; // Color of the set edges

    private Vector3[,] gridPositions; // Stores the positions of all nodes
    private List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)>(); // List of set edges

    void Start()
    {
        // Initialize the grid positions
        gridPositions = new Vector3[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                gridPositions[x, z] = new Vector3(x * nodeSpacing, 0, z * nodeSpacing);
            }
        }

        // Randomly set one edge per node with a 50/50 chance
        RandomlySetEdges();
    }

    void RandomlySetEdges()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                // 50/50 chance to set an edge
                if (Random.value > 0.5f)
                {
                    // Get potential neighbors
                    List<Vector3> neighbors = GetNeighbors(x, z);

                    // Randomly pick one neighbor
                    if (neighbors.Count > 0)
                    {
                        Vector3 selectedNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                        Vector3 currentNode = gridPositions[x, z];

                        // Ensure the edge isn't already set
                        if (!edges.Contains((currentNode, selectedNeighbor)) &&
                            !edges.Contains((selectedNeighbor, currentNode)))
                        {
                            edges.Add((currentNode, selectedNeighbor));
                        }
                    }
                }
            }
        }
    }

    List<Vector3> GetNeighbors(int x, int z)
    {
        List<Vector3> neighbors = new List<Vector3>();

        if (x + 1 < gridSizeX) // Right neighbor
            neighbors.Add(gridPositions[x + 1, z]);
        if (x - 1 >= 0) // Left neighbor
            neighbors.Add(gridPositions[x - 1, z]);
        if (z + 1 < gridSizeY) // Forward neighbor
            neighbors.Add(gridPositions[x, z + 1]);
        if (z - 1 >= 0) // Backward neighbor
            neighbors.Add(gridPositions[x, z - 1]);

        return neighbors;
    }

    void OnDrawGizmos()
    {
        if (gridPositions == null)
            return;

        // Draw nodes
        Gizmos.color = nodeColor;
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                Gizmos.DrawSphere(gridPositions[x, z], nodeRadius);
            }
        }

        // Draw edges
        Gizmos.color = edgeColor;
        foreach (var edge in edges)
        {
            Gizmos.DrawLine(edge.Item1, edge.Item2);
        }
    }
}

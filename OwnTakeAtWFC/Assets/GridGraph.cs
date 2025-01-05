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
    public int numEmptyBlocks = 5; // Number of 2x2 blocks to free up

    private Vector3[,] gridPositions; // Stores the positions of all nodes
    private List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)>(); // List of set edges
    private bool[,] isNodeEmpty; // To track which nodes are empty

    void Start()
    {
        // Initialize the grid positions
        gridPositions = new Vector3[gridSizeX, gridSizeY];
        isNodeEmpty = new bool[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                // Calculate position based on grid size and spacing
                Vector3 position = new Vector3(x * nodeSpacing, 0, z * nodeSpacing);
                gridPositions[x, z] = position;
            }
        }

        // Randomly remove 2x2 blocks of nodes and mark them as empty
        RemoveRandom2x2Blocks(numEmptyBlocks);

        // Create edges for each node based on cardinal directions
        CreateEdgesForAllNodes();
    }

    void CreateEdgesForAllNodes()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                Vector3 currentNode = gridPositions[x, z];

                // Skip creating edges for empty nodes
                if (isNodeEmpty[x, z]) continue;

                // Get all neighbors for this node (up, down, left, right)
                List<Vector3> neighbors = GetNeighbors(x, z);

                foreach (var neighbor in neighbors)
                {
                    // Skip edges that would connect to empty nodes
                    int neighborX = (int)(neighbor.x / nodeSpacing);
                    int neighborZ = (int)(neighbor.z / nodeSpacing);
                    if (isNodeEmpty[neighborX, neighborZ]) continue;

                    // Create an edge between the current node and the neighbor
                    edges.Add((currentNode, neighbor));
                }
            }
        }
    }

    // Removes random 2x2 blocks of nodes and marks them as empty
    void RemoveRandom2x2Blocks(int numberOfBlocks)
    {
        for (int i = 0; i < numberOfBlocks; i++)
        {
            int startX = Random.Range(0, gridSizeX - 1);
            int startZ = Random.Range(0, gridSizeY - 1);

            // Mark the 2x2 block as empty
            for (int x = startX; x < startX + 2; x++)
            {
                for (int z = startZ; z < startZ + 2; z++)
                {
                    isNodeEmpty[x, z] = true;
                }
            }
        }
    }

    // Gets the neighbors of a node in the cardinal directions
    List<Vector3> GetNeighbors(int x, int z)
    {
        List<Vector3> neighbors = new List<Vector3>();

        // Check for valid neighbors in cardinal directions
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
                // Skip drawing empty nodes
                if (isNodeEmpty[x, z]) continue;

                Gizmos.DrawSphere(gridPositions[x, z], nodeRadius);
            }
        }

        // Draw edges
        Gizmos.color = edgeColor;
        foreach (var edge in edges)
        {
            // Only draw edges that connect non-empty nodes
            int x1 = (int)(edge.Item1.x / nodeSpacing);
            int z1 = (int)(edge.Item1.z / nodeSpacing);
            int x2 = (int)(edge.Item2.x / nodeSpacing);
            int z2 = (int)(edge.Item2.z / nodeSpacing);

            if (!isNodeEmpty[x1, z1] && !isNodeEmpty[x2, z2])
            {
                Gizmos.DrawLine(edge.Item1, edge.Item2);
            }
        }
    }
}

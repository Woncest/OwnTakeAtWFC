using System.Collections.Generic;
using UnityEngine;

public class GridGraph : MonoBehaviour
{
    public int gridSizeX = 25; // Number of nodes in X direction
    public int gridSizeY = 25; // Number of nodes in Z direction
    public float nodeSpacing = 1.5f; // Distance between nodes
    public float nodeRadius = 0.2f; // Radius of the node circles
    public Color nodeColor = Color.blue; // Color of the node circles
    public Color streetColor = Color.gray; // Color of the street nodes
    public Color edgeColor = Color.green; // Color of the set edges
    public int minBlockSize = 2; // Minimum size of the block (e.g., 2x2)
    public int maxBlockSize = 4; // Maximum size of the block (e.g., 4x4)

    private Vector3[,] gridPositions; // Stores the positions of all nodes
    private NodeState[,] nodeStates; // States of each node in the grid
    private List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)>(); // List of set edges

    // Enum for the states of nodes
    private enum NodeState
    {
        Filled, // Node is part of the grid
        Empty,  // Node is part of an empty space
        Street  // Node is part of a street (adjacent to empty space)
    }

    void Start()
    {
        // Initialize the grid positions and node states
        gridPositions = new Vector3[gridSizeX, gridSizeY];
        nodeStates = new NodeState[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                // Calculate position based on grid size and spacing
                Vector3 position = new Vector3(x * nodeSpacing, 0, z * nodeSpacing);
                gridPositions[x, z] = position;
                nodeStates[x, z] = NodeState.Filled; // Initialize all nodes as filled
            }
        }

        // Create empty spaces and mark adjacent nodes as streets
        CreateEmptySpaces();

        // Create edges for each node based on cardinal directions
        CreateEdgesForAllNodes();
    }

    void CreateEmptySpaces()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                // Skip nodes that are already empty or streets
                if (nodeStates[x, z] != NodeState.Filled) continue;

                // Attempt to place a block
                PlaceRandomSizedBlock(x, z);
            }
        }
    }

    void PlaceRandomSizedBlock(int startX, int startZ)
    {
        int blockWidth = Random.Range(minBlockSize, maxBlockSize + 1);
        int blockHeight = Random.Range(minBlockSize, maxBlockSize + 1);

        while (blockWidth >= minBlockSize && blockHeight >= minBlockSize)
        {
            if (CanCreateBlock(startX, startZ, blockWidth, blockHeight))
            {
                CreateEmptyBlock(startX, startZ, blockWidth, blockHeight);
                return;
            }

            // Shrink the block size
            if (blockWidth > blockHeight)
            {
                blockWidth--;
            }
            else
            {
                blockHeight--;
            }
        }
    }

    bool CanCreateBlock(int startX, int startZ, int blockWidth, int blockHeight)
    {
        // Check if the block fits within the grid and doesn't overwrite existing streets or empty spaces
        for (int x = startX; x < startX + blockWidth; x++)
        {
            for (int z = startZ; z < startZ + blockHeight; z++)
            {
                // Check if the node is within bounds
                if (x >= gridSizeX || z >= gridSizeY) return false;

                // Check if the node is already empty or a street
                if (nodeStates[x, z] != NodeState.Filled) return false;
            }
        }
        return true;
    }

    void CreateEmptyBlock(int startX, int startZ, int blockWidth, int blockHeight)
    {
        // Adjust block size to ensure it doesn't exceed grid bounds
        int adjustedWidth = Mathf.Min(blockWidth, gridSizeX - startX);
        int adjustedHeight = Mathf.Min(blockHeight, gridSizeY - startZ);

        // Mark the block as empty
        for (int x = startX; x < startX + adjustedWidth && x < gridSizeX; x++)
        {
            for (int z = startZ; z < startZ + adjustedHeight && z < gridSizeY; z++)
            {
                nodeStates[x, z] = NodeState.Empty;
            }
        }

        // Mark adjacent nodes (including diagonals) as streets
        for (int x = startX - 1; x <= startX + adjustedWidth; x++)
        {
            for (int z = startZ - 1; z <= startZ + adjustedHeight; z++)
            {
                if (x >= 0 && x < gridSizeX && z >= 0 && z < gridSizeY)
                {
                    if (nodeStates[x, z] == NodeState.Filled)
                    {
                        nodeStates[x, z] = NodeState.Street;
                    }
                }
            }
        }
    }

    void CreateEdgesForAllNodes()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                Vector3 currentNode = gridPositions[x, z];

                // Skip creating edges for empty nodes
                if (nodeStates[x, z] == NodeState.Empty) continue;

                // Get all neighbors for this node (up, down, left, right)
                List<Vector3> neighbors = GetNeighbors(x, z);

                foreach (var neighbor in neighbors)
                {
                    // Skip edges that would connect to empty nodes
                    int neighborX = (int)(neighbor.x / nodeSpacing);
                    int neighborZ = (int)(neighbor.z / nodeSpacing);
                    if (nodeStates[neighborX, neighborZ] == NodeState.Empty) continue;

                    // Create an edge between the current node and the neighbor
                    edges.Add((currentNode, neighbor));
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
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                Vector3 position = gridPositions[x, z];

                // Set Gizmos color based on node state
                if (nodeStates[x, z] == NodeState.Empty)
                    Gizmos.color = Color.red;
                else if (nodeStates[x, z] == NodeState.Street)
                    Gizmos.color = streetColor;
                else
                    Gizmos.color = nodeColor;

                // Draw the node
                Gizmos.DrawSphere(position, nodeRadius);
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

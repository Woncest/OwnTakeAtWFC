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

    [Header("Edge Probability (Sum must be 100%)")]
    [Range(0, 100)] public int chanceOf0Edges = 25; // Probability of 0 edges
    [Range(0, 100)] public int chanceOf2Edges = 25; // Probability of 2 edges
    [Range(0, 100)] public int chanceOf3Edges = 25; // Probability of 3 edges
    [Range(0, 100)] public int chanceOf4Edges = 25; // Probability of 4 edges

    private Vector3[,] gridPositions; // Stores the positions of all nodes
    private List<(Vector3, Vector3)> edges = new List<(Vector3, Vector3)>(); // List of set edges
    private Dictionary<Vector3, int> nodeEdgeCount = new Dictionary<Vector3, int>(); // Tracks edges per node

    void Start()
    {
        // Initialize the grid positions
        gridPositions = new Vector3[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                Vector3 position = new Vector3(x * nodeSpacing, 0, z * nodeSpacing);
                gridPositions[x, z] = position;
                nodeEdgeCount[position] = 0; // Initialize edge count
            }
        }

        // Randomly set edges for each node
        RandomlySetEdges();
    }

    void RandomlySetEdges()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeY; z++)
            {
                Vector3 currentNode = gridPositions[x, z];

                // Roll for the total number of edges based on inspector probabilities
                int rolledEdges = RollEdgeCount();

                // Calculate how many edges still need to be placed for this node
                int edgesToPlace = rolledEdges - nodeEdgeCount[currentNode];
                if (edgesToPlace <= 0) continue;

                // Get all potential neighbors
                List<Vector3> neighbors = GetNeighbors(x, z);

                while (edgesToPlace > 0 && neighbors.Count > 0)
                {
                    // Randomly pick a neighbor
                    Vector3 selectedNeighbor = neighbors[Random.Range(0, neighbors.Count)];
                    
                    // Ensure the edge isn't already set
                    if (!edges.Contains((currentNode, selectedNeighbor)) &&
                        !edges.Contains((selectedNeighbor, currentNode)))
                    {
                        edges.Add((currentNode, selectedNeighbor));
                        nodeEdgeCount[currentNode]++;
                        nodeEdgeCount[selectedNeighbor]++;
                        edgesToPlace--;
                    }

                    // Remove the selected neighbor from the list to prevent re-selection
                    neighbors.Remove(selectedNeighbor);
                }
            }
        }
    }

    int RollEdgeCount()
    {
        // Get a random value between 0 and 100
        int roll = Random.Range(0, 100);

        // Determine the edge count based on the roll and probabilities
        if (roll < chanceOf0Edges) return 0;
        else if (roll < chanceOf0Edges + chanceOf2Edges) return 2;
        else if (roll < chanceOf0Edges + chanceOf2Edges + chanceOf3Edges) return 3;
        else return 4;
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

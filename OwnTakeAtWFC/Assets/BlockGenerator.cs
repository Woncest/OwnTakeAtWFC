using System.Collections.Generic;
using UnityEngine;

public class BlockGenerator : MonoBehaviour
{
    public int defaultWidth = 2; // Default block width
    public int blockHeight = 2; // Default block height

    private HashSet<Vector3> nodes = new HashSet<Vector3>();
    private HashSet<(Vector3, Vector3)> edges = new HashSet<(Vector3, Vector3)>();

    void Start()
    {
        GenerateFilledGrid(25, 25);
        OutputEdgeDimensions();
    }

    void GenerateFilledGrid(int minWidth, int minHeight)
    {
        int gridWidth = Mathf.CeilToInt((float)minWidth / defaultWidth) * defaultWidth;
        int gridHeight = Mathf.CeilToInt((float)minHeight / blockHeight) * blockHeight;

        for (int y = 0; y < gridHeight; y += blockHeight)
        {
            for (int x = 0; x < gridWidth; x += defaultWidth)
            {
                // Check for potential extensions
                bool canExtendRight = x + defaultWidth * 2 <= gridWidth && !BlockExists(new Vector3(x + defaultWidth, y, 0));
                bool canExtendTop = y + this.blockHeight * 2 <= gridHeight && !BlockExists(new Vector3(x, y + this.blockHeight, 0));

                // Randomly decide to extend the block if possible
                bool extendRight = canExtendRight && Random.value > 0.5f; // 50% chance to extend right
                bool extendTop = canExtendTop && Random.value > 0.5f;   // 50% chance to extend top

                int blockWidth = extendRight ? defaultWidth * 2 : defaultWidth;
                int blockHeight = extendTop ? this.blockHeight * 2 : this.blockHeight;

                GenerateBlock(new Vector3(x, y, 0), blockWidth, blockHeight);

                // Skip extra space for extended blocks
                if (extendRight) x += defaultWidth;
            }
        }
    }

    bool BlockExists(Vector3 position)
    {
        // Check if a block already occupies this position
        return nodes.Contains(position + new Vector3(0, blockHeight, 0)) ||
               nodes.Contains(position + new Vector3(defaultWidth, blockHeight, 0)) ||
               nodes.Contains(position) ||
               nodes.Contains(position + new Vector3(defaultWidth, 0, 0));
    }

    void GenerateBlock(Vector3 position, int width, int height)
    {
        // Generate corner nodes
        AddNode(position + new Vector3(0, height, 0)); // Top-left
        AddNode(position + new Vector3(width, height, 0)); // Top-right
        AddNode(position); // Bottom-left
        AddNode(position + new Vector3(width, 0, 0)); // Bottom-right

        // Generate side nodes (every 1 unit along the edges)
        for (int x = 1; x < width; x++)
        {
            AddNode(position + new Vector3(x, 0, 0)); // Bottom edge
            AddNode(position + new Vector3(x, height, 0)); // Top edge
        }

        for (int y = 1; y < height; y++)
        {
            AddNode(position + new Vector3(0, y, 0)); // Left edge
            AddNode(position + new Vector3(width, y, 0)); // Right edge
        }

        // Connect edges only between directly adjacent nodes
        CreateEdges(position, width, height);
    }

    void AddNode(Vector3 node)
    {
        nodes.Add(node);
    }

    void CreateEdges(Vector3 position, int width, int height)
    {
        // Bottom and top edges
        for (int x = 0; x < width; x++)
        {
            ConnectNodes(position + new Vector3(x, 0, 0), position + new Vector3(x + 1, 0, 0)); // Bottom
            ConnectNodes(position + new Vector3(x, height, 0), position + new Vector3(x + 1, height, 0)); // Top
        }

        // Left and right edges
        for (int y = 0; y < height; y++)
        {
            ConnectNodes(position + new Vector3(0, y, 0), position + new Vector3(0, y + 1, 0)); // Left
            ConnectNodes(position + new Vector3(width, y, 0), position + new Vector3(width, y + 1, 0)); // Right
        }
    }

    void ConnectNodes(Vector3 start, Vector3 end)
    {
        if (nodes.Contains(start) && nodes.Contains(end))
        {
            edges.Add((start, end));
        }
    }

    void OutputEdgeDimensions()
    {
        float totalHorizontal = 0;
        float totalVertical = 0;

        foreach (var edge in edges)
        {
            if (Mathf.Abs(edge.Item1.y - edge.Item2.y) < 0.01f && edge.Item1.y == 0) // Horizontal edge on y=0
            {
                totalHorizontal += Vector3.Distance(edge.Item1, edge.Item2);
            }
            else if (Mathf.Abs(edge.Item1.x - edge.Item2.x) < 0.01f && edge.Item1.x == 0) // Vertical edge on x=0
            {
                totalVertical += Vector3.Distance(edge.Item1, edge.Item2);
            }
        }

        Debug.Log($"Total Horizontal Length (y=0): {totalHorizontal}");
        Debug.Log($"Total Vertical Length (x=0): {totalVertical}");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var node in nodes)
        {
            Gizmos.DrawSphere(node, 0.1f); // Draw nodes as small spheres
        }

        Gizmos.color = Color.blue;
        foreach (var edge in edges)
        {
            Gizmos.DrawLine(edge.Item1, edge.Item2); // Draw edges as lines
        }
    }
}

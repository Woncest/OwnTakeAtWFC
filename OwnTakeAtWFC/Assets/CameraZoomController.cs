using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraZoomController : MonoBehaviour
{
    public TileGridGenerator tileGridGenerator;  // Reference to the grid size from your grid generator
    public float padding = 2f;  // Optional padding around the grid
    public float yOffsetMultiplier = 1.5f;  // Multiplier for camera height adjustment based on orthographic size

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        AdjustZoom();
    }

    void Update()
    {
        AdjustZoom();
    }

    public void AdjustZoom()
    {
        // Calculate the necessary orthographic size to fit the grid within the camera's view
        float requiredSize = (tileGridGenerator.gridSize / 2f) + padding;
        
        // Set the camera's orthographic size based on the larger of the default or required size
        cam.orthographicSize = requiredSize;

        // Center the camera horizontally on the grid
        float centerX = (tileGridGenerator.gridSize - 1) / 2f;
        float centerZ = (tileGridGenerator.gridSize - 1) / 2f;
        
        // Adjust the camera's y position based on orthographic size and offset multiplier
        float yPosition = cam.orthographicSize * yOffsetMultiplier;

        // Update the camera position
        transform.position = new Vector3(centerX, yPosition, centerZ);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    // Lists to hold references to allowed neighbors in each direction
    public List<GameObject> allowedAbove;  // Replaced array with List<GameObject>
    public List<GameObject> allowedRight;  // Replaced array with List<GameObject>
    public List<GameObject> allowedBelow;  // Replaced array with List<GameObject>
    public List<GameObject> allowedLeft;   // Replaced array with List<GameObject>
}

using UnityEngine;

public class Tile : MonoBehaviour
{
    // Arrays to hold references to allowed neighbors in each direction
    public GameObject[] allowedAbove;
    public GameObject[] allowedRight;
    public GameObject[] allowedBelow;
    public GameObject[] allowedLeft;
}

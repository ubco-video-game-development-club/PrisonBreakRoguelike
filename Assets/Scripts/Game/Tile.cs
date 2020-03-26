using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector]
    public bool isDoor;

    [HideInInspector]
    public bool isWall;

    [HideInInspector]
    public TileObject occupant;
}

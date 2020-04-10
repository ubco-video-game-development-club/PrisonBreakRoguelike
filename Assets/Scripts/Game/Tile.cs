using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector]
    public bool isDoor;

    [HideInInspector]
    public bool isWall;

    [HideInInspector]
    public TileObject occupant;

    ///<summary>Returns true if this tile is a wall or if it has a blocking occupant.</summary>
    public bool IsBlocked()
    {
        return isWall || (occupant && occupant.gameObject.layer == LayerMask.NameToLayer("Environment"));
    }
}

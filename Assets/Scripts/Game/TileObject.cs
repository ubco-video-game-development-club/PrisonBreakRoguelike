using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class TileObject : MonoBehaviour
{
    public void Destroy()
    {
        int x = (int)transform.position.x;
        int y = (int)transform.position.y;
        LevelController.instance.tiles[x, y].occupant = null;
        Destroy(gameObject);
    }
}

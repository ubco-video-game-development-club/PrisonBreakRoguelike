using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int size = 16;
    public float tileScale = 1f;
    public Tile tilePrefab;

    [HideInInspector]
    public int x, y;
    [HideInInspector]
    public bool visited = false;

    private Tile[,] tiles;


    public Tile GetTile(int x, int y)
    {
        if (x < 0 || x >= tiles.GetLength(0) ||
            y < 0 || y >= tiles.GetLength(1))
        {
            return null;
        }

        return tiles[x, y];
    }

    public void InitializeTiles(Color tileColor)
    {
        tiles = new Tile[size, size];
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Vector2 pos = new Vector2((i + 0.5f) * tileScale, (j + 0.5f) * tileScale);
                Vector2 offset = new Vector2(x * size * tileScale, y * size * tileScale);
                Tile tile = Instantiate(tilePrefab, pos + offset, Quaternion.identity, transform) as Tile;
                tile.name = "Tile[" + i + ", " + j + "]";
                tile.GetComponent<SpriteRenderer>().color = tileColor;
                tile.x = i;
                tile.y = j;
                tiles[i, j] = tile;
            }
        }
    }
    //Retrieves a list of all tiles that are part of a wall, but are not a corner tile
    public List<Tile> GetWalls() 
    {
        List<Tile> walls = new List<Tile>(); 
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Tile tile = tiles[i, j];
                if (((tile.x == 0 || tile.y == size) && (tile.y != 0 || tile.y != size)) || ((tile.y == 0 || tile.y == size) && (tile.x != 0 || tile.x != size)))
                {
                    walls.Add(tile);
                    tile.isWall = true;
                }
            }
        }
        return walls;
    }
}

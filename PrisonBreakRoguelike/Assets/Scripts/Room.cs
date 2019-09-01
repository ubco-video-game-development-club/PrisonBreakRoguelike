using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public int width = 16, height = 16;
    public float tileSize = 1f;
    public Tile tilePrefab;

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

    void Start()
    {
        tiles = new Tile[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 pos = new Vector2(i + tileSize/2, j + tileSize/2);
                Tile tile = Instantiate(tilePrefab, pos, Quaternion.identity) as Tile;
                tile.x = i;
                tile.y = j;
                tiles[i, j] = tile;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
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

    public void InitializeTiles(Tile tilePrefab, int roomSize, float tileScale, Color tileColor)
    {
        tiles = new Tile[roomSize, roomSize];
        for (int i = 0; i < roomSize; i++)
        {
            for (int j = 0; j < roomSize; j++)
            {
                Vector3 pos = new Vector2((i + 0.5f) * tileScale, (j + 0.5f) * tileScale);
                pos += transform.position;
                Tile tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform) as Tile;
                tile.name = "Tile[" + i + ", " + j + "]";
                tile.GetComponent<SpriteRenderer>().color = tileColor;
                tile.x = i;
                tile.y = j;
                tiles[i, j] = tile;
                tile.pos = pos;
            }
        }
    }

    public void InitializeObjects(GameObject[] decoPrefabs, GameObject[] itemPrefabs, GameObject[] enemyPrefabs, float decoChance, float itemChance, float enemyChance) 
    {
        GameObject[] objects = null;
        foreach(Tile tile in tiles)
        {           
            if(tile.occupant == null)
            {
                float rand = Random.Range(0f, 1f);

                if (rand < enemyChance)
                {
                    objects = enemyPrefabs;
                }
                else if (rand < enemyChance + itemChance)
                {
                    objects = itemPrefabs;
                }
                else if (rand < decoChance + enemyChance + itemChance)
                {
                    objects = decoPrefabs;
                }
                else
                {
                    continue;
                }
                
                if (objects != null)
                {
                    int randIndex = Random.Range(0, objects.Length); 
                    tile.occupant = Instantiate(objects[randIndex], tile.pos, Quaternion.identity);
                }
            }
        }   
    }
}


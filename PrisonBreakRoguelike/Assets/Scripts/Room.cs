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
            }
        }
    }

    public void InitializeRoom(List<GameObject[]> Objects) //Init room with each tile randomly selected from a few given arrays of objects, ideally with weights corrisponding to the liklihood of each array being chosen
    {
        foreach (Tile tile in tiles)
        {
            int rand = Random.Range(0, Objects.Count);
            GameObject[] choices = Objects[rand];
            int randFromArray = Random.Range(0, choices.Length);
            tile.occupant = choices[randFromArray];
        }
    }
 
}

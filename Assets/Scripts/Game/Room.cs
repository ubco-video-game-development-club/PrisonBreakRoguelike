using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    // [HideInInspector]
    // public int x, y; 
    // [HideInInspector]
    // public bool visited = false;
    // [HideInInspector]
    // public bool initialized = false;

    // private Tile[,] tiles;

    // public Vector2Int ToGridPosition(Vector2 worldPosition)
    // {
    //     // Offset to local space
    //     Vector2 temp = worldPosition - (Vector2)transform.position;

    //     // Scale for tile size
    //     temp /= LevelController.instance.tileScale;

    //     // Round to int
    //     return new Vector2Int(Mathf.FloorToInt(temp.x), Mathf.FloorToInt(temp.y));
    // }

    // public Tile TileAt(int x, int y)
    // {
    //     if (x < 0 || x >= tiles.GetLength(0) ||
    //         y < 0 || y >= tiles.GetLength(1))
    //     {
    //         return null;
    //     }

    //     return tiles[x, y];
    // }

    // public Dictionary<Vector2, Tile> GetTileLookup()
    // {
    //     Dictionary<Vector2, Tile> result = new Dictionary<Vector2, Tile>();
    //     if (tiles == null)
    //     {
    //         Debug.Log("bruh wut");
    //     }
    //     for (int i = 0; i < tiles.GetLength(0); i++)
    //     {
    //         for (int j = 0; j < tiles.GetLength(1); j++)
    //         {
    //             result.Add(tiles[i, j].transform.position, tiles[i, j]);
    //         }
    //     }
    //     return result;
    // }

    // public List<Item> GetItems()
    // {
    //     List<Item> items = new List<Item>();
    //     if (tiles != null)
    //     {
    //         for (int i = 0; i < tiles.GetLength(0); i++)
    //         {
    //             for (int j = 0; j < tiles.GetLength(1); j++)
    //             {
    //                 GameObject occupant = tiles[i, j].occupant;
    //                 if (occupant != null)
    //                 {
    //                     Item item;
    //                     if (occupant.TryGetComponent<Item>(out item))
    //                     {
    //                         items.Add(item);
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     return items;
    // }

    // public void InitializeTiles(Tile tilePrefab, int roomSize, float tileScale, Color tileColor, Sprite[] sprites,float[] weights )
    // {
    //     tiles = new Tile[roomSize, roomSize];
    //     for (int i = 0; i < roomSize; i++)
    //     {
    //         for (int j = 0; j < roomSize; j++)
    //         {
    //             Vector3 pos = new Vector2((i + 0.5f) * tileScale, (j + 0.5f) * tileScale);
    //             pos += transform.position;
    //             Tile tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform) as Tile;
    //             tile.name = "Tile[" + i + ", " + j + "]";
    //             tile.GetComponent<SpriteRenderer>().sprite = tile.ChooseSprite(sprites , weights);
    //             tile.GetComponent<SpriteRenderer>().color = tileColor;
    //             tile.x = i;
    //             tile.y = j;
    //             tiles[i, j] = tile;
    //         }
    //     }
    //     // Fit the boxcollider of the room to the roomSize for raycasting
    //     BoxCollider2D bc2d = GetComponent<BoxCollider2D>();
    //     float roomScale = roomSize * tileScale;
    //     bc2d.offset = new Vector2(roomScale / 2, roomScale / 2);
    //     bc2d.size = new Vector2(roomScale, roomScale);
    //     initialized = true;
    // }

    // public void InitializeObjects(GameObject[] decoPrefabs, GameObject[] itemPrefabs, GameObject[] enemyPrefabs, float decoChance, float itemChance, float enemyChance) 
    // {
    //     GameObject[] objects = null;
    //     foreach(Tile tile in tiles)
    //     {           
    //         if(!tile.IsOccupied())
    //         {
    //             float rand = Random.Range(0f, 1f);

    //             if (rand < enemyChance)
    //             {
    //                 objects = enemyPrefabs;
    //             }
    //             else if (rand < enemyChance + itemChance)
    //             {
    //                 objects = itemPrefabs;
    //             }
    //             else if (rand < decoChance + enemyChance + itemChance)
    //             {
    //                 objects = decoPrefabs;
    //             }
    //             else
    //             {
    //                 continue;
    //             }
                
    //             if (objects != null)
    //             {
    //                 int randIndex = Random.Range(0, objects.Length);
    //                 GameObject objPrefab = objects[randIndex];
                    
    //                 GameObject objInstance = Instantiate(objPrefab, tile.transform.position, Quaternion.identity);
    //                 tile.occupant = objInstance;

    //                 Item item;
    //                 if (objInstance.TryGetComponent<Item>(out item))
    //                 {
    //                     item.SetOccupiedTile(tile);
    //                 }
    //             }
    //         }
    //     }   
    // }
    // public void InitializeExit(GameObject stairs) 
    // {
    //     Tile tile;
    //     do  
    //         tile = TileAt(Random.Range(0, tiles.GetLength(0)), Random.Range(0, tiles.GetLength(1))); // Get a single, non occupied point randomly

    //     while(tile.occupant != null); 

    //     tile.occupant = Instantiate(stairs, tile.transform.position, Quaternion.identity);
    // }
}


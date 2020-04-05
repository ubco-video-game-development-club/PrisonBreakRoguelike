using System.Collections.Generic;
using UnityEngine;
// using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;  

public class TempLevelController : MonoBehaviour
{
    // public static TempLevelController instance = null;
    // public static int currentLevel = 0;

    // public int numLevels = 1;
    // public int width, height;
    // public int roomSize = 16;
    // public float tileScale = 1f;
    // public int spawnX, spawnY;
    // [Tooltip("The minimum distance from the player's start location that the exit to the next level will spawn.")]
    // public int exitDistance = 1;
    // public BranchMode branchMode;
    // [Tooltip("The probability of a room to completely skip the branch algorithm.")]
    // public float skipProbability;
    // [Tooltip("The probability of a room to branch off in a given direction.")]
    // public float branchProbability;
    // public Room roomPrefab;
    // public Tile tilePrefab;
    // public Tile wallTilePrefab;
    // public GameObject wallParentPrefab;
    // public GameObject stairsPrefab;
    // public float decoChance, itemChance, enemyChance; //enemyChance + itemChance + decoChance + emptyChance = 100% 
    // public GameObject[] decoPrefabs;
    // public GameObject[] itemPrefabs;
    // public GameObject[] enemyPrefabs;
    // [Tooltip("Wall tile sprites with index based on adjacent tiles: 1 = top, 2 = right, 4 = bottom, 8 = left.")]
    // public Sprite[] wallSprites = new Sprite[16];
    // public Sprite[] floorSprites;
    // public float[] floorWeights; // Values must be entered in the same order as the sprites they will corrispond to
    // public Sprite doorSprite;

    // private Room[,] rooms;
    // private Dictionary<string, List<Tile>> walls;
    // private Dictionary<Vector2, Tile> wallTileLookup;
    // private Dictionary<Vector2, Tile> doorTileLookup;
    // private GameObject wallTileParent;

    // void Start()
    // {
    //     if (instance != null)
    //     {
    //         Destroy(gameObject);
    //     }
    //     instance = this;

    //     rooms = new Room[width, height]; 
    //     walls = new Dictionary<string, List<Tile>>();
    //     wallTileLookup = new Dictionary<Vector2, Tile>();
    //     doorTileLookup = new Dictionary<Vector2, Tile>();
    //     wallTileParent = Instantiate(wallParentPrefab, Vector2.zero, Quaternion.identity) as GameObject;
    //     InitializeLevel();
    // }

    // public void LoadFirstLevel()
    // {
    //     currentLevel = 0;
    //     SceneManager.LoadSceneAsync("Level");
    // }

    // public void LoadNextLevel()
    // {
    //     currentLevel++;
    //     // If we've completed enough levels, we reach the win screen
    //     if (currentLevel >= numLevels)
    //     {
    //         SceneManager.LoadSceneAsync("Win");
    //     }
    //     // Otherwise, load the next level
    //     else
    //     {
    //         SceneManager.LoadSceneAsync("Level");
    //     }
    // }

    // public void InitializeLevel() 
    // {
    //     InitializeGrid();
    //     GenerateLevel();
    //     SpawnPlayer();
    // }

    // public Room GetNearestRoom(Vector3 position)
    // {
    //     Room nearestRoom = null;
    //     float nearestDist = Mathf.Infinity;
    //     for (int i = 0; i < rooms.GetLength(0); i++)
    //     {
    //         for (int j = 0; j < rooms.GetLength(1); j++)
    //         {
    //             Room room = rooms[i, j];
    //             float dist = DistanceToRoom(position, room);
    //             if (dist < nearestDist)
    //             {
    //                 nearestRoom = room;
    //                 nearestDist = dist;
    //             }
    //         }
    //     }
    //     return nearestRoom;
    // }

    // public float DistanceToRoom(Vector2 position, Room room)
    // {
    //     Vector2 roomCenterPos = room.transform.position;
    //     roomCenterPos += Vector2.one * (roomSize / 2.0f);
    //     float dist = Vector3.Distance(position, roomCenterPos);
    //     return dist;
    // }

    // public static Vector2 RoundToTilePosition(Vector2 worldPosition)
    // {
    //     float tileX = Mathf.Floor(worldPosition.x) + 0.5f;
    //     float tileY = Mathf.Floor(worldPosition.y) + 0.5f;
    //     return new Vector2(tileX, tileY);
    // }

    // public Tile WallTileAt(Vector2 position)
    // {
    //     return wallTileLookup[position];
    // }

    // public Tile DoorTileAt(Vector2 position)
    // {
    //     return doorTileLookup.ContainsKey(position) ? doorTileLookup[position] : null;
    // }

    // public List<Room> GetAdjacentInitializedRooms(int x, int y)
    // {
    //     // List<Room> result = GetAdjacentRooms(x, y);
    //     // for (int i = 0; i < result.Count; i++)
    //     // {
    //     //     if (!result[i].initialized)
    //     //     {
    //     //         result.RemoveAt(i);
    //     //     }
    //     // }
    //     // return result;
    //     List<Room> result = new List<Room>();
    //     if (IsRoomValid(x+1, y) && rooms[x+1, y].initialized)
    //     {
    //         result.Add(rooms[x+1, y]);
    //     }
    //     if (IsRoomValid(x-1, y) && rooms[x-1, y].initialized)
    //     {
    //         result.Add(rooms[x-1, y]);
    //     }
    //     if (IsRoomValid(x, y+1) && rooms[x, y+1].initialized)
    //     {
    //         result.Add(rooms[x, y+1]);
    //     }
    //     if (IsRoomValid(x, y-1) && rooms[x, y-1].initialized)
    //     {
    //         result.Add(rooms[x, y-1]);
    //     }
    //     return result;

    // }

    // public List<Room> GetAdjacentUnvisitedRooms(int x, int y)
    // {
    //     // List<Room> result = GetAdjacentRooms(x, y);
    //     // for (int i = 0; i < result.Count; i++)
    //     // {
    //     //     if (result[i].visited)
    //     //     {
    //     //         result.RemoveAt(i);
    //     //     }
    //     // }
    //     // return result;
    //     List<Room> result = new List<Room>();
    //     if (IsRoomValid(x+1, y) && !rooms[x+1, y].visited)
    //     {
    //         result.Add(rooms[x+1, y]);
    //     }
    //     if (IsRoomValid(x-1, y) && !rooms[x-1, y].visited)
    //     {
    //         result.Add(rooms[x-1, y]);
    //     }
    //     if (IsRoomValid(x, y+1) && !rooms[x, y+1].visited)
    //     {
    //         result.Add(rooms[x, y+1]);
    //     }
    //     if (IsRoomValid(x, y-1) && !rooms[x, y-1].visited)
    //     {
    //         result.Add(rooms[x, y-1]);
    //     }
    //     return result;
    // }

    // public List<Room> GetAdjacentRooms(int x, int y)
    // {
    //     List<Room> result = new List<Room>();
    //     if (IsRoomValid(x+1, y))
    //     {
    //         result.Add(rooms[x+1, y]);
    //     }
    //     if (IsRoomValid(x-1, y))
    //     {
    //         result.Add(rooms[x-1, y]);
    //     }
    //     if (IsRoomValid(x, y+1))
    //     {
    //         result.Add(rooms[x, y+1]);
    //     }
    //     if (IsRoomValid(x, y-1))
    //     {
    //         result.Add(rooms[x, y-1]);
    //     }
    //     return result;
    // }

    // public bool IsRoomValid(int x, int y)
    // {
    //     return !IsRoomOutOfBounds(x, y) && !IsRoomNull(x, y);
    // }

    // public static int TileDistance(int x1, int y1, int x2, int y2)
    // {
    //     return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    // }

    // public Room GetRandomRoom(List<Room> rooms)
    // {
    //     int rand = Random.Range(0, rooms.Count);
    //     return rooms[rand];
    // }

    // public Tile GetRandomTile(List<Tile> tiles)
    // {
    //     int rand = Random.Range(0, tiles.Count);
    //     return tiles[rand];
    // }

    // private void InitializeGrid()
    // {
    //     Transform parentRoom = new GameObject("Rooms").transform;
    //     for (int i = 0; i < width; i++)
    //     {
    //         for (int j = 0; j < height; j++) 
    //         {
    //             float scale = (roomSize + 1) * tileScale;
    //             Vector2 pos = new Vector2(i * scale, j  * scale);
    //             Room room = Instantiate(roomPrefab, pos, Quaternion.identity, parentRoom) as Room;
    //             room.name = "Room[" + i + ", " + j + "]";
    //             room.x = i;
    //             room.y = j;
    //             rooms[i, j] = room;
    //         }
    //     }
    // }

    // private void GenerateLevel()
    // {
    //     List<Room> validExits = new List<Room>();
    //     for (int i = 0; i < width; i++)
    //     {
    //         for (int j = 0; j < height; j++)
    //         {
    //             if (TileDistance(i, j, spawnX, spawnY) > exitDistance)
    //             {
    //                 validExits.Add(rooms[i, j]);
    //             }
    //         }
    //     }
    //     Room exit = GetRandomRoom(validExits);
    //     List<Room> exitPath = GeneratePath(exit.x, exit.y, spawnX, spawnY);
    //     List<Room> generatedRooms = GenerateBranches(exitPath);
    //     GenerateBoundaryWall(); 
    //     AssignWallSprites();
    //     foreach (Room room in generatedRooms)
    //     {
    //         if (room.x == spawnX && room.y == spawnY)
    //         {
    //             room.InitializeTiles(tilePrefab, roomSize, tileScale, Color.blue, floorSprites, floorWeights);
    //         }
    //         else if (room.x == exit.x && room.y == exit.y)
    //         {
    //             room.InitializeTiles(tilePrefab, roomSize, tileScale, Color.red, floorSprites, floorWeights);
    //         }
    //         else if (exitPath.Contains(room))
    //         {
    //             room.InitializeTiles(tilePrefab, roomSize, tileScale, Color.white, floorSprites, floorWeights);
    //         }
    //         else
    //         {
    //             room.InitializeTiles(tilePrefab, roomSize, tileScale, Color.white, floorSprites, floorWeights);
    //         }
    //         room.InitializeObjects(decoPrefabs, itemPrefabs, enemyPrefabs, decoChance, itemChance, enemyChance); 
    //     }
    //     exit.InitializeExit(stairsPrefab); //Generate stairs in exit room
    //     Debug.Log("Done");
    // }

    // private void SpawnPlayer()
    // {
    //     Room spawn = rooms[spawnX, spawnY];
    //     Vector3 roomPos = spawn.transform.position;
    //     Vector3 offset = new Vector2(roomSize * tileScale / 2, roomSize * tileScale / 2);
    //     Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    //     player.Spawn(spawn, roomPos + offset);
    // }

    // private List<Room> GeneratePath(int startX, int startY, int endX, int endY)
    // {
    //     List<Room> path;

    //     do
    //     {
    //         path = new List<Room>();
    //         ClearVisited();

    //         Room start = rooms[startX, startY];
    //         start.visited = true;
    //         path.Add(start);

    //         path = GeneratePath(path, startX, startY, endX, endY);
    //     }
    //     while (path == null);

    //     GeneratePathWalls(path);

    //     return path;
    // }

    // private List<Room> GeneratePath(List<Room> path, int x, int y, int endX, int endY)
    // {
    //     List<Room> adjacentRooms = GetAdjacentUnvisitedRooms(x, y);

    //     if (adjacentRooms.Count == 0)
    //     {
    //         return null;
    //     }

    //     Room next = GetRandomRoom(adjacentRooms);
    //     next.visited = true;
    //     path.Add(next);

    //     if (next.x == endX && next.y == endY)
    //     {
    //         return path;
    //     }

    //     return GeneratePath(path, next.x, next.y, endX, endY);
    // }

    // private void GeneratePathWalls(List<Room> path)
    // {
    //     ClearVisited();
    //     for (int i = 0; i < path.Count; i++)
    //     {
    //         Room room = path[i];
    //         room.visited = true;

    //         // Generate walls
    //         List<Room> adjacentRooms = GetAdjacentUnvisitedRooms(room.x, room.y);
    //         foreach (Room adj in adjacentRooms)
    //         {
    //             List<Tile> wall = GetWall(room, adj);
    //             if (wall == null)
    //             {
    //                 wall = GenerateWall(room, adj);
    //             }
    //         }
            
    //         // Connect the path with doors
    //         if (i < path.Count - 1)
    //         {
    //             List<Tile> wall = GetWall(room, path[i+1]);
    //             GenerateRandomDoor(wall);
    //         }
    //     }
    // }

    // private List<Room> GenerateBranches(List<Room> path)
    // {
    //     List<Room> generatedRooms = new List<Room>();

    //     // start from the spawn point
    //     path.Reverse();
    //     List<Room> roomList = new List<Room>(path);
    //     while (roomList.Count > 0)
    //     {
    //         Room room = roomList[0];
    //         roomList.RemoveAt(0);
    //         room.visited = true;
            
    //         if (generatedRooms.Contains(room))
    //         {
    //             continue;
    //         }
    //         generatedRooms.Add(room);

    //         // chance to not branch at all from this room
    //         bool skipBranching = Random.Range(0, 1.0f) < skipProbability;

    //         List<Room> adjacentRooms = GetAdjacentUnvisitedRooms(room.x, room.y);
    //         foreach (Room adj in adjacentRooms)
    //         {
    //             List<Tile> wall = GetWall(room, adj);
    //             if (wall == null)
    //             {
    //                 wall = GenerateWall(room, adj);
    //             }

    //             if (!skipBranching && Random.Range(0, 1.0f) < branchProbability)
    //             {
    //                 if (branchMode == BranchMode.DEPTH_FIRST)
    //                 {
    //                     // depth-first uses stack
    //                     roomList.Insert(0, adj);
    //                 }
    //                 else if (branchMode == BranchMode.BREADTH_FIRST)
    //                 {
    //                     // breadth-first uses queue
    //                     roomList.Add(adj);
    //                 }
    //                 GenerateRandomDoor(wall);
    //             }
    //         }
    //     }

    //     return generatedRooms;
    // }

    // private void GenerateBoundaryWall()
    // {
    //     float posX = transform.position.x;
    //     float posY = transform.position.y;
    //     Vector2 tileOffset = new Vector2(tileScale / 2, tileScale / 2);

    //     for (int i = 0; i < width * (roomSize + 1) - 1; i++)
    //     {
    //         // Create the bottom wall
    //         Vector2 pos = new Vector2(posX + (i * tileScale), posY - tileScale);

    //         Tile wallTile = CreateWallTile(pos + tileOffset);

    //         // Create the top wall
    //         float heightScale = height * (roomSize + 1) * tileScale;
    //         pos = new Vector2(posX + (i * tileScale), posY + heightScale - tileScale);

    //         wallTile = CreateWallTile(pos + tileOffset);
    //     }

    //     for (int i = 0; i < height * (roomSize + 1) - 1; i++)
    //     {
    //         // Create the left wall
    //         Vector2 pos = new Vector2(posX - tileScale, posY + (i * tileScale));

    //         Tile wallTile = CreateWallTile(pos + tileOffset);

    //         // Create the right wall
    //         float widthScale = width * (roomSize + 1) * tileScale;
    //         pos = new Vector2(posX + widthScale - tileScale, posY  + (i * tileScale));

    //         wallTile = CreateWallTile(pos + tileOffset);
    //     }
    // }

    // //Generates a wall between two rooms
    // private List<Tile> GenerateWall(Room room1, Room room2)
    // {
    //     List<Tile> wall = new List<Tile>();

    //     // Get the direction of the wall with respect to room 1
    //     int dirX = Mathf.Clamp(room2.x - room1.x, -1, 1);
    //     int dirY = Mathf.Clamp(room2.y - room1.y, -1, 1);

    //     // Get the world position of room 1
    //     float startX = room1.transform.position.x; 
    //     float startY = room1.transform.position.y;

    //     // Offset by 1 room if dir is 1
    //     float offsetX = (roomSize + 1) * tileScale * Mathf.Clamp(dirX, 0, 1);
    //     float offsetY = (roomSize + 1) * tileScale * Mathf.Clamp(dirY, 0, 1);

    //     // Offset by half tileScale because tiles are center-aligned
    //     float tileOffset = 0.5f * tileScale;

    //     // Offset by -1 tilescale
    //     float dirOffsetX = -tileScale * Mathf.Abs(dirX);
    //     float dirOffsetY = -tileScale * Mathf.Abs(dirY);

    //     float posX = startX + offsetX + tileOffset + dirOffsetX;
    //     float posY = startY + offsetY + tileOffset + dirOffsetY;

    //     // Include corner tiles at the ends of the wall
    //     for(int i = -1; i < roomSize + 1; i++)
    //     {
    //         float indexX = (Mathf.Abs(dirY) * i) * tileScale;
    //         float indexY = (Mathf.Abs(dirX) * i) * tileScale;
    //         Vector2 pos = new Vector2(posX + indexX, posY + indexY);
    //         Tile wallTile = CreateWallTile(pos);
    //         wall.Add(wallTile);
    //     }

    //     string key1 = room1.ToString() + room2.ToString();
    //     string key2 = room2.ToString() + room1.ToString();
    //     walls.Add(key1, wall);
    //     walls.Add(key2, wall);

    //     return wall;
    // }

    // private void GenerateRandomDoor(List<Tile> wall)
    // {
    //     List<Tile> validTiles = wall.GetRange(2, wall.Count - 4);
    //     List<Tile> chosenTiles = new List<Tile>();
    //     Tile centerTile = GetRandomTile(validTiles);
    //     chosenTiles.Add(centerTile);
    //     chosenTiles.Add(wall[wall.IndexOf(centerTile) - 1]);
    //     chosenTiles.Add(wall[wall.IndexOf(centerTile) + 1]);
    //     foreach (Tile tile in chosenTiles)
    //     {
    //         wallTileLookup.Remove(tile.transform.position);
    //         doorTileLookup.Add(tile.transform.position, tile);
    //         tile.GetComponent<BoxCollider2D>().isTrigger = true;
    //         tile.GetComponent<SpriteRenderer>().sprite = doorSprite;
    //         tile.GetComponent<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Background");
    //         Destroy(tile.GetComponent<ShadowCaster2D>());
    //     }
    // }

    // private void AssignWallSprites()
    // {
    //     foreach (Tile wallTile in wallTileLookup.Values)
    //     {
    //         int spriteIndex = 0;

    //         // top = 1
    //         Vector2 top = wallTile.transform.position + new Vector3(0, tileScale);
    //         spriteIndex += wallTileLookup.ContainsKey(top) ? 1 : 0;
            
    //         // right = 2
    //         Vector2 right = wallTile.transform.position + new Vector3(tileScale, 0);
    //         spriteIndex += wallTileLookup.ContainsKey(right) ? 2 : 0;
            
    //         // top = 4
    //         Vector2 bottom = wallTile.transform.position + new Vector3(0, -tileScale);
    //         spriteIndex += wallTileLookup.ContainsKey(bottom) ? 4 : 0;
            
    //         // top = 8
    //         Vector2 left = wallTile.transform.position + new Vector3(-tileScale, 0);
    //         spriteIndex += wallTileLookup.ContainsKey(left) ? 8 : 0;

    //         wallTile.GetComponent<SpriteRenderer>().sprite = wallSprites[spriteIndex];
    //     }
    // }

    // private void ClearVisited()
    // {
    //     for (int i = 0; i < width; i++)
    //     {
    //         for (int j = 0; j < height; j++)
    //         {
    //             rooms[i, j].visited = false;
    //         }
    //     }
    // }

    // private bool IsRoomNull(int x, int y)
    // {
    //     return rooms[x, y] == null;
    // }

    // private bool IsRoomOutOfBounds(int x, int y)
    // {
    //     return (
    //         x < 0 || x >= rooms.GetLength(0) ||
    //         y < 0 || y >= rooms.GetLength(1)
    //     );
    // }

    // // Creates a wall tile at the given position or returns 
    // // the wall tile that currently exists at that position
    // private Tile CreateWallTile(Vector2 pos)
    // {
    //     if (wallTileLookup.ContainsKey(pos))
    //     {
    //         return wallTileLookup[pos];
    //     }
    //     Tile wallTile = Instantiate(wallTilePrefab, pos, Quaternion.identity, wallTileParent.transform);
    //     wallTile.name = "WallTile[" + pos.x + ", " + pos.y + "]";
    //     wallTile.isWall = true;
    //     wallTileLookup.Add(pos, wallTile);
    //     return wallTile;
    // }

    // // Returns the wall if it exists or null if not
    // private List<Tile> GetWall(Room room1, Room room2)
    // {
    //     string key = room1.ToString() + room2.ToString();
    //     return walls.ContainsKey(key) ? walls[key] : null;
    // }
}

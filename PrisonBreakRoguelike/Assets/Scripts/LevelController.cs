using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public int width, height;
    public int roomSize = 16;
    public float tileScale = 1f;
    public int spawnX, spawnY;
    [Tooltip("The minimum distance from the player's start location that the exit to the next level will spawn.")]
    public int exitDistance = 1;
    public Room roomPrefab;
    public Tile tilePrefab;

    private Room[,] rooms;

    void Start()
    {
        InitializeRooms();
        InitializeExitPath();
        SpawnPlayer();
    }

    private void InitializeRooms()
    {
        rooms = new Room[width, height];
        Transform parentRoom = new GameObject("Rooms").transform;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++) 
            {
                float scale = (roomSize + 1) * tileScale;
                Vector2 pos = new Vector2(i * scale, j  * scale);
                Room room = Instantiate(roomPrefab, pos, Quaternion.identity, parentRoom) as Room;
                room.name = "Room[" + i + ", " + j + "]";
                room.x = i;
                room.y = j;
                rooms[i, j] = room;
            }
        }
    }

    private void InitializeExitPath()
    {
        List<Room> validExits = new List<Room>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (TileDistance(i, j, spawnX, spawnY) > exitDistance)
                {
                    validExits.Add(rooms[i, j]);
                }
            }
        }
        Room exit = GetRandomRoom(validExits);
        List<Room> exitPath = GeneratePath(exit.x, exit.y, spawnX, spawnY);
        foreach (Room room in exitPath)
        {
            if (room.x == spawnX && room.y == spawnY)
            {
                room.InitializeTiles(tilePrefab, roomSize, tileScale, Color.blue);
            }
            else if (room.x == exit.x && room.y == exit.y)
            {
                room.InitializeTiles(tilePrefab, roomSize, tileScale, Color.red);
            }
            else
            {
                room.InitializeTiles(tilePrefab, roomSize, tileScale, Color.white);
            }
        }
    }

    private void SpawnPlayer()
    {
        Room spawn = rooms[spawnX, spawnY];
        Vector3 roomPos = spawn.transform.position;
        Vector3 offset = new Vector2(roomSize * tileScale / 2, roomSize * tileScale / 2);
        GameObject.FindGameObjectWithTag("Player").transform.position = roomPos + offset;
    }

    private List<Room> GeneratePath(int startX, int startY, int endX, int endY)
    {
        List<Room> path;

        do
        {
            path = new List<Room>();
            ClearVisited();

            Room start = rooms[startX, startY];
            start.visited = true;
            path.Add(start);

            path = GeneratePath(path, startX, startY, endX, endY);
        }
        while (path == null);

        return path;
    }

    private List<Room> GeneratePath(List<Room> path, int x, int y, int endX, int endY)
    {
        List<Room> adjacentRooms = GetAdjacentRooms(x, y);

        if (adjacentRooms.Count == 0)
        {
            return null;
        }

        Room next = GetRandomRoom(adjacentRooms);
        List<Tile> wall = GenerateWall(rooms[x, y], next);
        GenerateRandomDoor(wall);
        next.visited = true;
        path.Add(next);

        if (next.x == endX && next.y == endY)
        {
            return path;
        }

        return GeneratePath(path, next.x, next.y, endX, endY);
    }

    //Generates a wall between two rooms
    private List<Tile> GenerateWall(Room room1, Room room2)
    {
        List<Tile> wall = new List<Tile>();

        // If room2 is below or to the left, don't offset (0)
        // If room2 is above or to the right, offset (1)
        int dirX = Mathf.Clamp(room2.x - room1.x, -1, 1);
        int dirY = Mathf.Clamp(room2.y - room1.y, -1, 1);

        // Get the world position of room 1
        float startX = room1.transform.position.x; 
        float startY = room1.transform.position.y;

        // Offset by 1 room if dir is 1
        float offsetX = (roomSize + 1) * tileScale * Mathf.Clamp(dirX, 0, 1);
        float offsetY = (roomSize + 1) * tileScale * Mathf.Clamp(dirY, 0, 1);

        // Offset by half tileScale because tiles are center-aligned
        float tileOffset = 0.5f * tileScale;

        // Offset by -1 tilescale in the current direction
        float dirOffsetX = -tileScale * Mathf.Abs(dirX);
        float dirOffsetY = -tileScale * Mathf.Abs(dirY);

        float posX = startX + offsetX + tileOffset + dirOffsetX;
        float posY = startY + offsetY + tileOffset + dirOffsetY;

        for(int i = 0; i < roomSize; i++)
        {
            float indexX = (Mathf.Abs(dirY) * i);
            float indexY = (Mathf.Abs(dirX) * i);
            Vector2 pos = new Vector2(posX + indexX, posY + indexY);
            Tile wallTile = Instantiate(tilePrefab, pos, Quaternion.identity);
            wallTile.name = "WallTile[" + pos.x + ", " + pos.y + "]";
            wallTile.GetComponent<SpriteRenderer>().color = Color.yellow;
            wall.Add(wallTile);
        }

        return wall;
    }

    private void GenerateRandomDoor(List<Tile> wall)
    {
        List<Tile> validTiles = wall.GetRange(2, wall.Count - 4);
        List<Tile> chosenTiles = new List<Tile>();
        Tile centerTile = GetRandomTile(validTiles);
        chosenTiles.Add(centerTile);
        chosenTiles.Add(wall[wall.IndexOf(centerTile) - 1]);
        chosenTiles.Add(wall[wall.IndexOf(centerTile) + 1]);
        foreach (Tile tile in chosenTiles)
        {
            tile.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    private void ClearVisited()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                rooms[i, j].visited = false;
            }
        }
    }

    private List<Room> GetAdjacentRooms(int x, int y)
    {
        List<Room> result = new List<Room>();
        if (!IsRoomBlocked(x+1, y))
        {
            result.Add(rooms[x+1, y]);
        }
        if (!IsRoomBlocked(x-1, y))
        {
            result.Add(rooms[x-1, y]);
        }
        if (!IsRoomBlocked(x, y+1))
        {
            result.Add(rooms[x, y+1]);
        }
        if (!IsRoomBlocked(x, y-1))
        {
            result.Add(rooms[x, y-1]);
        }
        return result;
    }

    private bool IsRoomBlocked(int x, int y)
    {
        return IsOutOfBounds(x, y) || IsRoomBlocked(rooms[x, y]);
    }

    private bool IsRoomBlocked(Room room)
    {
        return room == null || room.visited == true;
    }

    private bool IsOutOfBounds(int x, int y)
    {
        return (
            x < 0 || x >= rooms.GetLength(0) ||
            y < 0 || y >= rooms.GetLength(1)
        );
    }

    private int TileDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }

    private Room GetRandomRoom(List<Room> rooms)
    {
        int rand = Random.Range(0, rooms.Count);
        return rooms[rand];
    }

    private Tile GetRandomTile(List<Tile> tiles)
    {
        int rand = Random.Range(0, tiles.Count);
        return tiles[rand];
    }

    void Update()
    {
        
    }
}

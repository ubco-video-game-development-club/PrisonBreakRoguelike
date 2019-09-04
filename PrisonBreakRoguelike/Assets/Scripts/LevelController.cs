using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public int width, height;
    public int spawnX, spawnY;
    [Tooltip("The minimum distance from the player's start location that the exit to the next level will spawn.")]
    public int exitDistance = 1;
    public Room roomPrefab;

    private Room[,] rooms;

    void Start()
    {
        InitializeRooms();
        InitializeExitPath();
    }

    private void InitializeRooms()
    {
        rooms = new Room[width, height];
        Transform parentRoom = new GameObject("Rooms").transform;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float scale = roomPrefab.size * roomPrefab.tileScale;
                Vector2 pos = new Vector2(i * scale, j * scale);
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
                room.InitializeTiles(Color.blue);
            }
            else if (room.x == exit.x && room.y == exit.y)
            {
                room.InitializeTiles(Color.red);
            }
            else
            {
                room.InitializeTiles(Color.white);
            }
        }
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
        next.visited = true;
        path.Add(next);

        if (next.x == endX && next.y == endY)
        {
            return path;
        }

        return GeneratePath(path, next.x, next.y, endX, endY);
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

    private void CreateDoorway(Room room)
    {
        List<Tile> walls = room.GetWalls();
        int rand = Random.Range(0, walls.Count);
        walls[rand].isDoor = true;
    }

    void Update()
    {
        
    }
}

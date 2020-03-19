using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;  

public class LevelController : MonoBehaviour
{
    public static LevelController instance = null;
    public static int currentLevel = 0;

    public int numLevels = 1;
    public int gridWidth, gridHeight;
    public int roomSize = 16;
    public float tileScale = 1f;
    public int spawnX, spawnY;
    [Tooltip("The minimum distance from the player's start location that the exit to the next level will spawn.")]
    public int exitDistance = 1;
    public BranchMode branchMode;
    [Tooltip("The probability of a room to completely skip the branch algorithm.")]
    public float skipProbability;
    [Tooltip("The probability of a room to branch off in a given direction.")]
    public float branchProbability;
    public Room roomPrefab;
    public Tile tilePrefab;
    public Tile wallTilePrefab;
    public GameObject wallParentPrefab;
    public GameObject stairsPrefab;
    public float decoChance, itemChance, enemyChance; //enemyChance + itemChance + decoChance + emptyChance = 100% 
    public GameObject[] decoPrefabs;
    public GameObject[] itemPrefabs;
    public GameObject[] enemyPrefabs;
    [Tooltip("Wall tile sprites with index based on adjacent tiles: 1 = top, 2 = right, 4 = bottom, 8 = left.")]
    public Sprite[] wallSprites = new Sprite[16];
    public Sprite[] floorSprites;
    public float[] floorWeights; // Values must be entered in the same order as the sprites they will corrispond to
    public Sprite doorSprite;

    private Tile[,] tiles;
    private GameObject wallTileParent;

    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;

        tiles = new Tile[gridWidth, gridHeight];
        wallTileParent = Instantiate(wallParentPrefab, Vector2.zero, Quaternion.identity) as GameObject;
        InitializeLevel();
    }

    public void LoadFirstLevel()
    {
        currentLevel = 0;
        SceneManager.LoadSceneAsync("Level");
    }

    public void LoadNextLevel()
    {
        currentLevel++;
        // If we've completed enough levels, we reach the win screen
        if (currentLevel >= numLevels)
        {
            SceneManager.LoadSceneAsync("Win");
        }
        // Otherwise, load the next level
        else
        {
            SceneManager.LoadSceneAsync("Level");
        }
    }

    public void InitializeLevel()
    {
        GenerateLevel();
        SpawnPlayer();
    }

    public static int GridDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }

    private void GenerateLevel()
    {
        // The procedural generation algorithm generates a list of pairs
        // of rooms between which doorways should be located.
        List<(Vector2Int, Vector2Int)> doors = new List<(Vector2Int, Vector2Int)>();
        
        Vector2Int spawnPoint = new Vector2Int(spawnX, spawnY);
        Vector2Int exitPoint = GenerateExit();
        List<Vector2Int> path = GeneratePath(spawnPoint, exitPoint);
        doors.AddRange(GetPathDoors(path));
        doors.AddRange(GenerateBranches(path));
    }

    private void SpawnPlayer()
    {
        Room spawn = rooms[spawnX, spawnY];
        Vector3 roomPos = spawn.transform.position;
        Vector3 offset = new Vector2(roomSize * tileScale / 2, roomSize * tileScale / 2);
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        player.Spawn(spawn, roomPos + offset);
    }

    ///<summary>Returns a random exit point that is at least exitDistance grid squares away from the player spawn point.</summary>
    private Vector2Int GenerateExit()
    {
        // We use Vector2Ints to represent grid positions (rooms)
        List<Vector2Int> validExits = new List<Vector2Int>();
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                // Add all grid squares that are sufficiently far from the player spawn point
                if (GridDistance(i, j, spawnX, spawnY) > exitDistance)
                {
                    validExits.Add(new Vector2Int(i, j));
                }
            }
        }
        // Choose a random grid square (room)
        int rand = Random.Range(0, validExits.Count);
        return validExits[rand];
    }

    ///<summary>Returns a random path between the player spawn and the exit as a list of steps between rooms.</summary>
    private List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int end)
    {
        // We use Vector2Ints to represent grid positions (rooms)
        List<Vector2Int> path = null;

        // I would describe this as a "flailing arm" path generation algorithm.
        // Each time this loop runs, it tries to find a random path from the start
        // to the exit by moving randomly through the grid room-by-room until it
        // either reaches the exit or gets stuck, in which case it runs again.
        while (path == null)
        {
            // Reset the list at the start of the iteration
            path = new List<Vector2Int>();
            path.Add(start);

            // Keep track of the grid squares we've visited in this iteration
            bool[,] visited = new bool[gridWidth, gridHeight];
            visited[start.x, start.y] = true;

            Vector2Int current = start;
            // Move through the grid room-by-room
            while (!current.Equals(end))
            {
                // Get a random adjacent room
                current = GetRandomAdjacentRoom(current, visited);

                // No more adjacent rooms - we're stuck! Break and try again.
                if (current == null)
                {
                    path = null;
                    break;
                }
                
                // Add this step to our list
                path.Add(current);
                visited[current.x, current.y] = true;
            }
        }

        return path;
    }

    ///<summary>Returns a list of doors (pairs of rooms) generated from a path of rooms.</summary>
    private List<(Vector2Int, Vector2Int)> GetPathDoors(List<Vector2Int> path)
    {
        List<(Vector2Int, Vector2Int)> doors = new List<(Vector2Int, Vector2Int)>();
        for (int i = 0; i < path.Count - 1; i++)
        {
            doors.Add((path[i], path[i+1]));
        }
        return doors;
    }

    ///<summary>Returns a list of doors (pairs of rooms) generated from a set of random branches off a given path of rooms.</summary>
    private List<(Vector2Int, Vector2Int)> GenerateBranches(List<Vector2Int> path)
    {
        // We use Vector2Ints to represent grid positions (rooms)
        List<(Vector2Int, Vector2Int)> branches = new List<(Vector2Int, Vector2Int)>();

        // Copy the list of rooms from which to branch
        List<Vector2Int> rooms = new List<Vector2Int>(path);

        // Keep track of the grid squares we've visited
        bool[,] visited = new bool[gridWidth, gridHeight];

        // This algorithm works simply by stepping through each room in the path,
        // and randomly choosing to generate doors in any given direction. You can
        // customize various aspects of this process in the Unity editor.
        while (rooms.Count > 0)
        {
            // Pop the first room off the list
            Vector2Int current = rooms[0];
            rooms.RemoveAt(0);
            visited[current.x, current.y] = true;

            // Determine whether to skip branching on this room entirely
            if (Random.Range(0, 1.0f) < skipProbability)
            {
                continue;
            }

            // For each adjacent unvisited room, randomly decide whether to branch, 
            // and if so, add that room to our list of rooms from which to branch
            List<Vector2Int> adjacentRooms = GetAdjacentRooms(current, visited);
            foreach (Vector2Int adj in adjacentRooms)
            {
                // Determine whether to branch to this room
                if (Random.Range(0, 1.0f) < branchProbability)
                {
                    // Add this room to our list of rooms based on the branchMode
                    if (branchMode == BranchMode.DEPTH_FIRST)
                    {
                        // *** In non-technical terms ***:
                        // We will branch as far as possible off of this room of the path
                        // before trying to branch off of other parts of the path

                        // Depth-first uses a stack datastructure (LIFO)
                        rooms.Insert(0, adj);
                    }
                    else if (branchMode == BranchMode.BREADTH_FIRST)
                    {
                        // *** In non-technical terms ***:
                        // We will branch off of each room in the path evenly by one level at a time

                        // Breadth-first uses a queue datastructure (FIFO)
                        rooms.Add(adj);
                    }

                    // Add this branch to our list of branches
                    branches.Add((current, adj));
                }
            }
        }

        return branches;
    }

    ///<summary>Returns a random adjacent grid position that has not been visited yet.</summary>
    private Vector2Int GetRandomAdjacentRoom(Vector2Int room, bool[,] visited)
    {
        List<Vector2Int> adjacentRooms = GetAdjacentRooms(room, visited);
        int rand = Random.Range(0, adjacentRooms.Count);
        return adjacentRooms[rand];
    }

    ///<summary>Returns a list of adjacent grid positions that have not been visited yet.</summary>
    private List<Vector2Int> GetAdjacentRooms(Vector2Int room, bool[,] visited)
    {
        // Generate a list of valid adjacent rooms
        List<Vector2Int> adjacentRooms = new List<Vector2Int>();
        Vector2Int right = room + Vector2Int.right;
        if (IsInBounds(right) && !visited[right.x, right.y])
        {
            adjacentRooms.Add(right);
        }
        Vector2Int left = room + Vector2Int.left;
        if (IsInBounds(left) && !visited[left.x, left.y])
        {
            adjacentRooms.Add(left);
        }
        Vector2Int top = room + Vector2Int.up;
        if (IsInBounds(top) && !visited[top.x, top.y])
        {
            adjacentRooms.Add(top);
        }
        Vector2Int bottom = room + Vector2Int.down;
        if (IsInBounds(bottom) && !visited[bottom.x, bottom.y])
        {
            adjacentRooms.Add(bottom);
        }
        return adjacentRooms;
    }

    ///<summary>Checks if a grid position is with the bounds of the grid.</summary>
    private bool IsInBounds(Vector2Int room)
    {
        return room.x < gridWidth && room.x >= 0 && 
            room.y < gridHeight && room.y >= 0;
    }
}

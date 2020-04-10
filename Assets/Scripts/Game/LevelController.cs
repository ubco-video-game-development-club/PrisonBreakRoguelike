using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  

public class LevelController : MonoBehaviour
{
    public static LevelController instance = null;
    public static int currentLevel = 0;

    [Header("Level Generation Options")]
    [Tooltip("The number of game levels that the player must complete to win.")]
    public int numLevels = 1;

    [Tooltip("The number of rooms along the x-axis that should be generated in the level.")]
    public int gridWidth;
    
    [Tooltip("The number of rooms along the y-axis that should be generated in the level.")]
    public int gridHeight;

    [Tooltip("The number of tiles that form the side length of each room. Rooms are square.")]
    public int roomSize = 16;

    [Tooltip("The number of tiles wide the doorways between rooms are.")]
    public int doorSize = 3;

    [Tooltip("The x index of the room the player should spawn in. Room(0,0) is the bottom left room.")]
    public int spawnX;
    
    [Tooltip("The y index of the room the player should spawn in. Room(0,0) is the bottom left room.")]
    public int spawnY;

    [Tooltip("The minimum number of rooms from the player's start room that the exit to the next level will spawn.")]
    public int exitDistance = 1;

    [Tooltip("Determines what approach the branching algorithm uses. See GenerateBranches() for details.")]
    public BranchMode branchMode;

    [Tooltip("The probability of a room to completely skip the branch algorithm.")]
    public float skipProbability;

    [Tooltip("The probability of a room to branch off in a given direction.")]
    public float branchProbability;

    [Header("Tile Options")]
    [Tooltip("The Tile object used to generate each tile in the grid.")]
    public Tile tilePrefab;

    [Tooltip("The list of possible sprites that may be assigned to floor tiles.")]
    public Sprite[] tileSprites;

    [Tooltip("The list of associated probabilities of each tile sprite being selected.")]
    public float[] tileWeights;

    [Tooltip("The parent object that should contain all the tile objects.")]
    public GameObject tileParentPrefab;

    [Tooltip("The Tile object used to generate each wall tile in the grid.")]
    public Tile wallTilePrefab;

    [Tooltip("Wall tile sprites with index based on adjacent walls: +1 = wall above, +2 = wall to right, +4 = wall below, +8 = wall to left. Example: 7 = adjacent walls above, right, and below.")]
    public Sprite[] wallSprites = new Sprite[16];

    [Tooltip("The parent object that should contain all the wall tile objects. NOTE: should have a CompositeShadowCaster2D component for efficiency.")]
    public GameObject wallTileParentPrefab;

    [Tooltip("The parent object that should contain all the door tile objects.")]
    public GameObject doorTileParentPrefab;

    [Header("Object Spawn Options")]
    [Tooltip("The stairs object that is spawned in the exit room for moving between levels.")]
    public TileObject stairsPrefab;

    [Tooltip("The list of possible deco objects that may be spawned.")]
    public TileObject[] decoPrefabs;

    [Tooltip("The probability of a deco object being spawned at a given tile. Deco + Item + Enemy + Empty Chance = 100%")]
    public float decoChance;

    [Tooltip("The list of possible items that may be spawned.")]
    public TileObject[] itemPrefabs;

    [Tooltip("The probability of an item being spawned at a given tile. Deco + Item + Enemy + Empty Chance = 100%")]
    public float itemChance;

    [Tooltip("The list of possible enemies that may be spawned.")]
    public Enemy[] enemyPrefabs;

    [Tooltip("The probability of an enemy being spawned at a given tile. Deco + Item + Enemy + Empty Chance = 100%")]
    public float enemyChance;

    [HideInInspector]
    public Tile[,] tiles;

    void Start()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;

        GenerateLevel();
    }

    ///<summary>Loads the next game level and checks if the player has reached the end.</summary>
    public void LoadNextLevel()
    {
        currentLevel++;
        // If we've completed enough levels, we reach the win screen
        if (currentLevel >= numLevels)
        {
            currentLevel = 0;
            SceneManager.LoadSceneAsync("Win");
        }
        // Otherwise, load the next level
        else
        {
            SceneManager.LoadSceneAsync("Level");
        }
    }

    ///<summary>Returns a square of tiles centered at the given position out to a given range.</summary>
    public List<Tile> GetTilesInRange(Vector2Int gridPosition, int tileRange, bool excludeCenter = false)
    {
        List<Tile> result = new List<Tile>();
        for (int y = gridPosition.y - tileRange; y <= gridPosition.y + tileRange; y++)
        {
            for (int x = gridPosition.x - tileRange; x <= gridPosition.x + tileRange; x++)
            {
                // If this is a valid tile position
                if (IsTileInBounds(x, y))
                {
                    // If we have set to exclude the center tile and this is the center, skip
                    if (excludeCenter && gridPosition.x == x && gridPosition.y == y)
                    {
                        continue;
                    }

                    // Get the tile at this position
                    Tile tile = tiles[x, y];

                    // If this tile is not blocked, add it to the list
                    if (!tile.IsBlocked())
                    {
                        result.Add(tile);
                    }
                }
            }
        }
        return result;
    }

    ///<summary>Returns the grid position of the nearest Tile to the given world position.</summary>
    public static Vector2Int WorldToTilePosition(Vector2 worldPosition)
    {
        int x = Mathf.RoundToInt(worldPosition.x);
        int y = Mathf.RoundToInt(worldPosition.y);
        return new Vector2Int(x, y);
    }

    ///<summary>Returns the distance between two spaces in a grid as the x difference plus the y difference.</summary>
    public static int GridDistance(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }

    ///<summary>Checks if a tile is with the bounds of the grid.</summary>
    public bool IsTileInBounds(int tileX, int tileY)
    {
        return tileX < tiles.GetLength(0) && tileX >= 0 && tileY < tiles.GetLength(1) && tileY >= 0;
    }

    ///<summary>Procedurally generates the game level.</summary>
    private void GenerateLevel()
    {
        // The procedural generation algorithm generates a list of pairs
        // of rooms between which doorways should be located.
        List<(Vector2Int, Vector2Int)> doorways = new List<(Vector2Int, Vector2Int)>();
        
        // Determine the player spawn room
        Vector2Int spawnRoom = new Vector2Int(spawnX, spawnY);
        // Generate an exit room at a sufficient distance from the spawn room
        Vector2Int exitRoom = GenerateExit();
        // Generate a random path between the spawn and exit rooms
        List<Vector2Int> path = GeneratePath(spawnRoom, exitRoom);
        // Get the doorways along the path to be added
        doorways.AddRange(GetPathDoors(path));
        // Generate branches off the main path and save the generated doorways
        doorways.AddRange(GenerateBranches(path));

        InitializeTiles();
        InitializeDoors(doorways);
        InitializeSprites();

        SpawnObjects();
        SpawnStairs(exitRoom);
        SpawnPlayer(spawnRoom);
    }

    ///<summary>Initializes all of the tiles in the level.</summary>
    private void InitializeTiles()
    {
        // The number of tiles is equal to the number of rooms * the number of tiles per room
        // +1 for the wall tiles per room, +1 overall for the final set of wall tiles
        int numTilesX = (gridWidth * (roomSize + 1)) + 1;
        int numTilesY = (gridHeight * (roomSize + 1)) + 1;
        tiles = new Tile[numTilesX, numTilesY];

        // Instantiate parent objects for tiles. Used to optimize 2D shadowcasting for walls
        GameObject tileParent = Instantiate(tileParentPrefab, Vector2.zero, Quaternion.identity) as GameObject;
        GameObject wallTileParent = Instantiate(wallTileParentPrefab, Vector2.zero, Quaternion.identity) as GameObject;

        // Initialize all the Tile objects in the grid
        for (int y = 0; y < numTilesY; y++)
        {
            for (int x = 0; x < numTilesX; x++)
            {
                // Check if this is a wall tile
                bool isWall = x % (roomSize + 1) == 0 || y % (roomSize + 1) == 0;

                // Setup object properties
                Tile prefab = isWall ? wallTilePrefab : tilePrefab;
                Vector2 position = new Vector2(x, y);
                GameObject parent = isWall ? wallTileParent : tileParent;

                // Instantiate tile object
                Tile tile = Instantiate(prefab, position, Quaternion.identity, parent.transform) as Tile;
                tile.name = (isWall ? "Wall" : "") + "Tile[" + x + ", " + y + "]";
                tile.isWall = isWall;
                tiles[x, y] = tile;
            }
        }
    }

    ///<summary>Initializes door tiles at the given doorway positions.</summary>
    private void InitializeDoors(List<(Vector2Int, Vector2Int)> doorways)
    {
        // Instantiate parent objects for door tiles.
        GameObject tileParent = Instantiate(doorTileParentPrefab, Vector2.zero, Quaternion.identity) as GameObject;
        tileParent.name = "DoorTiles";

        // BEWARE: This gets pretty heavy into Vector math in the conversion
        // between room coordinates and tile coordinates. Each iteration of this
        // loop basically takes two rooms, locates the wall tiles between those
        // rooms, and cuts a doorway in a random point along that wall.
        foreach ((Vector2Int, Vector2Int) door in doorways)
        {
            // Convert from room coordinates into tile coordinates
            // This gets a little confusing...
            Vector2Int startRoom = door.Item1;
            Vector2Int endRoom = door.Item2;

            // This is basically for determining which side of the
            // startRoom the wall should be placed on. If the door
            // leads vertically between rooms, it's a horizontal wall.
            Vector2Int roomDirection = endRoom - startRoom;
            // This is the direction our wall tiles align with.
            // If it's a horizontal wall, we step through tiles along
            // the x-axis. This is just perpendicular to the absolute
            // value of roomDirection. I know, I'm sorry.
            Vector2Int wallDirection = Vector2Int.one - (roomDirection * roomDirection);

            // Convert startRoom to the tile coordinates of the
            // bottom-left wall tile of that room.
            Vector2Int startRoomTilePos = startRoom * (roomSize + 1);
            // If the room direction is positive, offset by one room size.
            // This is because we start from the bottom-left corner of startRoom.
            Vector2Int startOffset = Vector2.Dot(roomDirection, Vector2.one) > 0 ?
                roomDirection * (roomSize + 1) : Vector2Int.zero;
            // Get the actual start position of our desired wall
            Vector2Int startWallTilePos = startRoomTilePos + startOffset + wallDirection;

            // Get a random index along our wall at which to start the door
            int doorIndex = Random.Range(0, roomSize - doorSize + 1);
            // Get the start position of the door based on the wall start position
            Vector2Int startDoorTilePos = startWallTilePos + (wallDirection * doorIndex);

            // Finally, loop through all the door tiles and set them as doors!
            Vector2Int currentTilePos = startDoorTilePos;
            for (int i = 0; i < doorSize; i++)
            {
                int x = currentTilePos.x;
                int y = currentTilePos.y;

                // Destroy the current wall tile at this position
                Destroy(tiles[x, y].gameObject);

                // Instantiate new tile object
                Vector2 position = new Vector2(x, y);
                Tile tile = Instantiate(tilePrefab, position, Quaternion.identity, tileParent.transform) as Tile;
                tile.name = "DoorTile[" + x + ", " + y + "]";
                tile.isDoor = true;
                tiles[x, y] = tile;
                currentTilePos += wallDirection;
            }
        }
    }

    ///<summary>Initializes all tiles in the level to the appropriate sprites.</summary>
    private void InitializeSprites()
    {
        // The number of tiles is equal to the number of rooms * the number of tiles per room
        // +1 for the wall tiles per room, +1 overall for the final set of wall tiles
        int numTilesX = (gridWidth * (roomSize + 1)) + 1;
        int numTilesY = (gridHeight * (roomSize + 1)) + 1;

        // Set the sprites for each tile in the grid
        for (int y = 0; y < numTilesY; y++)
        {
            for (int x = 0; x < numTilesX; x++)
            {
                // Set each sprite based on whether it's a wall tile or not
                if (tiles[x, y].isWall)
                {
                    // Wall sprites are set based on what walls are adjacent to them.
                    // These are assigned through a bytewise calculation:
                    // Tile above = +1, right = +2, below = +4, left = +8
                    // Then these are added together to get the appropriate sprite from an array.
                    int spriteIndex = 0;
                    spriteIndex += IsTileInBounds(x, y+1) && tiles[x, y+1].isWall ? 1 : 0;
                    spriteIndex += IsTileInBounds(x+1, y) && tiles[x+1, y].isWall ? 2 : 0;
                    spriteIndex += IsTileInBounds(x, y-1) && tiles[x, y-1].isWall ? 4 : 0;
                    spriteIndex += IsTileInBounds(x-1, y) && tiles[x-1, y].isWall ? 8 : 0;
                    tiles[x, y].GetComponent<SpriteRenderer>().sprite = wallSprites[spriteIndex];
                }
                else
                {
                    // Assign a random sprite to this tile based on the sprite weights
                    float rand = Random.Range(0, 1.0f);
                    float total = 0;
                    for (int i = 0; i < tileSprites.Length; i++)
                    {
                        total += tileWeights[i];
                        if (rand < total)
                        {
                            tiles[x, y].GetComponent<SpriteRenderer>().sprite = tileSprites[i];
                            break;
                        }
                    }
                }
            }
        }
    }

    ///<summary>Spawns enemies, deco, and items randomly throughout the generated level.</summary>
    private void SpawnObjects()
    {
        // The number of tiles is equal to the number of rooms * the number of tiles per room
        // +1 for the wall tiles per room, +1 overall for the final set of wall tiles
        int numTilesX = (gridWidth * (roomSize + 1)) + 1;
        int numTilesY = (gridHeight * (roomSize + 1)) + 1;

        // For each tile that is not a wall tile or door tile, spawn objects
        for (int y = 0; y < numTilesY; y++)
        {
            for (int x = 0; x < numTilesX; x++)
            {
                if (!tiles[x, y].isWall && !tiles[x, y].isDoor)
                {
                    // Decide whether this tile should be an enemy, deco or item
                    float rand = Random.Range(0, 1.0f);
                    if (rand < enemyChance)
                    {
                        // Spawn an enemy object
                        int enemyIndex = Random.Range(0, enemyPrefabs.Length);
                        Instantiate(enemyPrefabs[enemyIndex], new Vector2(x, y), Quaternion.identity);
                    }
                    else if (rand < enemyChance + decoChance)
                    {
                        // Spawn a deco object
                        int decoIndex = Random.Range(0, decoPrefabs.Length);
                        tiles[x, y].occupant = Instantiate(decoPrefabs[decoIndex], new Vector2(x, y), Quaternion.identity);
                    }
                    else if (rand < enemyChance + decoChance + itemChance)
                    {
                        // Spawn an item object
                        int itemIndex = Random.Range(0, itemPrefabs.Length);
                        tiles[x, y].occupant = Instantiate(itemPrefabs[itemIndex], new Vector2(x, y), Quaternion.identity);
                    }
                }
            }
        }
    }

    ///<summary>Spawns the stairs object randomly in the exit room.</summary>
    private void SpawnStairs(Vector2Int exitRoom)
    {
        // Get the tile position of the bottom-right corner of the exit room
        int exitRoomTileX = exitRoom.x * (roomSize + 1) + 1;
        int exitRoomTileY = exitRoom.y * (roomSize + 1) + 1;

        // Get a random tile position in the exit room to place the stairs
        int randX = Random.Range(0, roomSize) + exitRoomTileX;
        int randY = Random.Range(0, roomSize) + exitRoomTileY;

        // Make sure we don't already have something on this tile
        if (tiles[randX, randY].occupant != null)
        {
            tiles[randX, randY].occupant.Destroy();
        }

        // Spawn the stairs object
        Vector2 position = new Vector2(randX, randY);
        tiles[randX, randY].occupant = Instantiate(stairsPrefab, position, Quaternion.identity);
    }

    ///<summary>Spawns the player in the spawn room.</summary>
    private void SpawnPlayer(Vector2Int spawnRoom)
    {
        // Get the tile position of the bottom-right corner of the spawn room
        int spawnRoomTileX = spawnRoom.x * (roomSize + 1) + 1;
        int spawnRoomTileY = spawnRoom.y * (roomSize + 1) + 1;

        // Get the centermost tile position in the spawn room
        int spawnTileX = spawnRoomTileX + (roomSize / 2);
        int spawnTileY = spawnRoomTileY + (roomSize / 2);
        
        // Spawn the player
        Player player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        Vector2 position = new Vector2(spawnTileX, spawnTileY);
        player.transform.position = position;
        player.Spawn();
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
                if (GridDistance(i, j, spawnX, spawnY) >= exitDistance)
                {
                    validExits.Add(new Vector2Int(i, j));
                }
            }
        }
        // Choose a random grid square (room)
        int rand = Random.Range(0, validExits.Count);
        return validExits[rand];
    }

    ///<summary>Returns a random path between the player spawn and the exit as a list of rooms.</summary>
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
                if (current.x == -1 && current.y == -1)
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

        // Consider all the rooms in our existing path as visited
        foreach (Vector2Int room in path)
        {
            visited[room.x, room.y] = true;
        }

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

    ///<summary>Returns a random adjacent grid position that has not been visited yet or (-1,-1) if none was found.</summary>
    private Vector2Int GetRandomAdjacentRoom(Vector2Int room, bool[,] visited)
    {
        Vector2Int result = new Vector2Int(-1, -1);
        List<Vector2Int> adjacentRooms = GetAdjacentRooms(room, visited);
        if (adjacentRooms.Count > 0)
        {
            int rand = Random.Range(0, adjacentRooms.Count);
            result = adjacentRooms[rand];
        }
        return result;
    }

    ///<summary>Returns a list of adjacent grid positions that have not been visited yet.</summary>
    private List<Vector2Int> GetAdjacentRooms(Vector2Int room, bool[,] visited)
    {
        // Generate a list of valid adjacent rooms
        List<Vector2Int> adjacentRooms = new List<Vector2Int>();
        Vector2Int right = room + Vector2Int.right;
        if (IsRoomInBounds(right) && !visited[right.x, right.y])
        {
            adjacentRooms.Add(right);
        }
        Vector2Int left = room + Vector2Int.left;
        if (IsRoomInBounds(left) && !visited[left.x, left.y])
        {
            adjacentRooms.Add(left);
        }
        Vector2Int top = room + Vector2Int.up;
        if (IsRoomInBounds(top) && !visited[top.x, top.y])
        {
            adjacentRooms.Add(top);
        }
        Vector2Int bottom = room + Vector2Int.down;
        if (IsRoomInBounds(bottom) && !visited[bottom.x, bottom.y])
        {
            adjacentRooms.Add(bottom);
        }
        return adjacentRooms;
    }

    ///<summary>Checks if a room is with the bounds of the grid.</summary>
    private bool IsRoomInBounds(Vector2Int room)
    {
        return room.x < gridWidth && room.x >= 0 && 
            room.y < gridHeight && room.y >= 0;
    }
}

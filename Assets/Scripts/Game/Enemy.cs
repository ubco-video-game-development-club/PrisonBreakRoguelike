using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol = 0,
        Attack = 1,
        Idle = 2,
        Stunned = 3
    }

    [Header("Movement Settings")]
    [Tooltip("The movement speed of the enemy in attack mode.")]
    public float attackSpeed = 1f;
    [Tooltip("The movement speed of the enemy in patrol mode.")]
    public float patrolSpeed = 1f;
    [Tooltip("The maximum tile distance the enemy will consider when choosing a random nearby target position in patrol mode.")]
    public int maxPatrolDistance = 1;
    [Tooltip("The minimum number of seconds the enemy will idle between patrol movements.")]
    public float minIdleDuration = 1f;
    [Tooltip("The maximum number of seconds the enemy will idle between patrol movements.")]
    public float maxIdleDuration = 1f;

    [Header("Detection Settings")]
    [Tooltip("The range of the frontal vision cone used to detect the player.")]
    public float visionDistance = 1f;
    [Tooltip("The arc angle in degrees of the frontal vision cone used to detect the player.")]
    public float visionArcAngle = 0f;
    [Tooltip("The radius of the circular hearing area used to detect the player.")]
    public float hearingRadius = 1f;
    [Tooltip("The max distance at which the enemy will try to follow the player in attack mode.")]
    public float followDistance = 1f;

    private EnemyState state;
    private EnemyPath path;
    private Vector2 direction;
    private float stunTimer = 0f;
    private float idleTimer = 0f;
    private Player player;

    ///<summary>Set this Enemy to the Stunned state for stunDuration seconds.</summary>
    public void Stun(float stunDuration)
    {
        SetState(EnemyState.Stunned);
        stunTimer = stunDuration;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        state = EnemyState.Idle;
    }

    void Update()
    {
        LookForPlayer();

        switch (state)
        {
            case EnemyState.Patrol:
            {
                bool reachedTarget = MoveToTarget();
                if (reachedTarget)
                {
                    bool reachedEnd = path.TargetNext();
                    if (reachedEnd)
                    {
                        SetState(EnemyState.Idle);
                    }
                }
                break;
            }
            case EnemyState.Idle:
            {
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0)
                {
                    SetState(EnemyState.Patrol);
                }
                break;
            }
            case EnemyState.Attack:
            {
                // Check if the player is out of follow range
                float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
                if (distToPlayer > followDistance)
                {
                    // Set state to patrol
                    SetState(EnemyState.Patrol);
                }

                // Raycast to the player
                Vector3 dir = (player.transform.position - transform.position).normalized;
                float dist = Vector2.Distance(player.transform.position, transform.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, dist);

                // Check if the player is visible 
                Player p;
                if (hit.collider.TryGetComponent<Player>(out p))
                {
                    
                }
                break;
            }
            case EnemyState.Stunned:
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0)
                {
                    SetState(EnemyState.Patrol);
                }
                break;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Player p;
        if (collision.collider.TryGetComponent<Player>(out p))
        {
            p.Die();
        }
    }

    ///<summary>Set the state of the enemy and run any code that should happen on the given state change.</summary>
    private void SetState(EnemyState enemyState)
    {
        state = enemyState;
        switch (state)
        {
            case EnemyState.Patrol:
            {
                target = GetRandomPatrolTarget();
                break;
            }
            case EnemyState.Idle:
            {
                
                break;
            }
            case EnemyState.Attack:
            {
                
                break;
            }
            case EnemyState.Stunned:
            {
                
                break;
            }
        }
    }

    ///<summary>Check to see if the player has been detected by the enemy's vision or hearing.</summary>
    private void LookForPlayer()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
        bool heardPlayer = distToPlayer < hearingRadius;

        Vector2 dirToPlayer = player.transform.position - transform.position;
        float angleToPlayer = Vector2.Angle(dirToPlayer, direction);
        bool sawPlayer = angleToPlayer < visionArcAngle && distToPlayer < visionDistance;

        if (heardPlayer || sawPlayer)
        {
            state = EnemyState.Attack;
        }
    }

    ///<summary>Returns a random nearby position to move towards.</summary>
    private Vector2 GetRandomPatrolTarget()
    {
        // Get all tiles within maxPatrolDistance of the enemy's position
        Vector2Int gridPos = LevelController.WorldToTilePosition(transform.position);
        List<Tile> tiles = LevelController.instance.GetTileRange(gridPos, maxPatrolDistance);
        // Get a random tile from the list and return its position
        return tiles[Random.Range(0, tiles.Count)].transform.position;
    }

    ///<summary>Follows the player using the pathfinding algorithm and attempts to catch them.</summary>
    private void Attack()
    {
        List<Vector2> movePath = new List<Vector2>();
        // if the player is out of range, set state back to patrol and return

        // check if the player has moved 
        Tile playerTile = player.GetClosestTile();
        if (movePath == null || movePath.Count == 0)
        {
            movePath = GetPathToTarget(player.transform.position);
            if (movePath == null || movePath.Count == 0)
            {
                state = EnemyState.Patrol;
                return;
            }
        }
        else
        {
            Vector2 playerPos = playerTile.transform.position;
            Vector2 endPos = movePath[movePath.Count - 1];
            if (Vector2.Distance(playerPos, endPos) > 1)
            {
                movePath = GetPathToTarget(player.transform.position);
            }
        }

        Vector2 nextPos = movePath[0];
        tempPos = nextPos;
        float speed = attackSpeed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, nextPos, speed);
        
        if (Vector2.Distance(transform.position, nextPos) < Mathf.Epsilon)
        {
            movePath.RemoveAt(0);
        }
    }

    private class EnemyPath
    {
        public List<Vector2Int> tilePath;
        public List<Vector2> optimizedPath;
        private int currentIndex = 0;

        public EnemyPath(Vector2 start, Vector2 end)
        {
            bool pathFound = CalculatePath(start, end);
            if (pathFound)
            {
                OptimizePath();
            }
        }

        ///<summary>Returns the current target position along the path. Change to the next target by calling TargetNext().</summary>
        public Vector2 Current()
        {
            return optimizedPath[currentIndex];
        }

        ///<summary>Sets the current target to the next point on the path. Returns true if the end was reached.</summary>
        public bool TargetNext()
        {
            currentIndex++;
            return currentIndex >= optimizedPath.Count;
        }

        ///<summary>Recalculates the current path efficiently based on a slightly adjusted endpoint.</summary>
        public void AdjustEndpoint(Vector2 adjustedEnd)
        {
            // Draw a new path from the current endpoint to the new endpoint
            Vector2Int currentEnd = tilePath[tilePath.Count - 1];
            EnemyPath addon = new EnemyPath(currentEnd, adjustedEnd);

            // Add the new path to the end of our current path
            tilePath.AddRange(addon.tilePath);
            
            // Re-optimize the new extended path
            OptimizePath();
        }

        ///<summary>Calculates the shortest path from start to end. Returns a success boolean for whether a path was found.</summary>
        private bool CalculatePath(Vector2 start, Vector2 end)
        {
            // Success boolean
            bool success = false;

            // Convert the start and end positions to tile coordinates
            Vector2Int startTilePos = LevelController.WorldToTilePosition(start);
            Vector2Int endTilePos = LevelController.WorldToTilePosition(end);

            // Get the max range we are willing to check for a valid path to the target
            // Currently that is set to the distance to the target plus a buffer of 3 tiles
            int maxRange = Mathf.CeilToInt(Vector2.Distance(start, end)) + 3;

            // Store a list of tile positions we've been to. Each tile stores the previous step taken to get there.
            // This will let us loop back through all the previous steps to find the path taken to reach the target.
            Dictionary<Vector2Int, Vector2Int> pathMap = new Dictionary<Vector2Int, Vector2Int>();

            // Instantiate the priority queue of tile positions we still need to check.
            // This allows us to easily check tiles that are closer to the end position first.
            PriorityQueue<Vector2Int, float> queue = new PriorityQueue<Vector2Int, float>();

            Vector2Int currentTilePos = startTilePos;
            // While the current position is within our maxRange
            while (Vector2.Distance(currentTilePos, startTilePos) < maxRange)
            {
                // If we reached the end position, backtrack along the path we took
                if (currentTilePos == endTilePos)
                {
                    // While we haven't reached the start of the path, get the previous step
                    while (currentTilePos != startTilePos)
                    {
                        tilePath.Add(currentTilePos);
                        currentTilePos = pathMap[currentTilePos];
                    }
                    // We want the path TO the target not FROM the target
                    tilePath.Reverse();
                    success = true;
                    break;
                }

                // Add adjacent tile positions to the queue
                for (float angle = 0; angle < 2 * Mathf.PI; angle += Mathf.PI / 2)
                {
                    // Get the axis-aligned direction determined by the current 90 degree angle
                    Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Vector2Int adj = currentTilePos + new Vector2Int(Mathf.RoundToInt(dir.x), Mathf.RoundToInt(dir.y));

                    // If this is a valid tile position
                    if (LevelController.instance.IsTileInBounds(adj.x, adj.y))
                    {
                        // Get the tile at this position
                        Tile tile = LevelController.instance.tiles[adj.x, adj.y];

                        // If this tile is not a wall and has not already been visited
                        if (!tile.isWall && !pathMap.ContainsKey(adj))
                        {
                            // Add this tile position to the queue
                            queue.Add(adj, Vector2.Distance(adj, endTilePos));
                        }
                    }
                }

                // Move to the next tile in the queue
                currentTilePos = queue.Remove();
            }

            return success;
        }

        ///<summary>Uses raycasting to determine the most direct route through points in the path and removes unnecessary points.</summary>
        private void OptimizePath()
        {
            optimizedPath = new List<Vector2>();

            int currentIndex = 0;
            int lastKeyIndex = 0;
            // While current index is less than path length
            while (currentIndex < tilePath.Count)
            {
                Vector2 currentPoint = tilePath[currentIndex];
                Vector2 lastKeyPoint = tilePath[lastKeyIndex];

                // Raycast from current point to last key point
                Vector2 dir = (lastKeyPoint - currentPoint).normalized;
                float dist = Vector2.Distance(currentPoint, lastKeyPoint);
                RaycastHit2D hit = Physics2D.Raycast(currentPoint, dir, dist);

                // If the current point is not visible from last key point
                if (hit.collider == null)
                {
                    // Set the previous index as the new last key point
                    lastKeyIndex = currentIndex - 1;

                    // Add that point to our optimized path
                    optimizedPath.Add(tilePath[lastKeyIndex]);
                }

                // Move to the next point
                currentIndex++;
            }
        }
    }







    ///<summary>
    /// Returns true if the target is within followDistance and is in the same room or an adjacent room to the Enemy.
    ///</summary>
    private bool IsTargetInRange(Vector2 target)
    {
        return true;
    }

    ///<summary>
    /// Returns true if the Enemy has line of sight to the player. Obstacles and walls block line of sight.
    ///</summary>
    private bool IsTargetVisible(Vector2 target)
    {
        return true;
    }

    ///<summary>
    /// Returns true if the Enemy already has a valid path to the player and the player has not moved too far from their previous position.
    ///</summary>
    private bool HasPathToTarget(Vector2 target)
    {
        return true;
    }

    ///<summary>
    /// Moves the enemy towards a target position.
    ///</summary>
    private void MoveToTarget(Vector2 target)
    {
        return;
    }

    ///<summary>
    /// Returns the shortest path to the target position as an ordered list of points.
    ///</summary>
    private List<Vector2> GetPathToTarget(Vector2 target)
    {
        // Get a lookup table of nearby tiles to check (so we can get tiles by position)
        Dictionary<Vector2, Tile> tilesInRange = GetTilesInRange();
        // Store a list of tiles we've checked. Each tile stores the previous step taken to get there.
        // This will let us loop back through all the previous steps to find the path taken to reach the target.
        Dictionary<Vector2, Vector2> pathMap = new Dictionary<Vector2, Vector2>();
        // A priority queue which prioritizes checking tiles that are closer to the target for efficiency
        PriorityQueue<Tile, float> queue = new PriorityQueue<Tile, float>();

        // Start from the current tile the enemy is on
        Tile currentTile = GetCurrentTile();
        // Use negativeInfinity to represent the end of our path
        pathMap.Add(currentTile.transform.position, Vector2.negativeInfinity);
        do
        {
            // Check if we've reached the target
            Vector2 currentPos = currentTile.transform.position;
            if (currentPos == target)
            {
                List<Vector2> result = new List<Vector2>();
                // While we haven't reached the end of the path, get the next step
                while (pathMap[currentPos] != Vector2.negativeInfinity)
                {
                    result.Add(currentPos);
                    currentPos = pathMap[currentPos];
                }
                // We want the path TO the target not FROM the target
                result.Reverse();
                return result;
            }

            // Add all the adjacent tiles to the queue
            QueueAdjacentTiles(queue, currentTile, target);

            // Move to the next tile in the queue
            currentTile = tileQueue.Remove();
        }
        while (tileQueue.Length() > 0);

        return null;
    }

    ///<summary>
    /// Returns a lookup table of all tiles in the Enemy's current room and all adjacent rooms.
    ///</summary>
    private Dictionary<Vector2, Tile> GetTilesInRange()
    {
        Dictionary<Vector2, Tile> tileMap = new Dictionary<Vector2, Tile>();
        List<Room> adjacentRooms = new List<Room>();
        adjacentRooms.Add(currentRoom);
        adjacentRooms.AddRange(LevelController.instance.GetAdjacentInitializedRooms(currentRoom.x, currentRoom.y));
        foreach (Room room in adjacentRooms)
        {
            foreach (KeyValuePair<Vector2, Tile> tileLookup in room.GetTileLookup())
            {
                tileMap.Add(tileLookup.Key, tileLookup.Value);
            }
        }
        return tileMap;
    }

    ///<summary>
    /// Returns an optimized path with the minimal number of points.
    ///</summary>
    private List<Vector2> OptimizePath(List<Vector2> path)
    {
        return null;
    }

    ///<summary>
    /// Adds the adjacent tiles to the queue by priority of their distance to the target.
    ///</summary>
    private void QueueAdjacentTiles(PriorityQueue<Tile, float> queue, Tile currentTile, Vector2 target)
    {
        QueueAdjacentTile(currentTile, target, Vector2.left);
        QueueAdjacentTile(currentTile, target, Vector2.right);
        QueueAdjacentTile(currentTile, target, Vector2.up);
        QueueAdjacentTile(currentTile, target, Vector2.down);
    }

    ///<summary>
    /// Adds the adjacent tile to the queue by priority of its distance to the target.
    ///</summary>
    private void QueueAdjacentTile(Tile currentTile, Vector2 target, Vector2 direction)
    {
        float tileScale = LevelController.instance.tileScale;
        direction.Normalize();
        Vector2 adjPos = (Vector2)currentTile.transform.position + (direction * tileScale);
        float adjDist = Vector2.Distance(adjPos, target);
        Tile adjTile = tileMap.ContainsKey(adjPos) ? tileMap[adjPos] : LevelController.instance.DoorTileAt(adjPos);
        if (IsTileValid(adjTile))
        {
            pathMap.Add(adjTile, currentTile);
            tileQueue.Add(adjTile, adjDist);
        }
    }

    ///<summary>
    /// Returns true if the tile has not already been visited and it is not occupied by an obstacle.
    ///</summary>
    private bool IsTileValid(Tile tile)
    {
        return (
            tile != null &&
            !pathMap.ContainsKey(tile) &&
            !tile.IsOccupied()
        );
    }
}


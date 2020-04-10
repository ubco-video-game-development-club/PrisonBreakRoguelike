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
    [Tooltip("The distance the player must move from their previous position for the enemy path to be re-calculated.")]
    public float pathAdjustDistance = 1f;

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
    private float speed = 1f;
    private float stunTimer = 0f;
    private float idleTimer = 0f;
    private Player player;

    ///<summary>Sets this Enemy to the Stunned state for stunDuration seconds.</summary>
    public void Stun(float stunDuration)
    {
        SetState(EnemyState.Stunned);
        stunTimer = stunDuration;
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        path = new EnemyPath();
        SetState(EnemyState.Idle);
    }

    void Update()
    {
        LookForPlayer();

        // Update the enemy based on the current state
        switch (state)
        {
            case EnemyState.Patrol: Patrol(); break;
            case EnemyState.Idle: Idle(); break;
            case EnemyState.Attack: Attack(); break;
            case EnemyState.Stunned: Stunned(); break;
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

    ///<summary>Sets the state of the enemy and runs any code that should happen on the given state change.</summary>
    private void SetState(EnemyState enemyState)
    {
        state = enemyState;
        switch (state)
        {
            case EnemyState.Patrol:
            {
                GeneratePatrolPath();
                speed = patrolSpeed;
                break;
            }
            case EnemyState.Idle:
            {
                StartIdleTimer();
                break;
            }
            case EnemyState.Attack:
            {
                GenerateAttackPath();
                speed = attackSpeed;
                break;
            }
        }
    }

    ///<summary>Moves the enemy along the current path. Returns true if the enemy reached the end of the path.</summary>
    private bool MoveAlongPath()
    {
        bool endReached = false;

        // Get the current path target
        Vector3 target = path.Current();

        // Move the enemy towards the target based on their current speed
        transform.position = Vector2.MoveTowards(transform.position, target, speed * Time.deltaTime);

        // Update the current direction
        direction = (target - transform.position).normalized;

        // If the enemy has reached the target, move to the next target along the path
        float distToTarget = Vector2.Distance(transform.position, target);
        if (distToTarget < 0.1f)
        {
            endReached = path.TargetNext();
        }

        return endReached;
    }

    ///<summary>Checks to see if the player has been detected by the enemy's vision or hearing.</summary>
    private void LookForPlayer()
    {
        // Don't make any detection checks if the enemy is stunned
        if (state == EnemyState.Stunned)
        {
            return;
        }

        // Check if the enemy heard the player based on the hearing radius
        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
        bool heardPlayer = distToPlayer < hearingRadius;

        // Check if the enemy saw the player based on the frontal vision angle and distance
        Vector2 dirToPlayer = player.transform.position - transform.position;
        float angleToPlayer = Vector2.Angle(dirToPlayer, direction);
        bool sawPlayer = angleToPlayer < visionArcAngle / 2 && distToPlayer < visionDistance;

        // If the player was detected, set the state to attack
        if (heardPlayer || sawPlayer)
        {
            state = EnemyState.Attack;
        }
    }

    ///<summary>Generates a path to a random nearby tile position and sets it as the current path.</summary>
    private void GeneratePatrolPath()
    {
        // Get all tiles within maxPatrolDistance of the enemy's position
        Vector2Int gridPos = LevelController.WorldToTilePosition(transform.position);
        List<Tile> tiles = LevelController.instance.GetTileRange(gridPos, maxPatrolDistance);

        // Try generating paths to nearby tiles until we find one with a valid path
        bool foundPath = false;
        while (!foundPath)
        {
            // Get a random tile from the list and return its position
            Vector2 randPos = tiles[Random.Range(0, tiles.Count)].transform.position;

            // Generate a new path to the random tile position and set it as the current path
            foundPath = path.GeneratePath(transform.position, randPos);
        }
    }

    ///<summary>Sets the idle timer to a random value between min and max idle duration.</summary>
    private void StartIdleTimer()
    {
        idleTimer = Random.Range(minIdleDuration, maxIdleDuration);
    }

    ///<summary>Generates a path to the current player position and sets it as the current path.</summary>
    private void GenerateAttackPath()
    {
        // Generate a new path to the player position and set it as the current path
        bool foundPath = path.GeneratePath(transform.position, player.transform.position);

        // If we failed to find the player, just go back to patrolling
        if (!foundPath)
        {
            SetState(EnemyState.Patrol);
        }
    }

    ///<summary>Update function for the Attack state.</summary>
    private void Attack()
    {
        // Set state to patrol if the player is out of follow range
        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distToPlayer > followDistance)
        {
            SetState(EnemyState.Patrol);
        }

        // Adjust path if the player has moved too far from their previous position
        float distToPrev = Vector2.Distance(path.End(), player.transform.position);
        if (distToPrev > pathAdjustDistance)
        {
            path.AdjustEndpoint(player.transform.position);
        }

        // Move along the path to the player
        bool reachedEnd = MoveAlongPath();
        if (reachedEnd)
        {
            // If for some weird reason we reach the end of the path
            // and we haven't hit the player, go back to patrolling
            SetState(EnemyState.Patrol);
        }
    }

    ///<summary>Update function for the Patrol state.</summary>
    private void Patrol()
    {
        // Move along the path to our current patrol target
        bool reachedEnd = MoveAlongPath();
        if (reachedEnd)
        {
            // Go back to idle once we reach the target
            SetState(EnemyState.Idle);
        }
    }

    ///<summary>Update function for the Idle state.</summary>
    private void Idle()
    {
        // Update the idle timer
        idleTimer -= Time.deltaTime;
        if (idleTimer <= 0)
        {
            // Go back to patrol once we finish idling
            SetState(EnemyState.Patrol);
        }
    }

    ///<summary>Update function for the Stunned state.</summary>
    private void Stunned()
    {
        // Update the stunned timer
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0)
        {
            // Go back to patrol once we stop being stunned
            SetState(EnemyState.Patrol);
        }
    }

    private class EnemyPath
    {
        public List<Vector2Int> tilePath;
        public List<Vector2> optimizedPath;
        private int targetIndex = 0;

        public EnemyPath()
        {
            tilePath = new List<Vector2Int>();
            optimizedPath = new List<Vector2>();
        }

        ///<summary>Generates a new path between the given start and end positions. Returns true if a path was found.</summary>
        public bool GeneratePath(Vector2 start, Vector2 end)
        {
            bool pathFound = CalculatePath(start, end);
            if (pathFound)
            {
                OptimizePath();
                targetIndex = 0;
            }
            return pathFound;
        }

        ///<summary>Returns the current target position along the path. Change to the next target by calling TargetNext().</summary>
        public Vector2 Current()
        {
            if (targetIndex >= optimizedPath.Count)
            {
                Debug.Log("the hell?");
            }
            return optimizedPath[targetIndex];
        }

        ///<summary>Returns the end point of the path.</summary>
        public Vector2 End()
        {
            return optimizedPath[optimizedPath.Count - 1];
        }

        ///<summary>Sets the current target to the next point on the path. Returns true if the end was reached.</summary>
        public bool TargetNext()
        {
            targetIndex++;
            return targetIndex >= optimizedPath.Count;
        }

        ///<summary>Recalculates the current path efficiently based on a slightly adjusted endpoint.</summary>
        public void AdjustEndpoint(Vector2 adjustedEnd)
        {
            // Draw a new path from the current endpoint to the new endpoint
            Vector2Int currentEnd = tilePath[tilePath.Count - 1];
            EnemyPath addon = new EnemyPath();
            addon.GeneratePath(currentEnd, adjustedEnd);

            // Add the new path to the end of our current path
            tilePath.AddRange(addon.tilePath);
            
            // Re-optimize the new extended path
            OptimizePath();
        }

        ///<summary>Calculates the shortest path from start to end. Returns a success boolean for whether a path was found.</summary>
        private bool CalculatePath(Vector2 start, Vector2 end)
        {
            tilePath = new List<Vector2Int>();

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

                // Add adjacent tile positions to the queue and pathmap
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
                            // Add this tile position to the pathmap
                            pathMap.Add(adj, currentTilePos);
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
            // While current index is less than path length - 1
            while (currentIndex < tilePath.Count - 1)
            {
                Vector2 currentPoint = tilePath[currentIndex];
                Vector2 lastKeyPoint = tilePath[lastKeyIndex];

                // Raycast from current point to last key point
                Vector2 dir = (lastKeyPoint - currentPoint).normalized;
                float dist = Vector2.Distance(currentPoint, lastKeyPoint);
                RaycastHit2D hit = Physics2D.Raycast(currentPoint, dir, dist);

                // If the current point is not visible from last key point
                if (hit.collider != null)
                {
                    // Set the previous index as the new last key point
                    lastKeyIndex = currentIndex - 1;

                    // Add that point to our optimized path
                    optimizedPath.Add(tilePath[lastKeyIndex]);
                }

                // Move to the next point
                currentIndex++;
            }
            
            // Add the final point of the path
            optimizedPath.Add(tilePath[tilePath.Count - 1]);
        }
    }
}


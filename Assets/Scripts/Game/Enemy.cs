using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private enum EnemyState
    {
        Patrol = 0,
        Attack = 1
    }

    public float visionDistance = 1f;
    public float visionArcAngle = 0f;
    public float followDistance = 1f;
    public float hearingRadius = 1f;
    public float attackSpeed = 1f;
    public float patrolSpeed = 1f;

    private Vector2 tempPos;
    private Vector3 target;
    private Vector3 previousLocation;
    private bool isStunned = false;
    private float stunTimer = 0f;
    private EnemyState state;
    private Player player;
    private Vector2 currentDirection;
    private Room currentRoom;
    private Tile currentTile;
    private List<Tile> movePath;
    private Dictionary<Vector2, Tile> tileMap;
    private Dictionary<Tile, Tile> pathMap;
    private PriorityQueue<Tile, float> tileQueue;

    ///<summary>Set this Enemy to the Stunned state for stunDuration seconds.</summary>
    public void Stun(float stunDuration)
    {
        isStunned = true;
        stunTimer = stunDuration;
    }

    public Tile GetCurrentTile()
    {
        Vector2Int gridPos = currentRoom.ToGridPosition(transform.position);
        Vector2 tilePos = LevelController.RoundToTilePosition(transform.position);
        return currentRoom.TileAt(gridPos.x, gridPos.y) ?? LevelController.instance.DoorTileAt(tilePos);
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    void Start()
    {
        state = EnemyState.Patrol;
        //pick location to move to upon spawn
        target = NewTargetLocation();
        currentRoom = LevelController.instance.GetNearestRoom(transform.position);
        currentTile = GetCurrentTile();
    }

    void Update()
    {
        UpdateStunnedState();
        UpdateTileState();
    }

    void FixedUpdate()
    {
        if (isStunned)
        {
            return;
        }

        LookForPlayer();

        if (state == EnemyState.Patrol)
        {
            Patrol();
        }
        else if (state == EnemyState.Attack)
        {
            Attack();
        }
    }
    
    void OnTriggerEnter2D(Collider2D collider)
    {
        Room room;
        if (collider.TryGetComponent<Room>(out room))
        {
            currentRoom = room;
        }
    }

    void OnDrawGizmos()
    {
        if (state == EnemyState.Attack)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, tempPos);
        }
    }

    private void LookForPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
        bool heardPlayer = distToPlayer < hearingRadius;

        bool sawPlayer = false;
        float currentAngle = Vector2.Angle(currentDirection, Vector2.right) * Mathf.Deg2Rad;
        for (float angle = -visionArcAngle; angle < visionArcAngle; angle += 0.1f)
        {
            float totalAngle = currentAngle + angle;
            Vector2 dir = new Vector2(Mathf.Cos(totalAngle), Mathf.Sin(totalAngle));
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, visionDistance);
            if (hit.transform.gameObject == player.gameObject)
            {
                sawPlayer = true;
                break;
            }
        }

        if (heardPlayer || sawPlayer)
        {
            state = EnemyState.Attack;
        }
    }

    private void Patrol()
    {
        //switch direction if stuck  (location stays constant)
        if (previousLocation == transform.position)
        {
            target = NewTargetLocation();
        }

        //if location has been reached, choose new target
        if (transform.position == target)
        {
            target = NewTargetLocation();
        }

        // record previous location
        previousLocation = transform.position;

        float speed = patrolSpeed * Time.deltaTime;

        //move towards target
        transform.position = Vector3.MoveTowards(transform.position, target, speed);
    }

    private void Attack()
    {
        List<Vector2> movePath = new List<Vector2>();
        // if the player is out of range, set state back to patrol and return

        // check if the player has moved 
        Tile playerTile = player.GetCurrentTile();
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

    private Vector3 NewTargetLocation()
    {
        //random distance guard will move within radius of 10
        Vector2 offset = Random.insideUnitCircle * 10; 

        //insideUnitCircle returns point relative to world point, so add this value to current enemy location
        Vector3 newTarget = new Vector3(offset.x, offset.y, 0) + gameObject.transform.position;

        currentDirection = newTarget - transform.position;
        currentDirection.Normalize();

        return newTarget;
    }

    //switch direction on collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Player p;
        if (collision.collider.TryGetComponent<Player>(out p))
        {
            p.Die();
        }
        else
        {
            target = NewTargetLocation();
        }
    }

    private void UpdateStunnedState()
    {
        stunTimer -= Time.deltaTime;
        if (stunTimer <= 0)
        {
            isStunned = false;
        }
    }

    private void UpdateTileState()
    {
        Tile updatedCurrentTile = GetCurrentTile();
        if (updatedCurrentTile == null || currentTile == null)
        {
            return;
        }

        if (updatedCurrentTile != currentTile)
        {
            currentTile.occupant = null;
            updatedCurrentTile.occupant = gameObject;
            currentTile = updatedCurrentTile;
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
    private void MoveTowardsTarget(Vector2 target)
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


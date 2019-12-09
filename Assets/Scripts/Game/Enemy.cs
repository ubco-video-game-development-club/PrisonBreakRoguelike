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
        Tile playerTile = player.GetCurrentTile();
        if (movePath == null || movePath.Count == 0)
        {
            movePath = GetPathToTarget(playerTile);
            if (movePath == null || movePath.Count == 0)
            {
                state = EnemyState.Patrol;
                return;
            }
        }
        else
        {
            Vector2 playerPos = playerTile.transform.position;
            Vector2 endPos = movePath[movePath.Count - 1].transform.position;
            if (Vector2.Distance(playerPos, endPos) > 1)
            {
                movePath = GetPathToTarget(playerTile);
            }
        }

        Vector2 nextPos = movePath[0].transform.position;
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
        target = NewTargetLocation();
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

    private List<Tile> GetPathToTarget(Tile target)
    {
        // A dictionary of all searchable tiles by position
        tileMap = GetTileMap();
        // A dictionary of visited tiles. Each tile stores
        // the previous step taken to get to it so that we
        // can get the path back to our initial position.
        pathMap = new Dictionary<Tile, Tile>();
        // A priority queue which prioritizes checking tiles 
        // that are closer to the target
        tileQueue = new PriorityQueue<Tile, float>();

        // Start from the current tile the enemy is on
        Tile currentTile = GetCurrentTile();
        pathMap.Add(currentTile, null);
        do
        {
            // Check if we've reached the target
            Vector2 currentPos = currentTile.transform.position;
            Vector2 targetPos = target.transform.position;
            if (currentPos == targetPos)
            {
                List<Tile> result = new List<Tile>();
                // Get the path from the target
                while (pathMap[currentTile] != null)
                {
                    result.Add(currentTile);
                    currentTile = pathMap[currentTile];
                }
                // We want the path TO the target
                result.Reverse();
                if (result.Count == 0)
                {
                    Debug.Log("that's a count of 0");
                }
                return result;
            }

            // Add all the adjacent tiles to the queue
            QueueAdjacentTile(currentTile, targetPos, Vector2.left);
            QueueAdjacentTile(currentTile, targetPos, Vector2.right);
            QueueAdjacentTile(currentTile, targetPos, Vector2.up);
            QueueAdjacentTile(currentTile, targetPos, Vector2.down);

            // Move to the next tile in the queue
            currentTile = tileQueue.Remove();
        }
        while (tileQueue.Length() > 0);

        return null;
    }

    private void QueueAdjacentTile(Tile currentTile, Vector2 targetPos, Vector2 direction)
    {
        float tileScale = LevelController.instance.tileScale;
        direction.Normalize();
        Vector2 adjPos = (Vector2)currentTile.transform.position + (direction * tileScale);
        float adjDist = Vector2.Distance(adjPos, targetPos);
        Tile adjTile = tileMap.ContainsKey(adjPos) ? tileMap[adjPos] : LevelController.instance.DoorTileAt(adjPos);
        if (CanMoveTo(adjTile))
        {
            pathMap.Add(adjTile, currentTile);
            tileQueue.Add(adjTile, adjDist);
        }
    }

    private bool CanMoveTo(Tile tile)
    {
        return (
            tile != null &&
            !pathMap.ContainsKey(tile) &&
            !tile.IsOccupied()
        );
    }

    private Dictionary<Vector2, Tile> GetTileMap()
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
}


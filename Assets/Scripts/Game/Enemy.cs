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

    private Vector3 target;
    private Vector3 previousLocation;
    private bool isStunned = false;
    private float stunTimer = 0f;
    private EnemyState state;
    private Player player;
    private Vector2 currentDirection;
    private Room currentRoom;

    public void Stun(float stunDuration)
    {
        isStunned = true;
        stunTimer = stunDuration;
    }

    public Tile GetCurrentTile()
    {
        Vector2Int gridPos = currentRoom.ToGridPosition(transform.position);
        return currentRoom.TileAt(gridPos.x, gridPos.y);
    }

    void Start()
    {
        state = EnemyState.Patrol;
        //pick location to move to upon spawn
        target = NewTargetLocation();
    }

    void Update()
    {
        UpdateStunnedState();
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
        currentDirection = player.transform.position - transform.position;
        currentDirection.Normalize();

        float speed = attackSpeed * Time.deltaTime;

        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed);
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

    private List<Tile> GetPathToTile(Tile target)
    {
        float tileScale = LevelController.instance.tileScale;

        // TODO: add a dictionary of tiles with <tile, parent> for visited tiles?
        // Remove the visited dictionary and use the above instead?
        // Loop through the queue until found or failed
        // Make a helper method to add adjacent to queue by direction: QueueAdjacentTile(Vector2 direction)
        // With that helper method, make the tileMap, queue and new visited dict private member variables

        // Get the map of available tiles
        Dictionary<Vector2, Tile> tileMap = GetTileMap();
        Dictionary<Vector2, bool> visited = new Dictionary<Vector2, bool>();

        // Create a queue of tiles to check in order based on distance to target
        PriorityQueue<Tile, float> queue = new PriorityQueue<Tile, float>();

        // Get the current tile position
        Tile startTile = GetCurrentTile();
        float startDist = Vector2.Distance(startTile.transform.position, target.transform.position);
        queue.Add(startTile, startDist);

        Tile currentTile = queue.Remove();
        visited[currentTile.transform.position] = true;

        // Add the adjacent tiles of the current position to the queue
        Vector2 leftPos = (Vector2)currentTile.transform.position + (Vector2.left * tileScale);
        float leftDist = Vector2.Distance(leftPos, target.transform.position);
        Tile leftTile = tileMap[leftPos] ?? LevelController.instance.WallTileAt(leftPos);
        if (leftTile != null && !visited[leftPos])
        {
            queue.Add(leftTile, leftDist);
        }
        
        Vector2 rightPos = (Vector2)currentTile.transform.position + (Vector2.right * tileScale);
        float rightDist = Vector2.Distance(rightPos, target.transform.position);
        Tile rightTile = tileMap[rightPos] ?? LevelController.instance.WallTileAt(rightPos);
        if (rightTile != null && !visited[rightPos])
        {
            queue.Add(rightTile, rightDist);
        }
        
        Vector2 topPos = (Vector2)currentTile.transform.position + (Vector2.up * tileScale);
        float topDist = Vector2.Distance(topPos, target.transform.position);
        Tile topTile = tileMap[topPos] ?? LevelController.instance.WallTileAt(topPos);
        if (topTile != null && !visited[topPos])
        {
            queue.Add(topTile, topDist);
        }
        
        Vector2 bottomPos = (Vector2)currentTile.transform.position + (Vector2.down * tileScale);
        float bottomDist = Vector2.Distance(bottomPos, target.transform.position);
        Tile bottomTile = tileMap[bottomPos] ?? LevelController.instance.WallTileAt(bottomPos);
        if (bottomTile != null && !visited[bottomPos])
        {
            queue.Add(bottomTile, bottomDist);
        }

        // After checking the next tile, add its adjacent tiles to the queue

        // Once we reach the target, backtrack to get the path
        return null;
    }

    private Dictionary<Vector2, Tile> GetTileMap()
    {
        Dictionary<Vector2, Tile> tileMap = new Dictionary<Vector2, Tile>();
        List<Room> adjacentRooms = new List<Room>();
        Room currentRoom = LevelController.instance.RoomAt(transform.position);
        adjacentRooms.Add(currentRoom);
        adjacentRooms.AddRange(LevelController.instance.GetAdjacentRooms(currentRoom));
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


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

    public void Stun(float stunDuration)
    {
        isStunned = true;
        stunTimer = stunDuration;
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
}


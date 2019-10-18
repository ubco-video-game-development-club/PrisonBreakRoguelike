using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Vector3 target;
    private Vector3 previousLocation;
    private bool isStunned;
    private float stunTimer = 0f;

    public void Stun(float stunDuration)
    {
        isStunned = true;
        stunTimer = stunDuration;
    }

    void Start()
    {
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

        Patrol();
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

        //move towards target
        transform.position = Vector3.MoveTowards(transform.position, target, 2 * Time.deltaTime);
    }

    private Vector3 NewTargetLocation()
    {
        //random distance guard will move within radius of 10
        Vector2 offset = Random.insideUnitCircle * 10; 

        //insideUnitCircle returns point relative to world point, so add this value to current enemy location
        Vector3 newTarget = new Vector3(offset.x, offset.y, 0) + gameObject.transform.position;
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


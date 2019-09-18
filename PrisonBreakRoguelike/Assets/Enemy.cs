using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Vector3 target;
    Vector3 previousLocation;

    void Start()
    {
        //pick location to move to upon spawn
        target = NewTargetLocation();
    }

    void FixedUpdate()
    {  
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
        var offset = Random.insideUnitCircle * 10; 

        //insideUnitCircle returns point relative to world point, so add this value to current enemy location
        Vector3 newTarget = new Vector3(offset.x, offset.y, 0) + gameObject.transform.position;
        return newTarget;
    }

    //switch direction on collision
    private void OnCollisionEnter2D(Collision2D collision)
    {
        target = NewTargetLocation();
    }
}


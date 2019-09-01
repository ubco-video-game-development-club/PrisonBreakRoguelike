using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 1f;

    void Update()
    {
        float dx = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float dy = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        transform.position += new Vector3(dx, dy, 0f);
    }
}

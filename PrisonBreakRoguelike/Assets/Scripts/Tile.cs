using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [HideInInspector]
    public int x, y;
    public bool isDoor;
    public bool isWall;
    public Vector3 pos;

    /// <summary>
    /// The GameObject that is occupying this Tile.
    /// </summary>
    [HideInInspector]
    public GameObject occupant;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

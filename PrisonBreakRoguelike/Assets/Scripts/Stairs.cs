using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs  : MonoBehaviour 
{
    public Color color;    
    void Start()
    {
        GetComponent<SpriteRenderer>().color = color;
    }

    public void  SetAsTarget(bool isTarget)
    {
         GetComponent<LevelController>().NewLevel(); //Untested
    }
}





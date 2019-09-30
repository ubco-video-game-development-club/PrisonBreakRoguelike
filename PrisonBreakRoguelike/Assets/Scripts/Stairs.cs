using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs  : Item

{
    public Color stairColor;
    void Start()
    {
        GetComponent<SpriteRenderer>().color = stairColor;
    }

     new public void  SetAsTarget(bool isTarget)
    {
         LevelController.NewLevel(); //needs to be a static method
    }
}





using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour  // Stairs shares functionality w/ other items, but only one is generated per level
{
    public void  SetAsTarget(bool isTarget)
    {
         GetComponent<LevelController>().NewLevel(); //Untested
    }
}





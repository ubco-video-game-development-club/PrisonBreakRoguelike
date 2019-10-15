using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs  : MonoBehaviour 
{
    public void  SetAsTarget(bool isTarget)
    {
         GetComponent<LevelController>().NewLevel(); //Untested
    }
}





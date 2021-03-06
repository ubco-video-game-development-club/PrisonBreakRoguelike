﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stairs : MonoBehaviour  // Stairs shares functionality w/ other items, but only one is generated per level
{
    private LevelController levelController;

    void Start()
    {
        levelController = LevelController.instance;
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag == "Player")
        {
            levelController.LoadNextLevel();
        }
    }
}






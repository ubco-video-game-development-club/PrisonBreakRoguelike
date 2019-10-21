using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    //[HideInInspector]
    public int x, y;
    public bool isDoor;
    public bool isWall;
    public Vector3 pos;
    //[HideInInspector]

        
    public Sprite[] sprites; 

    public float[] weights; // Values must be entered in the same order as the sprites they will corrispond to

    public GameObject occupant;  
    /// <summary>
    /// The GameObject that is occupying this Tile.
    /// </summary>

    void Awake()
    {
        GetComponent<SpriteRenderer>().sprite = ChooseSprite();        
    }

    private Sprite ChooseSprite()
    {
        float rand = Random.Range(0f, 1f);

        float last = 0f;

        for (int i = 0; i < sprites.Length; i++)
        {
            if(InRange(rand, last, weights[i]))
                return sprites[i];
            
            else
                last += weights[i]; 
        }
        return sprites[0]; // Return first sprite as a default

      
    }
    private bool InRange(float num, float bot, float top) // checks if num is in range [bot, top), helper method for ChooseSprite()
    {
        return (num >= bot && num < top);
    }
}

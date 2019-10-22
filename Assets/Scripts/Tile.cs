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

    private SpriteRenderer sr;
    

        
    public Sprite[] sprites; 

    public float[] weights; // Values must be entered in the same order as the sprites they will corrispond to

    [HideInInspector]

    /// <summary>
    /// The GameObject that is occupying this Tile.
    /// </summary>

    public GameObject occupant;  

  
    public Sprite ChooseSprite()
    {
        float rand = Random.Range(0f, Sum(weights));

        float last = 0f;

        float next = 0f;

        for (int i = 0; i < sprites.Length; i++)
        {
            next += weights[i];

            if(InRange(rand, last, next))
                return sprites[i];
            
            else
                last += weights[i]; 
        }
        Debug.Log("Error: Cannot choose a sprite");
        return sprites[0];
    }
    private bool InRange(float num, float bot, float top) // checks if num is in range [bot, top), helper method for ChooseSprite()
    {
        return (num >= bot && num < top);
    }

    private float Sum(float[] arr)
    {
        float sum = 0;
        for(int i = 0; i < arr.Length; i++)
            sum += arr[i];

        return sum;
    }
}

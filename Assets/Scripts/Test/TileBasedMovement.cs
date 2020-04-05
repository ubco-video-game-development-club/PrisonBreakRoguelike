using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBasedMovement : MonoBehaviour
{
    // public float speed;

    // /// <summary>
    // /// The Tile you are moving toward.
    // /// </summary>
    // private Tile targetTile;
    // /// <summary>
    // /// The Tile you are currently on.
    // /// </summary>
    // private Tile currentTile;
    // /// <summary>
    // /// The Room you are currently in.
    // /// </summary>
    // private Room currentRoom;
    // private Vector2 startPos, endPos;
    // private int currentDirX, currentDirY;
    // private float totalDist;
    // private float distTravelled;
    // private bool init = false;

    // void Update()
    // {
    //     if (!init)
    //     {
    //         currentRoom = GameObject.Find("Room").GetComponent<Room>();
    //         currentTile = currentRoom.TileAt(0, 0);
    //         transform.position = currentTile.transform.position;
    //         init = true;
    //     }

    //     int inputX = GetSign(Input.GetAxis("Horizontal"));
    //     int inputY = GetSign(Input.GetAxis("Vertical"));
    //     Debug.Log("Inputs: " + inputX + ", " + inputY);
    //     if (inputX != 0 || inputY != 0)
    //     {
    //         Vector2 dir = GetDirectionFromInput(inputX, inputY);
    //         Tile newTarget = currentRoom.TileAt(currentTile.x + (int)dir.x, currentTile.y + (int)dir.y);
    //         if (newTarget != null && newTarget != targetTile)
    //         {
    //             startPos = transform.position;
    //             endPos = newTarget.transform.position;
    //             if (Vector2.Distance(startPos, endPos) < Mathf.Epsilon)
    //             {
    //                 return;
    //             }
    //             Debug.Log("Changed Target");
    //             targetTile = newTarget;
    //             currentDirX = (int)dir.x;
    //             currentDirY = (int)dir.y;
    //             distTravelled = 0;
    //             totalDist = Vector2.Distance(startPos, endPos);
    //         }
    //     }
    //     if (targetTile != null)
    //     {
    //         Debug.Log("Moved");
    //         distTravelled += speed * Time.deltaTime;
    //         float interpolant = distTravelled / totalDist;
    //         transform.position = Vector2.Lerp(startPos, endPos, interpolant);
    //         if (Vector2.Distance(transform.position, endPos) < Vector2.Distance(transform.position, startPos))
    //         {
    //             currentTile = targetTile;
    //             currentDirX = 0;
    //             currentDirY = 0;
    //             targetTile = null;
    //         }
    //     }
    // }

    // private Vector2 GetDirectionFromInput(int inputX, int inputY)
    // {
    //     int quadrantX = Mathf.Clamp(GetSign(transform.position.x - currentTile.transform.position.x), -1, 1);
    //     int quadrantY = Mathf.Clamp(GetSign(transform.position.y - currentTile.transform.position.y), -1, 1);
    //     quadrantX = inputX != 0 ? quadrantX : currentDirX;
    //     quadrantY = inputY != 0 ? quadrantY : currentDirY;
    //     Debug.Log("Quadrants: " + quadrantX + ", " + quadrantY);
    //     int dirX = Mathf.Clamp(inputX + quadrantX, -1, 1);
    //     int dirY = Mathf.Clamp(inputY + quadrantY, -1, 1);
    //     Debug.Log("Dirs: " + dirX + ", " + dirY);
    //     return new Vector2(dirX, dirY);
    // }

    // private int GetSign(float f)
    // {
    //     if (f > 0f) 
    //     {
    //         return 1;
    //     }
    //     else if (f < 0f)
    //     {
    //         return -1;
    //     }
    //     else
    //     {
    //         return 0;
    //     }
    // }
}

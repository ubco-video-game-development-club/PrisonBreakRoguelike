using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public Color idleColor;
    public Color highlightColor;

    public Tile occupiedTile;

    void Start()
    {
        GetComponent<SpriteRenderer>().color = idleColor;
    }

    public void ClearOccupiedTile()
    {
        // broken?
        if (occupiedTile != null)
        {
            occupiedTile.occupant = null;
        }
    }

    public void SetOccupiedTile(Tile tile)
    {
        occupiedTile = tile;
    }

    public void SetAsTarget(bool isTarget)
    {
        Color currentColor = isTarget ? highlightColor : idleColor;
        GetComponent<SpriteRenderer>().color = currentColor;
    }
}

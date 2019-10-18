using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public Color idleColor;
    public Color highlightColor;

    void Start()
    {
        GetComponent<SpriteRenderer>().color = idleColor;
    }

    public void SetAsTarget(bool isTarget)
    {
        Color currentColor = isTarget ? highlightColor : idleColor;
        GetComponent<SpriteRenderer>().color = currentColor;
    }
}

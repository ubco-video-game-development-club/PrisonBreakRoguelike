using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public ItemType itemType;
    public TextMesh hoverTextPrefab;
    private Vector2 hoverTextOffset;

    private TextMesh hoverText;

    void Start()
    {
        Vector3 textPos = transform.position + (Vector3)hoverTextOffset;
        hoverText = Instantiate(hoverTextPrefab, textPos, Quaternion.identity, transform) as TextMesh;
    }

    public void SetAsTarget(bool isTarget)
    {
        if (isTarget == false)
        {
            hoverText.text = "";
            return;
        }

        switch (itemType)
        {
            case ItemType.ENERGY_DRINK:
            {
                hoverText.text = "Take Energy Drink";
                break;
            }
            case ItemType.STUN_GUN:
            {
                hoverText.text = "Take Stun Gun";
                break;
            }
            case ItemType.BOMB:
            {
                hoverText.text = "Take Bomb";
                break;
            }
        }
    }
}

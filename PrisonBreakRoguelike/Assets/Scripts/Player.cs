using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed = 1f;
    public float pickupDistance = 1f;

    [HideInInspector]
    public Room currentRoom;
    private Item currentItemTarget;

    void Update()
    {
        UpdateMovement();
        UpdateInteraction();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {   
        Room room;
        if (collision.gameObject.TryGetComponent<Room>(out room))
        {
            Debug.Log("Yeet");
            currentRoom = room;
        }
    }

    private void UpdateMovement()
    {
        float dx = 0f, dy = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            dx--;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            dx++;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            dy++;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            dy--;
        }
        Vector2 vel = new Vector2(dx, dy);
        vel.Normalize();
        vel *= speed;
        transform.position += new Vector3(vel.x, vel.y, 0f) * Time.deltaTime;
    }

    private void UpdateInteraction()
    {
        Item item = GetClosestItem();
        if (item == null || item.Equals(currentItemTarget))
        {
            return;
        }

        if (currentItemTarget != null)
        {
            currentItemTarget.SetAsTarget(false);
        }

        Debug.Log("Ya yeet");
        item.SetAsTarget(true);
        currentItemTarget = item;

        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (item.itemType)
            {
                case ItemType.ENERGY_DRINK:
                {
                    break;
                }
                case ItemType.STUN_GUN:
                {
                    break;
                }
                case ItemType.BOMB:
                {
                    break;
                }
            }
            Destroy(item.gameObject);
        }
    }

    private Item GetClosestItem()
    {
        if (currentRoom == null)
        {
            return null;
        }

        List<Item> nearbyItems = currentRoom.GetItems();
        if (nearbyItems == null || nearbyItems.Count == 0)
        {
            return null;
        }
        
        Item item = null;
        float shortestDist = float.MaxValue;

        foreach (Item nearbyItem in nearbyItems)
        {
            float dist = Vector2.Distance(transform.position, nearbyItem.transform.position);
            if (dist < pickupDistance && dist < shortestDist)
            {
                item = nearbyItem;
                shortestDist = dist;
            }
        }

        return item;
    }
}

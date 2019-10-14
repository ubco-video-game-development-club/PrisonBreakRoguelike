using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 1f;
    [Header("Interaction")]
    public float pickupDistance = 1f;
    [Header("Energy Drink")]
    public float energyDrinkCooldown = 1f;
    public float energyDrinkDuration = 1f;
    public float energyDrinkSpeed = 1f;
    [Header("Stun Gun")]
    public float stunGunCooldown = 1f;
    public float stunGunDuration = 1f;
    public LayerMask stunGunLayer;
    public float stunGunRange = 1f;
    public string stunGunTargetTag = "Enemy";
    [Header("Bomb")]
    public float bombCooldown = 1f;
    public LayerMask bombLayer;
    public float bombRadius = 1f;
    public string bombTargetTag = "Deco";

    [HideInInspector]
    public Room currentRoom;
    private Item currentItemTarget;
    private int energyDrinkCount;
    private float energyDrinkDurationTimer;
    private float energyDrinkCooldownTimer;
    private bool energyDrinkActive;
    private int stunGunCount;
    private float stunGunCooldownTimer;
    private int bombCount;
    private float bombCooldownTimer;

    void Update()
    {
        UpdateMovement();
        UpdateInteraction();
        UpdateEnergyDrink();
        UpdateStunGun();
        UpdateBomb();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {   
        Room room;
        if (collider.TryGetComponent<Room>(out room))
        {
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
        vel *= energyDrinkActive ? energyDrinkSpeed : speed;
        transform.position += new Vector3(vel.x, vel.y, 0f) * Time.deltaTime;
    }

    private void UpdateInteraction()
    {
        Item item = GetClosestItem();
        if (item == null)
        {
            if (currentItemTarget != null)
            {
                currentItemTarget.SetAsTarget(false);
                currentItemTarget = null;
            }

            return;
        }

        if (!item.Equals(currentItemTarget))
        {
            if (currentItemTarget != null)
            {
                currentItemTarget.SetAsTarget(false);
            }

            item.SetAsTarget(true);
            currentItemTarget = item;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (item.itemType)
            {
                case ItemType.ENERGY_DRINK:
                {
                    energyDrinkCount++;
                    break;
                }
                case ItemType.STUN_GUN:
                {
                    stunGunCount++;
                    break;
                }
                case ItemType.BOMB:
                {
                    bombCount++;
                    break;
                }
            }
            Destroy(item.gameObject);
            currentItemTarget = null;
        }
    }

    private void UpdateEnergyDrink()
    {
        energyDrinkCooldownTimer -= Time.deltaTime;
        energyDrinkDurationTimer -= Time.deltaTime;

        if (energyDrinkDurationTimer <= 0)
        {
            energyDrinkActive = false;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) 
            && energyDrinkCooldownTimer <= 0 
            && energyDrinkCount > 0)
        {
            energyDrinkCooldownTimer = energyDrinkCooldown;
            energyDrinkDurationTimer = energyDrinkDuration;
            energyDrinkCount--;
            energyDrinkActive = true;
        }
    }

    private void UpdateStunGun()
    {
        stunGunCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Alpha2) 
            && stunGunCooldownTimer <= 0
            && stunGunCount > 0)
        {
            stunGunCooldownTimer = stunGunCooldown;
            stunGunCount--;
            
            Vector2 mouseDirection = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, mouseDirection, stunGunRange, stunGunLayer);
            foreach (RaycastHit2D hit in hits)
            {
                GameObject target = hit.collider.gameObject;
                if (target.CompareTag("Wall"))
                {
                    break;
                }
                if (target.CompareTag(stunGunTargetTag))
                {
                    Enemy enemy;
                    if (target.TryGetComponent<Enemy>(out enemy))
                    {
                        enemy.Stun(stunGunDuration);
                    }
                }
            }
        }
    }

    private void UpdateBomb()
    {
        bombCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Alpha3) 
            && bombCooldownTimer <= 0
            && bombCount > 0)
        {
            bombCooldownTimer = bombCooldown;
            bombCount--;

            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, bombRadius, bombLayer);
            foreach (Collider2D target in targets)
            {
                if (target.CompareTag(bombTargetTag))
                {
                    Destroy(target);
                }
            }
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

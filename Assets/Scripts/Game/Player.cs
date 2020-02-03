using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Vision")]
    public LayerMask visionLayer;
    public int visionRayCount = 100;
    public float visionRayDistance = 100f;
    [Header("Movement")]
    public float speed = 1f;
    [Header("Interaction")]
    public float pickupDistance = 1f;
    [Header("Energy Drink")]
    public ItemDisplay energyDrinkDisplay;
    public float energyDrinkCooldown = 1f;
    public float energyDrinkDuration = 1f;
    public float energyDrinkSpeed = 1f;
    [Header("Stun Gun")]
    public ItemDisplay stunGunDisplay;
    public float stunGunCooldown = 1f;
    public float stunGunDuration = 1f;
    public LayerMask stunGunLayer;
    public float stunGunRange = 1f;
    public string stunGunTargetTag = "Enemy";
    [Header("Bomb")]
    public ItemDisplay bombDisplay;
    public float bombCooldown = 1f;
    public LayerMask bombLayer;
    public float bombRadius = 1f;
    public string bombTargetTag = "Deco";

    private bool alive;
    private Room currentRoom;
    private Item currentItemTarget;
    private int energyDrinkCount;
    private float energyDrinkDurationTimer;
    private float energyDrinkCooldownTimer;
    private bool energyDrinkActive;
    private int stunGunCount;
    private float stunGunCooldownTimer;
    private int bombCount;
    private float bombCooldownTimer;
    private Animator animator;

    public void Die()
    {
        alive = false;
        HUDController.instance.ShowDeathScreen();
    }

    public Tile GetCurrentTile()
    {
        Vector2Int gridPos = currentRoom.ToGridPosition(transform.position);
        Vector2 tilePos = LevelController.RoundToTilePosition(transform.position);
        return currentRoom.TileAt(gridPos.x, gridPos.y) ?? LevelController.instance.DoorTileAt(tilePos);
    }

    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    public void Spawn(Room spawn, Vector2 spawnPos)
    {
        transform.position = spawnPos;
        currentRoom = spawn;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        alive = true;
    }

    void Update()
    {
        if (!alive)
        {
            return;
        }

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
            Debug.Log("New room is: " + currentRoom.name);
        }
    }

    private void UpdateVision()
    {
        // Light2D light2D = GetComponent<Light2D>();
        // light2D.shapePath = new Vector3[visionRayCount];
        // const float PI = Mathf.PI;
        // for (float angle = 0; angle < 2*PI; angle += 2*PI/visionRayCount)
        // {
        //     Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        //     RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, visionRayDistance, visionLayer);
        //     light2D.shapePath
        // }
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
    
        bool isMoving = (dy != 0 || dx != 0);
         
        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("X_Vel", dx);
        animator.SetFloat("Y_Vel", dy);
    
        Vector3 vel = new Vector3(dx, dy, 0);
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
            item.ClearOccupiedTile();
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

        float progress = energyDrinkCooldownTimer / energyDrinkCooldown;
        energyDrinkDisplay.SetProgress(progress);
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
            
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mouseDirection = mousePosition - (Vector2)transform.position;
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

        float progress = stunGunCooldownTimer / stunGunCooldown;
        stunGunDisplay.SetProgress(progress);
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
                    Destroy(target.gameObject);
                }
            }
        }

        float progress = bombCooldownTimer / bombCooldown;
        bombDisplay.SetProgress(progress);
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





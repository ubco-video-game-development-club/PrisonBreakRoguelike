using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("The movement speed of the player in units per second.")]
    public float speed = 1f;

    [Header("Interaction Settings")]
    [Tooltip("The max distance the player can be from an item to pick it up.")]
    public float pickupDistance = 1f;

    [Header("Energy Drink Settings")]
    [Tooltip("The UI element for the energy drink cooldown.")]
    public ItemDisplay energyDrinkDisplay;
    [Tooltip("The cooldown time between energy drink uses.")]
    public float energyDrinkCooldown = 1f;
    [Tooltip("The duration of the energy drink movement speed buff.")]
    public float energyDrinkDuration = 1f;
    [Tooltip("The movement speed of the player while the energy drink is active.")]
    public float energyDrinkSpeed = 1f;

    [Header("Stun Gun Settings")]
    [Tooltip("The UI element for the stun gun cooldown.")]
    public ItemDisplay stunGunDisplay;
    [Tooltip("The cooldown time between stun gun uses.")]
    public float stunGunCooldown = 1f;
    [Tooltip("The duration of the stun effect.")]
    public float stunGunDuration = 1f;
    [Tooltip("The physics layer the stun gun raycasts on.")]
    public LayerMask stunGunLayer;
    [Tooltip("The range of the stun gun raycast.")]
    public float stunGunRange = 1f;
    [Tooltip("The GameObjecct tag of allowed stun gun targets.")]
    public string stunGunTargetTag = "Enemy";

    [Header("Bomb Settings")]
    [Tooltip("The UI element for the bomb cooldown.")]
    public ItemDisplay bombDisplay;
    [Tooltip("The cooldown time between bomb uses.")]
    public float bombCooldown = 1f;
    [Tooltip("The physics layer the bomb checks on.")]
    public LayerMask bombLayer;
    [Tooltip("The radius of the stun gun physics check.")]
    public float bombRadius = 1f;
    [Tooltip("The GameObjecct tag of allowed bomb targets.")]
    public string bombTargetTag = "Deco";

    private bool alive = false;
    private int energyDrinkCount;
    private float energyDrinkDurationTimer;
    private float energyDrinkCooldownTimer;
    private bool energyDrinkActive;
    private int stunGunCount;
    private float stunGunCooldownTimer;
    private int bombCount;
    private float bombCooldownTimer;
    private Animator animator;

    ///<summary>Sets the player to the dead state and loads the lose scene.</summary>
    public void Die()
    {
        alive = false;
        SceneManager.LoadSceneAsync("Lose");
    }

    ///<summary>Sets the player to the alive state.</summary>
    public void Spawn()
    {
        alive = true;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
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

    ///<summary>Check for keyboard input, update player animations and update player position.</summary>
    private void UpdateMovement()
    {
        // Add up input directions such that if the user inputs
        // two opposite directions they will cancel out.
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
        
        // Update player animations
        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("X_Vel", dx);
        animator.SetFloat("Y_Vel", dy);
    
        // Update player position
        Vector3 vel = new Vector3(dx, dy, 0);
        vel.Normalize();
        vel *= energyDrinkActive ? energyDrinkSpeed : speed;
        transform.position += new Vector3(vel.x, vel.y, 0f) * Time.deltaTime;
    }

    ///<summary>Checks for user input on nearby items.</summary>
    private void UpdateInteraction()
    {
        Item item = GetClosestItem();
        if (item == null)
        {
            return;
        }

        // If the item is within pickup distance and the user presses E
        float dist = Vector2.Distance(item.transform.position, transform.position);
        if (dist < pickupDistance && Input.GetKeyDown(KeyCode.E))
        {
            // Pick up the item
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

            // Remove the picked-up Item from the grid
            Vector2Int gridPos = LevelController.WorldToTilePosition(item.transform.position);
            LevelController.instance.tiles[gridPos.x, gridPos.y].occupant = null;
            Destroy(item.gameObject);
        }
    }
    
    ///<summary>Updates the Energy Drink item usage every frame, including cooldown timer, checking input and updating the UI.</summary>
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

    ///<summary>Updates the Stun Gun item usage every frame, including cooldown timer, checking input and updating the UI.</summary>
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

    ///<summary>Updates the Bomb item usage every frame, including cooldown timer, checking input and updating the UI.</summary>
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

    ///<summary>Returns the closest item to the player or null if no items were found.</summary>
    private Item GetClosestItem()
    {
        // Get all tiles within (pickup distance + 1) of the player's position
        Vector2Int gridPos = LevelController.WorldToTilePosition(transform.position);
        int maxDist = Mathf.CeilToInt(pickupDistance) + 1;
        List<Tile> tiles = LevelController.instance.GetTileRange(gridPos, maxDist);
        
        // Get the closest item in the range of tiles
        Item closestItem = null;
        float shortestDist = float.MaxValue;
        foreach (Tile tile in tiles)
        {
            TileObject obj = tile.occupant;

            // Check if there is an item on this tile
            Item item;
            if (obj != null && obj.TryGetComponent<Item>(out item))
            {
                float dist = Vector2.Distance(gridPos, item.transform.position);
                if (dist < shortestDist)
                {
                    closestItem = item;
                    shortestDist = dist;
                }
            }
        }

        return closestItem;
    }
}





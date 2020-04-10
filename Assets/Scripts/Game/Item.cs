using UnityEngine;

public class Item : MonoBehaviour
{
    [Tooltip("The type of item.")]
    public ItemType itemType;
    
    [Tooltip("The sprite used when the item is not highlighted.")]
    public Sprite standardSprite;

    [Tooltip("The sprite used when the item is highlighted.")]
    public Sprite highlightedSprite;

    private Player player;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = standardSprite;
    }

    void Update()
    {
        // Update the sprite based on the distance to the player
        Vector2 dist = transform.position - player.transform.position;
        bool inRange = dist.sqrMagnitude < player.pickupDistance;
        spriteRenderer.sprite = inRange ? highlightedSprite : standardSprite;
    }
}

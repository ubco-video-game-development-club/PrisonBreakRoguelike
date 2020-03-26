using UnityEngine;

public class Item : MonoBehaviour
{
    [Tooltip("The type of item.")]
    public ItemType itemType;
    
    [Tooltip("The sprite used when the item is not highlighted.")]
    public Sprite standardSprite;

    [Tooltip("The sprite used when the item is highlighted.")]
    public Sprite highlightedSprite;

    [Tooltip("The distance from the player at which the item will become highlighted.")]
    public float highlightDistance = 1f;

    private Player player;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = standardSprite;
    }

    void Update()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        }

        // Update the sprite based on the distance to the player
        Vector2 dist = transform.position - player.transform.position;
        bool inRange = dist.sqrMagnitude < highlightDistance;
        spriteRenderer.sprite = inRange ? highlightedSprite : standardSprite;
    }
}

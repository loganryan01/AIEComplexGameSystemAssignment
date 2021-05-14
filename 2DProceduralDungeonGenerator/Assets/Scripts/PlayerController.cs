/*-----------------------------------------
    File Name: PlayerController.cs
    Purpose: Control the player's character
    Author: Logan Ryan
    Modified: 14/05/2021
-------------------------------------------
    Copyright 2021 Logan Ryan
-----------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;                             // Player's rigidbody
    public Sprite swordSprite;                  // Player's Image when they obtained the sword
    public Text coinText;                       // Text to display player's coins
    public Image keyImage;                      // Image to display that the player has a key
    public Image swordImage;                    // Image to display that the player has the sword

    [HideInInspector]
    public bool hasKey = false;                 // Does the player have a key
    [HideInInspector]
    public bool swordEquipped = false;          // Does the player have a sword equipped
    [HideInInspector]
    public bool invincible = false;             // Is the player invincible

    private int coins = 0;                      // How many coins the player has
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        //===== PLAYER MOVEMENT =====
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontalInput * 3, verticalInput * 3);

        rb.velocity = new Vector2(move.x, move.y);

        // If the player is moving left, flip the player's sprite so it is facing left
        if (rb.velocity.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

        // If the player's health is below 0, go to the game over screen
        if (GetComponent<PlayerStats>().Health <= 0)
        {
            SceneManager.LoadScene("GameOver");
        }

        // Display the player's coins
        coinText.text = "Coins: " + coins.ToString();

        // If the player has a key, display the key sprite
        if (hasKey)
        {
            keyImage.enabled = true;
        }

        // If the player has a sword, display the sword sprite
        if (swordEquipped)
        {
            swordImage.enabled = true;
        }
    }

    // Handle collision detection with the player and the dungeon
    private List<TileBase> CollisionDetection(Vector3Int worldToCellPosition, Grid parentGrid, Tilemap tilemap, out Vector3Int[] tilePositions)
    {
        List<TileBase> tiles = new List<TileBase>();
        tilePositions = new Vector3Int[4];

        // Check left of the player
        worldToCellPosition.x -= 1;

        tilePositions[0] = new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0);
        TileBase tile = tilemap.GetTile(tilePositions[0]);
        tiles.Add(tile);

        worldToCellPosition = parentGrid.WorldToCell(transform.position);

        // Check right of the player
        worldToCellPosition.x += 1;

        tilePositions[1] = new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0);
        tile = tilemap.GetTile(tilePositions[1]);
        tiles.Add(tile);

        worldToCellPosition = parentGrid.WorldToCell(transform.position);

        // Check below the player
        worldToCellPosition.y -= 1;

        tilePositions[2] = new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0);
        tile = tilemap.GetTile(tilePositions[2]);
        tiles.Add(tile);

        worldToCellPosition = parentGrid.WorldToCell(transform.position);

        // Check above the player
        worldToCellPosition.y += 1;

        tilePositions[3] = new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0);
        tile = tilemap.GetTile(tilePositions[3]);
        tiles.Add(tile);

        // Return the tiles that the player touches
        return tiles;
    }

    // Resolve the collision between the player and the dungeon
    private void ResolveCollision(List<TileBase> tileList, Tilemap tilemap, Vector3Int[] tilePositions)
    {
        // Go through the tiles that the player touches...
        for (int i = 0; i < tileList.Count; i++)
        {
            if (tileList[i] != null)
            {
                // If it touches the chest that has the sword in it...
                if (tileList[i].name == "SwordChest")
                {
                    // ... equip the player with the sword and remove the tile from the dungeon.
                    gameObject.GetComponent<SpriteRenderer>().sprite = swordSprite;
                    tilemap.SetTile(tilePositions[i], null);
                    swordEquipped = true;
                }
                // If it touches a coin...
                else if (tileList[i].name == "Coin")
                {
                    // ... increase the player's coin count by 1 and remove the tile from the dungeon.
                    tilemap.SetTile(tilePositions[i], null);
                    coins++;
                }
                // If it touches the exit...
                else if (tileList[i].name == "Exit")
                {
                    // ... and has the sword equipped to it...
                    if (swordEquipped)
                    {
                        // ... load the next scene
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                    }
                }
                // If it touches a healing poition...
                else if (tileList[i].name == "HealingPotion")
                {
                    // ... increase the player's health by 1 and remove the tile from the dungeon.
                    tilemap.SetTile(tilePositions[i], null);
                    GetComponent<PlayerStats>().Heal(1);
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the player touches the a room or corridor...
        if (collision.gameObject.GetComponent<Tilemap>())
        {
            // Get the grid of the parent object
            Grid parentGrid = collision.gameObject.GetComponentInParent<Grid>();

            // Get the cell position of the player's position
            Vector3Int worldToCellPosition = parentGrid.WorldToCell(transform.position);
            Vector3Int[] tilePositions;

            // Get the tilemap of the object that the player has collided with
            Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();

            // Get the tiles that the player has collided with
            List<TileBase> tileList = CollisionDetection(worldToCellPosition, parentGrid, tilemap, out tilePositions);

            // If the player has collided with a tile...
            if (tileList.Count > 0)
            {
                // ... resolve the collision of the tiles
                ResolveCollision(tileList, tilemap, tilePositions);
            }
        }

        // If the player collides with a projectile while they are not invincible...
        if (collision.gameObject.CompareTag("Projectile") && !invincible)
        {
            // ... player takes 1 damage and becomes invincible
            GetComponent<PlayerStats>().TakeDamage(1);
            StartCoroutine(Invincible());
        }

        // If the player collides with a door while they have a key
        if (collision.gameObject.CompareTag("Door") && hasKey)
        {
            // Destroy the door, remove the key from the player and turn off the image of the key
            Destroy(collision.gameObject);
            hasKey = false;
            keyImage.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player enters the room...
        if (collision.CompareTag("Room"))
        {
            // ... get the enemies that are in the dungeon
            GameObject[] enemiesInDungeon = GameObject.FindGameObjectsWithTag("Enemy");

            // Then find all the enemies that are in the room
            foreach (var enemy in enemiesInDungeon)
            {
                Rect enemyRect = new Rect(enemy.transform.position, enemy.transform.localScale);
                Rect roomRect = new Rect(collision.gameObject.transform.position, collision.gameObject.GetComponent<BoxCollider2D>().size);

                if (enemyRect.Overlaps(roomRect))
                {
                    enemy.GetComponent<EnemyController>().seePlayer = true;
                }
            }
        }
    }

    // Make the player invincible for 4 seconds
    public IEnumerator Invincible()
    {
        invincible = true;
        GetComponent<SpriteRenderer>().enabled = false;
        
        yield return new WaitForSeconds(0.5f);

        GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.5f);

        GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(0.5f);

        GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.5f);

        GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(0.5f);

        GetComponent<SpriteRenderer>().enabled = true;

        yield return new WaitForSeconds(0.5f);

        GetComponent<SpriteRenderer>().enabled = false;

        yield return new WaitForSeconds(0.5f);

        GetComponent<SpriteRenderer>().enabled = true;
        invincible = false;
    }
}

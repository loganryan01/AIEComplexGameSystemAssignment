/*-----------------------------------------
    File Name: PlayerController.cs
    Purpose: Control the player's character
    Author: Logan Ryan
    Modified: 12/05/2021
-------------------------------------------
    Copyright 2021 Logan Ryan
-----------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;                             // Player's rigidbody
    //public DungeonGenerator dungeonGenerator;   // The dungeon generator script
    public Sprite swordSprite;                  // Player's Image when they obtained the sword
    public Text coinText;                       // Text to display player's coins

    [HideInInspector]
    public bool swordEquipped = false;
    [HideInInspector]
    public bool invincible = false;

    private int coins = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        //gameObject.transform.position = new Vector2(dungeonGenerator.columns / 2, dungeonGenerator.rows / 2);
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontalInput * 3, verticalInput * 3);

        rb.velocity = new Vector2(move.x, move.y);

        if (rb.velocity.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }

        if (GetComponent<PlayerStats>().Health <= 0)
        {
            Debug.Log("Game Over");
        }

        coinText.text = "Coins: " + coins.ToString();
    }

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

        return tiles;
    }

    private void ResolveCollision(List<TileBase> tileList, Tilemap tilemap, Vector3Int[] tilePositions)
    {
        for (int i = 0; i < tileList.Count; i++)
        {
            if (tileList[i] != null)
            {
                if (tileList[i].name == "SwordChest")
                {
                    gameObject.GetComponent<SpriteRenderer>().sprite = swordSprite;
                    tilemap.SetTile(tilePositions[i], null);
                    swordEquipped = true;
                }
                else if (tileList[i].name == "Coin")
                {
                    tilemap.SetTile(tilePositions[i], null);
                    coins++;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Tilemap>())
        {
            Grid parentGrid = collision.gameObject.GetComponentInParent<Grid>();

            Vector3Int worldToCellPosition = parentGrid.WorldToCell(transform.position);
            Vector3Int[] tilePositions;

            Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();

            List<TileBase> tileList = CollisionDetection(worldToCellPosition, parentGrid, tilemap, out tilePositions);

            if (tileList.Count > 0)
            {
                ResolveCollision(tileList, tilemap, tilePositions);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Room"))
        {
            GameObject[] enemiesInDungeon = GameObject.FindGameObjectsWithTag("Enemy");
            List<GameObject> enemiesInRoom = new List<GameObject>();

            foreach (var enemy in enemiesInDungeon)
            {
                Rect enemyRect = new Rect(enemy.transform.position, enemy.transform.localScale);
                Rect roomRect = new Rect(collision.gameObject.transform.position, collision.gameObject.transform.GetComponent<BoxCollider2D>().size);

                if (enemyRect.Overlaps(roomRect))
                {
                    enemy.GetComponent<EnemyController>().seePlayer = true;
                }
            }
        }
    }

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

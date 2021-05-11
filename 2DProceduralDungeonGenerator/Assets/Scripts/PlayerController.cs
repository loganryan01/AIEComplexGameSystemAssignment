/*-----------------------------------------
    File Name: PlayerController.cs
    Purpose: Control the player's character
    Author: Logan Ryan
    Modified: 11/05/2021
-------------------------------------------
    Copyright 2021 Logan Ryan
-----------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;                             // Player's rigidbody
    public DungeonGenerator dungeonGenerator;   // The dungeon generator script
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        gameObject.transform.position = new Vector2(dungeonGenerator.columns / 2, dungeonGenerator.rows / 2);
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector2 move = new Vector2(horizontalInput * 3, verticalInput * 3);

        rb.velocity = new Vector2(move.x, move.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Tilemap>())
        {
            List<TileBase> tileList = new List<TileBase>();
            
            Grid parentGrid = collision.gameObject.GetComponentInParent<Grid>();

            Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();
            
            Vector3Int worldToCellPosition = parentGrid.WorldToCell(transform.position);

            // Check left of the player
            worldToCellPosition.x -= 1;

            TileBase leftTile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
            if (leftTile != null)
            {
                Debug.Log(leftTile.name);
            }
            tileList.Add(leftTile);

            worldToCellPosition = parentGrid.WorldToCell(transform.position);

            // Check right of the player
            worldToCellPosition.x += 1;

            TileBase rightTile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
            if (rightTile != null)
            {
                Debug.Log(rightTile.name);
            }
            tileList.Add(rightTile);

            worldToCellPosition = parentGrid.WorldToCell(transform.position);

            // Check below the player
            worldToCellPosition.y -= 1;

            TileBase bottomTile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
            if (bottomTile != null)
            {
                Debug.Log(bottomTile.name);
            }
            tileList.Add(bottomTile);

            worldToCellPosition = parentGrid.WorldToCell(transform.position);

            // Check above the player
            worldToCellPosition.y += 1;

            TileBase topTile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
            if (topTile != null)
            {
                Debug.Log(topTile.name);
            }
            
            tileList.Add(topTile);
        }
    }
}

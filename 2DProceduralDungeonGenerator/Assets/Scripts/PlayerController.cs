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

    private List<TileBase> CollisionDetection(GameObject gameObject)
    {
        List<TileBase> tiles = new List<TileBase>();

        Grid parentGrid = gameObject.GetComponentInParent<Grid>();

        Tilemap tilemap = gameObject.GetComponent<Tilemap>();

        Vector3Int worldToCellPosition = parentGrid.WorldToCell(transform.position);

        // Check left of the player
        worldToCellPosition.x -= 1;

        TileBase tile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
        //if (tile != null)
        //{
        //    Debug.Log(tile.name);
        //}
        tiles.Add(tile);

        worldToCellPosition = parentGrid.WorldToCell(transform.position);

        // Check right of the player
        worldToCellPosition.x += 1;

        tile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
        //if (tile != null)
        //{
        //    Debug.Log(tile.name);
        //}
        tiles.Add(tile);

        worldToCellPosition = parentGrid.WorldToCell(transform.position);

        // Check below the player
        worldToCellPosition.y -= 1;

        tile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
        //if (tile != null)
        //{
        //    Debug.Log(tile.name);
        //}
        tiles.Add(tile);

        worldToCellPosition = parentGrid.WorldToCell(transform.position);

        // Check above the player
        worldToCellPosition.y += 1;

        tile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
        //if (tile != null)
        //{
        //    Debug.Log(tile.name);
        //}
        tiles.Add(tile);

        return tiles;
    }

    private void ResolveCollision(List<TileBase> tileList)
    {
        foreach (var tile in tileList)
        {
            if (tile != null)
            {
                if (tile.name == "SwordChest")
                {
                    Debug.Log("Obtained Sword");
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Tilemap>())
        {
            List<TileBase> tileList = CollisionDetection(collision.gameObject);
            
            if (tileList.Count > 0)
            {
                ResolveCollision(tileList);
            }
        }
    }
}

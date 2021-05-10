using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rb;
    public DungeonGenerator dungeonGenerator;
    
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
            Grid parentGrid = collision.gameObject.GetComponentInParent<Grid>();

            Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();
            BoundsInt tilemapBounds = tilemap.cellBounds;
            
            Vector3Int worldToCellPosition = parentGrid.WorldToCell(transform.position);
            Debug.Log(worldToCellPosition);

            TileBase tile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));

            int roomHeightHalf = tilemapBounds.yMax / 2;
            int roomWidthHalf = tilemapBounds.xMax / 2;

            if (worldToCellPosition.x < roomWidthHalf && tile == null)
            {
                worldToCellPosition.x -= 1;

                tile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
                worldToCellPosition = parentGrid.WorldToCell(transform.position);
            }
            
            if (worldToCellPosition.x > roomWidthHalf && tile == null)
            {
                worldToCellPosition.x += 1;

                tile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
                worldToCellPosition = parentGrid.WorldToCell(transform.position);
            }

            if (worldToCellPosition.y < roomHeightHalf && tile == null)
            {
                worldToCellPosition.y -= 1;

                tile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
                worldToCellPosition = parentGrid.WorldToCell(transform.position);
            }
            
            if (worldToCellPosition.y > roomHeightHalf && tile == null)
            {
                worldToCellPosition.y += 1;

                tile = tilemap.GetTile(new Vector3Int(worldToCellPosition.x, worldToCellPosition.y, 0));
                worldToCellPosition = parentGrid.WorldToCell(transform.position);
            }

            if (tile)
            {
                Debug.Log(tile);
            }
        }

        //if (collision.contactCount > 0)
        //{
        //    Vector3 hitPosition = Vector3.zero;
        //    foreach (ContactPoint2D hit in collision.contacts)
        //    {
        //        hitPosition.x = hit.point.x - 0.01f * hit.normal.x;
        //        hitPosition.y = hit.point.y - 0.01f * hit.normal.y;
        //        Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();
        //        Vector3Int cellPosition = tilemap.WorldToCell(hitPosition);
        //        TileBase tile = tilemap.GetTile(cellPosition);

        //        if (tile != null)
        //        {
        //            Debug.Log(tile.name);
        //        }
        //    }
        //}
    }
}

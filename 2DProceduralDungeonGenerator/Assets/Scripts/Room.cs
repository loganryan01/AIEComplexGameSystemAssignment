using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room 
{
    public int xPos;    // The x coordinate of the lower left tile of the room.
    public int yPos;    // The y coordinate of the lower left tile of the room.
    public int width;   // How many tiles wide the room is.
    public int height;  // How many tiles high the room is.

    public void SetupFirstRoom(BoundsInt roomBounds, int columns, int rows)
    {
        width = roomBounds.size.x;
        height = roomBounds.size.y;

        xPos = Mathf.RoundToInt(columns / 2f - width / 2f);
        yPos = Mathf.RoundToInt(rows / 2f - height / 2f);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

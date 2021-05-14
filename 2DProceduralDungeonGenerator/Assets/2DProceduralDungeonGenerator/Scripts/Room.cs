/*-------------------------------------------
    File Name: Room.cs
    Purpose: Store information about the room
    Author: Logan Ryan
    Modified: 12/05/2021
---------------------------------------------
    Copyright 2021 Logan Ryan
-------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room 
{
    public int xPos;                        // The x coordinate of the lower left tile of the room.
    public int yPos;                        // The y coordinate of the lower left tile of the room.
    public int width;                       // How many tiles wide the room is.
    public int height;                      // How many tiles high the room is.
    public Direction enteringDirection;     // The direction of the corridor that is entering this room.
    public GameObject roomTemplate;         // The template of the room

    public void SetupFirstRoom(BoundsInt roomBounds, GameObject room, int columns, int rows)
    {
        // Set the width and height of the room
        width = roomBounds.size.x;
        height = roomBounds.size.y;

        // Set the x and y coordinates so the room is roughly in the middle of the board.
        xPos = Mathf.RoundToInt(columns / 2f - width / 2f);
        yPos = Mathf.RoundToInt(rows / 2f - height / 2f);

        // Set the room template
        roomTemplate = room;
    }

    public void SetupRoom(BoundsInt roomBounds, GameObject room, int columns, int rows, Corridor corridor)
    {
        // Set the entering corridor direction.
        enteringDirection = corridor.direction;

        // Set the width and height of the room
        width = roomBounds.size.x;
        height = roomBounds.size.y;

        switch (corridor.direction)
        {
            // If the corridor entering this room is going north...
            case Direction.North:
                // The y coordinate of the room must be at the end of the corridor (since corridor leads to the bottom of the room).
                yPos = corridor.EndPositionY - 1;

                // The x coordinate can be random but the left-most possibility is no further than the width 
                // and the right-most possibility is that the end of the corridor is at the position of the room.
                xPos = Random.Range(corridor.EndPositionX, corridor.EndPositionX - (width - 3));

                // This must be clamped to ensure that the room doesn't go off the board
                xPos = Mathf.Clamp(xPos, 0, columns - width);
                break;
            case Direction.East:
                xPos = corridor.EndPositionX - 1;

                yPos = Random.Range(corridor.EndPositionY, corridor.EndPositionY - (height - 3));
                yPos = Mathf.Clamp(yPos, 0, rows - height);
                break;
            case Direction.South:
                yPos = corridor.EndPositionY - height + 1;

                xPos = Random.Range(corridor.EndPositionX, corridor.EndPositionX - (width - 3));
                xPos = Mathf.Clamp(xPos, 0, columns - width);
                break;
            case Direction.West:
                xPos = corridor.EndPositionX - width + 1;

                yPos = Random.Range(corridor.EndPositionY, corridor.EndPositionY - (height - 3));
                yPos = Mathf.Clamp(yPos, 0, rows - height);
                break;
        }

        // Set the room template
        roomTemplate = room;
    }
}

/*-----------------------------------------------
    File Name: Corridor.cs
    Purpose: Store information about the corridor
    Author: Logan Ryan
    Modified: 11/05/2021
-------------------------------------------------
    Copyright 2021 Logan Ryan
-----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enum to specify the direction is heading
public enum Direction
{
    North, East, South, West
}

public class Corridor 
{
    public int startXPos;           // The x coordinate for the start of the corridor.
    public int startYPos;           // The y coordinate for the start of the corridor.
    public int corridorLength;      // How many units long the corridor is.
    public Direction direction;     // Which direction the corridor is heading from it's room.

    // Get the end position of the corridor based on it's start position and which direction it's heading
    public int EndPositionX
    {
        get
        {
            if (direction == Direction.North || direction == Direction.South)
                return startXPos;
            if (direction == Direction.East)
                return startXPos + corridorLength;
            return startXPos - corridorLength;
        }
    }

    public int EndPositionY
    {
        get
        {
            if (direction == Direction.East || direction == Direction.West)
                return startYPos;
            if (direction == Direction.North)
                return startYPos + corridorLength;
            return startYPos - corridorLength;
        }
    }

    public void SetupCorridor(Room room, IntRange length, int columns, int rows, bool firstCorridor)
    {
        // Set a random direction (a random index from 0 to 3, cast to Direction)
        direction = (Direction)Random.Range(0, 4);

        // Find the direction opposite to the one entering the room this corridor is leaving from.
        // Cast the previous corridor's direction to an int between 0 and 3 and add 2 (a number between 2 and 5).
        // Find the remainder when dividing by 4 (if 2 then 2, if 3 then 3, if 4 then 0, if 5 then 1).
        // Cast this number back to a direction.
        // Overall effect is if the direction was South then that is 2, becomes 4, remainder is 0, which is north.
        Direction oppositeDirection = (Direction)(((int)room.enteringDirection + 2) % 4);

        // If this is not the first corridor and the randomly selected direction is opposite to the previous corridor's direction...
        if (!firstCorridor && direction == oppositeDirection)
        {
            // Rotate the direction 90 degrees clockwise (North becomes East, East becomes South, etc)
            // This is a more broken down version of the opposite direction operation above but instead of adding 2 we're adding 1
            // This means instead of rotating 180 (the opposite direction) we're rotating 90.
            int directionInt = (int)direction;
            directionInt++;
            directionInt %= 4;
            direction = (Direction)directionInt;
        }

        // Set a random length
        corridorLength = length.Random;

        // Create a cap for how long the length can be (This will be changed based on the direction and positions).
        int maxLength = length.m_Max;

        switch (direction)
        {
            // If the chosen direction is North (up)...
            case Direction.North:
                //Debug.Log("Minimum xPos: " + (room.xPos + 1));
                //Debug.Log("Maximum xPos: " + (room.xPos + room.width - 1));

                // ... the starting position in the x axis can be random but within the width of the room.
                startXPos = Random.Range(room.xPos + 1, room.xPos + room.width - 2);

                // The starting position in the y axis must be the top of the room.
                startYPos = room.yPos + room.height - 1;

                // The maximum length the corridor can be is the height of the board (rows) but from the top of the room (y pos + height)
                maxLength = rows - startYPos - room.height;
                break;
            case Direction.East:
                startXPos = room.xPos + room.width - 1;
                startYPos = Random.Range(room.yPos + 1, room.yPos + room.height - 2);
                maxLength = columns - startXPos - room.width;
                break;
            case Direction.South:
                startXPos = Random.Range(room.xPos + 1, room.xPos + room.width - 2);
                startYPos = room.yPos + 1;
                maxLength = startYPos - room.height;
                break;
            case Direction.West:
                startXPos = room.xPos + 1;
                startYPos = Random.Range(room.yPos + 1, room.yPos + room.height - 2);
                maxLength = startXPos - room.width;
                break;
        }

        // We clamp the lengths of the corridor to make sure it doesn't go off the board.
        corridorLength = Mathf.Clamp(corridorLength, 1, maxLength);
    }
}

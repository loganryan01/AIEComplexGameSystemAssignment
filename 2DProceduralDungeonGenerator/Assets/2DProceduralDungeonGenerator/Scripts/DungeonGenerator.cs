/*----------------------------------------------
    File Name: DungeonGenerator.cs
    Purpose: Generate dungeons for the developer
    Author: Logan Ryan
    Modified: 14/05/2021
------------------------------------------------
    Copyright 2021 Logan Ryan
----------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LabeledArrayAttribute : PropertyAttribute
{
    public readonly string[] names;
    public LabeledArrayAttribute(string[] names) { this.names = names; }
    public LabeledArrayAttribute(Type enumType) { names = Enum.GetNames(enumType); }
}

[Serializable]
public class RoomTemplate
{
    public GameObject roomPrefab;
    public int numberOfRooms;
}

#if (UNITY_EDITOR)
public class DungeonGenerator : MonoBehaviour
{
    // Enum to store the type of tiles in the dungeon
    public enum TileType
    {
        NULL = 0,
        ROOM = 1,
        CORRIDOR = 2,
    }

    [Header("Grid size")]
    [Tooltip("Number of columns in the grid")]
    public int columns = 100;                               // Number of columns in the grid
    [Tooltip("Number of rows in the grid")]
    public int rows = 100;                                  // Number of rows in the grid

    [Header("Corridor controls")]
    [Tooltip("Number of columns in the grid")]
    public IntRange corridorLength = new IntRange(3, 10);   // Lengths of the corridor

    [Header("Room controls")]
    [Tooltip("Number of rooms in the dungeon")]
    public int numberOfRooms;                               // Number of rooms in the dungeon
    [Tooltip("Starting room template")]
    public GameObject startingRoom;                         // Starting room template
    [Tooltip("Finishing room template")]
    public GameObject finishingRoom;                        // Finishing room template
    [Tooltip("Other rooms to be created for the dungeon")]
    [LabeledArray(new string[] {"Room 1", "Room 2", "Room 3" })]
    public RoomTemplate[] roomPrefabs;                      // Room templates

    private bool success = false;                           // Dungeon was successfully created
    private GameObject boardHolder;                         // GameObject that holds the dungeon
    private Room[] rooms;                                   // Array to store the rooms in the dungeon
    private Corridor[] corridors;                           // Array to store the corridors in the dungeon
    private TileType[][] tiles;                             // Jagged tile array that stores the type of tiles in the dungeon

    private StreamReader reader;                            // Reader for text files
    private string text;                                    // Line of text from the text file
    List<TileBase> tilePalette = new List<TileBase>();      // Tiles to use in the dungeon

    // Generate a dungeon
    public void GenerateDungeon()
    {
        // Locate the board holder of the dungeon
        if (boardHolder == null)
        {
            boardHolder = GameObject.Find("BoardHolder");
        }

        // Destroy the original board holder, whenever a new dungeon is being generated
        if (boardHolder != null)
        {
            DestroyImmediate(boardHolder);
        }

        // Create new board holder object
        boardHolder = new GameObject("BoardHolder");
        boardHolder.AddComponent<Grid>();

        // Get Tile Palette
        GetTilePalette();

        // Create a dungeon until it is successful
        do
        {
            SetupTilesArray();

            CreateRoomsAndCorridors();

            SetTilesValuesForRooms();
            SetTilesValuesForCorridors();
        } while (!success);

        // Once a dungeon is successfully created, instantiate the rooms, corridors and outer walls
        InstantiateRooms();
        InstantiateCorridors();
        InstantiateOuterWalls();
    }

    // Setup the tiles jagged array
    void SetupTilesArray()
    {
        // Set the tiles jagged array to the correct width.
        tiles = new TileType[columns][];

        // Go through all the tile arrays...
        for (int i = 0; i < tiles.Length; i++)
        {
            // ... and set each tile array is the correct width
            tiles[i] = new TileType[rows];
        }
    }

    // Create rooms and corridors for the dungeon
    void CreateRoomsAndCorridors()
    {
        // Assume that the dungeon creation is a success
        success = true;

        int size = 0;
        int[] roomsAvailable = new int[roomPrefabs.Length];

        // Check how many rooms are available to use in the dungeon creation
        for (int i = 0; i < roomPrefabs.Length; i++)
        {
            if (roomPrefabs[i].numberOfRooms > 0)
            {
                size += roomPrefabs[i].numberOfRooms;
            }

            roomsAvailable[i] = roomPrefabs[i].numberOfRooms;
        }

        // If the user has a starting room and finishing room ...
        if (startingRoom != null && finishingRoom != null)
        {
            // ... Increase the size by 2
            size += 2;
        }
        // If the user has a starting room or finishing room ...
        else if (startingRoom != null || finishingRoom != null)
        {
            // ... Increase the size by 1
            size += 1;
        }

        // If the number of available rooms is less than the number of rooms to be in the dungeon...
        if (size < numberOfRooms)
        {
            // Create the rooms array with a size of the number of room prefabs
            rooms = new Room[size];

            // There should be one less corridor than there is rooms
            corridors = new Corridor[size - 1];
        }
        else
        {
            // Create the rooms array with a size of the number of rooms
            rooms = new Room[numberOfRooms];

            // There should be one less corridor than there is rooms
            corridors = new Corridor[numberOfRooms - 1];
        }

        // Create the first room and corridor
        rooms[0] = new Room();
        corridors[0] = new Corridor();

        // Choose a random room prefab
        int roomIndex = UnityEngine.Random.Range(0, roomPrefabs.Length);

        // If the chosen room prefab has is not available...
        while (roomsAvailable[roomIndex] == 0)
        {
            // ... Choose a different room
            roomIndex = UnityEngine.Random.Range(0, roomPrefabs.Length);
        }

        // Get the bounds of the chosen room
        BoundsInt roomBounds = roomPrefabs[roomIndex].roomPrefab.GetComponentInChildren<Tilemap>().cellBounds;

        // Setup the first room
        if (startingRoom != null)
        {
            // If the user has a starting room, then use that rooms bounds.
            roomBounds = startingRoom.GetComponentInChildren<Tilemap>().cellBounds;

            rooms[0].SetupFirstRoom(roomBounds, startingRoom, columns, rows);
        }
        else
        {
            // If not then use the chosen room prefab from earlier.
            rooms[0].SetupFirstRoom(roomBounds, roomPrefabs[roomIndex].roomPrefab, columns, rows);
            roomsAvailable[roomIndex]--;
        }

        // Setup the first corridor
        corridors[0].SetupCorridor(rooms[0], corridorLength, columns, rows, true);

        for (int i = 1; i < rooms.Length; i++)
        {
            // Create a room
            rooms[i] = new Room();

            // Setup the room based on the previous corridor.

            // Choose a random room prefab
            roomIndex = UnityEngine.Random.Range(0, roomPrefabs.Length);

            int numberOfAvailableRooms = 0;

            // Check if there are available rooms
            for (int j = 0; j < roomPrefabs.Length; j++)
            {
                if (roomsAvailable[j] > 0)
                {
                    numberOfAvailableRooms++;
                }
            }

            // If there are available rooms...
            if (numberOfAvailableRooms > 0)
            {
                // Check if the chosen room prefab is available...
                while (roomsAvailable[roomIndex] == 0)
                {
                    // If not then choose another room prefab
                    roomIndex = UnityEngine.Random.Range(0, roomPrefabs.Length);
                }
            }

            // If we are at the last room or there are no more available rooms
            if (i == rooms.Length - 1 || numberOfAvailableRooms == 0)
            {
                // If the user has a finishing room template, then use that rooms bounds.
                if (finishingRoom != null)
                {
                    roomBounds = finishingRoom.GetComponentInChildren<Tilemap>().cellBounds;

                    rooms[i].SetupRoom(roomBounds, finishingRoom, columns, rows, corridors[i - 1]);
                }
                else
                {
                    // If not then use the chosen room prefab from earlier.
                    roomBounds = roomPrefabs[roomIndex].roomPrefab.GetComponentInChildren<Tilemap>().cellBounds;

                    rooms[i].SetupRoom(roomBounds, roomPrefabs[roomIndex].roomPrefab, columns, rows, corridors[i - 1]);
                }

                break;
            }
            else
            {
                // If not, then continue creating rooms
                roomBounds = roomPrefabs[roomIndex].roomPrefab.GetComponentInChildren<Tilemap>().cellBounds;
                rooms[i].SetupRoom(roomBounds, roomPrefabs[roomIndex].roomPrefab, columns, rows, corridors[i - 1]);
                roomsAvailable[roomIndex]--;
            }

            // If we haven't reached the end of the corridors array...
            if (i < corridors.Length)
            {
                // ... create a corridor
                corridors[i] = new Corridor();

                // Setup the corridor 
                corridors[i].SetupCorridor(rooms[i], corridorLength, columns, rows, false);
            }
        }

        // Check if rooms are overlapping
        CheckRoom2RoomCollision();

        // Check if corridors overlap
        CheckCorridor2CorridorCollision();
    }

    // Check if 2 rooms overlap each other
    void CheckRoom2RoomCollision()
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            for (int j = i + 1; j < rooms.Length; j++)
            {
                Rect room1Rect = new Rect(rooms[i].xPos, rooms[i].yPos, rooms[i].width, rooms[i].height);
                Rect room2Rect = new Rect(rooms[j].xPos, rooms[j].yPos, rooms[j].width, rooms[j].height);

                if (room1Rect.Overlaps(room2Rect))
                {
                    // If they do then the creation of the dungeon has been unsuccessful
                    success = false;
                }
            }
        }
    }

    // Check if 2 corridors overlap each other
    void CheckCorridor2CorridorCollision()
    {
        for (int i = 0; i < corridors.Length; i++)
        {
            for (int j = i + 1; j < corridors.Length; j++)
            {
                Rect corridor1Rect = new Rect();

                // Depending on the direction of the corridor...
                switch (corridors[i].direction)
                {
                    // Get the corridors rectangle
                    case Direction.North:
                        corridor1Rect = new Rect(corridors[i].startXPos, corridors[i].startYPos + 1, 3, corridors[i].corridorLength - 1);
                        break;
                    case Direction.East:
                        corridor1Rect = new Rect(corridors[i].startXPos + 1, corridors[i].startYPos, corridors[i].corridorLength - 1, 3);
                        break;
                    case Direction.South:
                        corridor1Rect = new Rect(corridors[i].startXPos, corridors[i].startYPos - 1, 3, corridors[i].corridorLength - 1);
                        break;
                    case Direction.West:
                        corridor1Rect = new Rect(corridors[i].startXPos - 1, corridors[i].startYPos, corridors[i].corridorLength - 1, 3);
                        break;
                }

                Rect corridor2Rect = new Rect();

                switch (corridors[j].direction)
                {
                    case Direction.North:
                        corridor2Rect = new Rect(corridors[j].startXPos, corridors[j].startYPos + 1, 3, corridors[j].corridorLength - 1);
                        break;
                    case Direction.East:
                        corridor2Rect = new Rect(corridors[j].startXPos + 1, corridors[j].startYPos, corridors[j].corridorLength - 1, 3);
                        break;
                    case Direction.South:
                        corridor2Rect = new Rect(corridors[j].startXPos, corridors[j].startYPos - 1, 3, corridors[j].corridorLength - 1);
                        break;
                    case Direction.West:
                        corridor2Rect = new Rect(corridors[j].startXPos - 1, corridors[j].startYPos, corridors[j].corridorLength - 1, 3);
                        break;
                }

                if (corridor1Rect.Overlaps(corridor2Rect))
                {
                    // If they do then the creation of the dungeon has been unsuccessful
                    success = false;
                }
            }
        }
    }

    // Set the values for the rooms in the tile jagged array
    void SetTilesValuesForRooms()
    {
        // Go through all the rooms...
        for (int i = 0; i < rooms.Length; i++)
        {
            Room currentRoom = rooms[i];

            // ... and for each room go through it's width.
            for (int j = 0; j < currentRoom.width; j++)
            {
                int xCoord = currentRoom.xPos + j;

                // For each horizontal tile, go up vertically through the room's height.
                for (int k = 0; k < currentRoom.height; k++)
                {
                    int yCoord = currentRoom.yPos + k;

                    if (xCoord < 0 || xCoord >= tiles.Length ||
                        yCoord < 0 || yCoord >= tiles.Length)
                    {
                        // If the room goes outside the tile array,
                        // Then the dungeon creation was unsuccessful
                        success = false;
                        return;
                    }
                    // The coordinates in the jagged array are based on the room's position and it's width and height
                    tiles[xCoord][yCoord] = TileType.ROOM;
                }
            }
        }
    }

    // Set the values for the corridors in the tile jagged array
    void SetTilesValuesForCorridors()
    {
        // Go through every corridor...
        for (int i = 0; i < corridors.Length; i++)
        {
            Corridor currentCorridor = corridors[i];

            // and go through it's length
            for (int j = 0; j < currentCorridor.corridorLength; j++)
            {
                // Start the coordinates at the start of the corridor
                int xCoord = currentCorridor.startXPos;
                int yCoord = currentCorridor.startYPos;

                // Depending on the direction, add or subtract from the appropriate
                // coordinate based on how far through the length the loop is.
                switch (currentCorridor.direction)
                {
                    case Direction.North:
                        yCoord += j;
                        break;
                    case Direction.East:
                        xCoord += j;
                        break;
                    case Direction.South:
                        yCoord -= j;
                        break;
                    case Direction.West:
                        xCoord -= j;
                        break;
                }

                if (xCoord < 0 || xCoord >= tiles.Length ||
                    yCoord < 0 || yCoord >= tiles.Length)
                {
                    success = false;
                    return;
                }

                // Set the tile at these coordinates to corridor
                tiles[xCoord][yCoord] = TileType.CORRIDOR;
            }
        }
    }

    // Create the room game objects
    void InstantiateRooms()
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            GameObject room = Instantiate(rooms[i].roomTemplate, new Vector3(rooms[i].xPos, rooms[i].yPos, 0), rooms[i].roomTemplate.transform.rotation, boardHolder.transform); ;
            Tilemap roomWallsTilemap = room.GetComponentsInChildren<Tilemap>()[1];

            if (i < rooms.Length - 1)
            {
                switch (corridors[i].direction)
                {
                    // Get the starting position of the corridors
                    // and remove the wall at the start of the corridor
                    // If the user has a door tile, then set the door tile at the start of the corridor
                    case Direction.North:
                        Vector3Int northCorridorPosition = new Vector3Int(corridors[i].startXPos, corridors[i].startYPos, 0);
                        Vector3Int northCorridorCellPosition = roomWallsTilemap.WorldToCell(northCorridorPosition);
                        int northCorridorEntranceXPosition = northCorridorCellPosition.x + 1;
                        roomWallsTilemap.SetTile(new Vector3Int(northCorridorEntranceXPosition, northCorridorCellPosition.y, 0), tilePalette[18]);
                        break;
                    case Direction.East:
                        Vector3Int eastCorridorPosition = new Vector3Int(corridors[i].startXPos, corridors[i].startYPos, 0);
                        Vector3Int eastCorridorCellPosition = roomWallsTilemap.WorldToCell(eastCorridorPosition);
                        int eastCorridorEntranceYPosition = eastCorridorCellPosition.y + 1;
                        roomWallsTilemap.SetTile(new Vector3Int(eastCorridorCellPosition.x, eastCorridorEntranceYPosition, 0), tilePalette[18]);
                        break;
                    case Direction.South:
                        Vector3Int southCorridorPosition = new Vector3Int(corridors[i].startXPos, corridors[i].startYPos - 1, 0);
                        Vector3Int southCorridorCellPosition = roomWallsTilemap.WorldToCell(southCorridorPosition);
                        int southCorridorEntranceXPosition = southCorridorCellPosition.x + 1;
                        roomWallsTilemap.SetTile(new Vector3Int(southCorridorEntranceXPosition, southCorridorCellPosition.y, 0), tilePalette[18]);
                        break;
                    case Direction.West:
                        Vector3Int westCorridorPosition = new Vector3Int(corridors[i].startXPos - 1, corridors[i].startYPos, 0);
                        Vector3Int westCorridorCellPosition = roomWallsTilemap.WorldToCell(westCorridorPosition);
                        int westCorridorEntranceYPosition = westCorridorCellPosition.y + 1;
                        roomWallsTilemap.SetTile(new Vector3Int(westCorridorCellPosition.x, westCorridorEntranceYPosition, 0), tilePalette[18]);
                        break;
                }
            }

            if (i > 0)
            {
                // Get the end position of the corridors
                // and remove the wall at the start of the corridor
                // If the user has a door tile, then set the door tile at the end of the corridor
                switch (rooms[i].enteringDirection)
                {
                    case Direction.North:
                        Vector3Int northCorridorPosition = new Vector3Int(corridors[i - 1].EndPositionX, corridors[i - 1].EndPositionY - 1, 0);
                        Vector3Int northCorridorCellPosition = roomWallsTilemap.WorldToCell(northCorridorPosition);
                        int northCorridorExitXPosition = northCorridorCellPosition.x + 1;
                        roomWallsTilemap.SetTile(new Vector3Int(northCorridorExitXPosition, northCorridorCellPosition.y, 0), tilePalette[18]);
                        break;
                    case Direction.East:
                        Vector3Int eastCorridorPosition = new Vector3Int(corridors[i - 1].EndPositionX - 1, corridors[i - 1].EndPositionY, 0);
                        Vector3Int eastCorridorCellPosition = roomWallsTilemap.WorldToCell(eastCorridorPosition);
                        int eastCorridorExitYPosition = eastCorridorCellPosition.y + 1;
                        roomWallsTilemap.SetTile(new Vector3Int(eastCorridorCellPosition.x, eastCorridorExitYPosition, 0), tilePalette[18]);
                        break;
                    case Direction.South:
                        Vector3Int southCorridorPosition = new Vector3Int(corridors[i - 1].EndPositionX, corridors[i - 1].EndPositionY, 0);
                        Vector3Int southCorridorCellPosition = roomWallsTilemap.WorldToCell(southCorridorPosition);
                        int southCorridorExitYPosition = southCorridorCellPosition.x + 1;
                        roomWallsTilemap.SetTile(new Vector3Int(southCorridorExitYPosition, southCorridorCellPosition.y, 0), tilePalette[18]);
                        break;
                    case Direction.West:
                        Vector3Int westCorridorPosition = new Vector3Int(corridors[i - 1].EndPositionX, corridors[i - 1].EndPositionY, 0);
                        Vector3Int westCorridorCellPosition = roomWallsTilemap.WorldToCell(westCorridorPosition);
                        int westCorridorExitYPosition = westCorridorCellPosition.y + 1;
                        roomWallsTilemap.SetTile(new Vector3Int(westCorridorCellPosition.x, westCorridorExitYPosition, 0), tilePalette[18]);
                        break;
                }
            }

        }
    }

    // Create the vertical corridor game objects
    void InstantiateVerticalCorridors(GameObject tileGridForFloors, GameObject tileGridForWalls, Corridor corridor, int index = 0)
    {
        // Get the corridors starting position
        int corridorStartXPos = 0;
        int corridorStartYPos = 0;

        if (corridor.direction == Direction.North)
        {
            corridorStartXPos = corridor.startXPos;
            corridorStartYPos = corridor.startYPos;
        }
        else if (corridor.direction == Direction.South)
        {
            corridorStartXPos = corridor.EndPositionX;
            corridorStartYPos = corridor.EndPositionY;
        }

        // Set the tiles in the floor tilemap to be floors
        for (int y = 0; y < corridor.corridorLength; y++)
        {
            tileGridForFloors.GetComponent<Tilemap>().SetTile(new Vector3Int(1, y, 0), tilePalette[0]);
        }

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < corridor.corridorLength; y++)
            {
                // Bottom Left Corner
                if (x == 0 && y == 0)
                {
                    // Check if the top left corner of the room is being overlapped by the bottom left corner of the corridor
                    Vector3Int corridorBottomLeftCorner = new Vector3Int(corridorStartXPos, corridorStartYPos, 0);

                    for (int j = 0; j < rooms.Length; j++)
                    {
                        Vector3Int roomTopLeftCorner = new Vector3Int(rooms[j].xPos, rooms[j].yPos + rooms[j].height - 1, 0);

                        // If it is change it to a left wall tile
                        if (corridorBottomLeftCorner == roomTopLeftCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[1]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            // Otherwise, set it to the bottom left corner of the vertical corridor
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[16]);
                        }
                    }

                    continue;
                }

                // Top Left Corner
                if (x == 0 && y == corridor.corridorLength - 1)
                {
                    // Check if the bottom left corner of the room is being overlapped by the top left corner of the corridor
                    Vector3Int corridorTopLeftCorner = new Vector3Int(corridorStartXPos, corridorStartYPos + corridor.corridorLength - 1, 0);

                    for (int j = 0; j < rooms.Length; j++)
                    {
                        Vector3Int roomBottomLeftCorner = new Vector3Int(rooms[j].xPos, rooms[j].yPos, 0);

                        // If it is change it to a left wall tile
                        if (corridorTopLeftCorner == roomBottomLeftCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[1]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            // Otherwise, set it to the top left corner of the vertical corridor
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[14]);
                        }
                    }

                    continue;
                }

                // Bottom Right Corner
                if (x == 2 && y == 0)
                {
                    // Check if the top right corner of the room is being overlapped by the bottom right corner of the corridor
                    Vector3Int corridorBottomRightCorner = new Vector3Int(corridorStartXPos + 2, corridorStartYPos, 0);

                    for (int j = 0; j < rooms.Length; j++)
                    {
                        Vector3Int roomTopRightCorner = new Vector3Int(rooms[j].xPos + rooms[j].width - 1, rooms[j].yPos + rooms[j].height - 1, 0);

                        // If it is change it to a right wall tile
                        if (corridorBottomRightCorner == roomTopRightCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[2]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            // Otherwise, set it to the bottom right corner of the vertical corridor
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[17]);
                        }
                    }

                    continue;
                }

                // Top Right Corner
                if (x == 2 && y == corridor.corridorLength - 1)
                {
                    // Check if the bottom right corner of the room is being overlapped by the top right corner of the corridor
                    Vector3Int corridorTopRightCorner = new Vector3Int(corridorStartXPos + 2, corridorStartYPos + corridor.corridorLength - 1, 0);

                    for (int j = 0; j < rooms.Length; j++)
                    {
                        Vector3Int roomBottomRightCorner = new Vector3Int(rooms[j].xPos + rooms[j].width - 1, rooms[j].yPos, 0);

                        // If it is change it to a right wall tile
                        if (corridorTopRightCorner == roomBottomRightCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[2]);

                            // If this corner is being overlapped by a horizontal corridor then change it to a corner tile
                            if (index > 0)
                            {
                                if (corridors[index - 1].direction == Direction.West)
                                {
                                    Vector3Int corridorBottomLeftCorner = new Vector3Int(corridors[index - 1].EndPositionX, corridors[index - 1].EndPositionY, 0);

                                    if (corridorTopRightCorner == corridorBottomLeftCorner)
                                    {
                                        tileGridForWalls.GetComponent<TilemapRenderer>().sortingOrder = 4;
                                        tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[15]);
                                    }
                                }
                            }
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            // Otherwise, set it to the bottom right corner of the vertical corridor
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[15]);
                        }
                    }

                    continue;
                }

                // If the current selected tile is on the left side of the vertical corridor, 
                // create a left wall tile
                if (x == 0)
                {
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[1]);
                }

                // If the current selected tile is on the right side of the vertical corridor, 
                // create a right wall tile
                if (x == 2)
                {
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[2]);
                }
            }
        }
    }

    // Create the horizontal corridor game objects
    void InstantiateHorizontalCorridors(GameObject tileGridForFloors, GameObject tileGridForWalls, Corridor corridor)
    {
        // Get the corridors starting position
        int corridorStartXPos = 0;
        int corridorStartYPos = 0;

        if (corridor.direction == Direction.East)
        {
            corridorStartXPos = corridor.startXPos;
            corridorStartYPos = corridor.startYPos;
        }
        else if (corridor.direction == Direction.West)
        {
            corridorStartXPos = corridor.EndPositionX;
            corridorStartYPos = corridor.EndPositionY;
        }

        // Set the tiles in the floor tilemap to be floors
        for (int x = 0; x < corridor.corridorLength; x++)
        {
            tileGridForFloors.GetComponent<Tilemap>().SetTile(new Vector3Int(x, 1, 0), tilePalette[0]);
        }

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < corridor.corridorLength; x++)
            {
                // Bottom Left Corner of the corridor
                if (x == 0 && y == 0)
                {
                    // Check if the bottom right corner of the room is being overlapped by the bottom left corner of the corridor
                    Vector3Int corridorBottomLeftCorner = new Vector3Int(corridorStartXPos, corridorStartYPos, 0);

                    for (int j = 0; j < rooms.Length; j++)
                    {
                        Vector3Int roomBottomRightCorner = new Vector3Int(rooms[j].xPos + rooms[j].width - 1, rooms[j].yPos, 0);

                        // If it is change it to a bottom wall tile
                        if (corridorBottomLeftCorner == roomBottomRightCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[4]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            // Otherwise, set it to the bottom left corner of the horizontal corridor
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[12]);
                        }
                    }

                    continue;
                }

                // Bottom Right Corner of the corridor
                if (y == 0 && x == corridor.corridorLength - 1)
                {
                    // Check if the bottom Left corner of the room is being overlapped by the bottom right corner of the corridor
                    Vector3Int corridorBottomRightCorner = new Vector3Int(corridorStartXPos + corridor.corridorLength - 1, corridorStartYPos, 0);

                    for (int j = 0; j < rooms.Length; j++)
                    {
                        Vector3Int roomBottomLeftCorner = new Vector3Int(rooms[j].xPos, rooms[j].yPos, 0);

                        // If it is change it to a bottom wall tile
                        if (corridorBottomRightCorner == roomBottomLeftCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[4]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            // Otherwise, set it to the bottom right corner of the horizontal corridor
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[13]);
                        }
                    }

                    continue;
                }

                // Top Left Corner of the corridor
                if (y == 2 && x == 0)
                {
                    // Check if the top right corner of the room is being overlapped by the top left corner of the corridor
                    Vector3Int corridorTopLeftCorner = new Vector3Int(corridorStartXPos, corridorStartYPos + 2, 0);

                    for (int j = 0; j < rooms.Length; j++)
                    {
                        Vector3Int roomTopRightCorner = new Vector3Int(rooms[j].xPos + rooms[j].width - 1, rooms[j].yPos + rooms[j].height - 1, 0);

                        // If it is change it to a top wall tile
                        if (corridorTopLeftCorner == roomTopRightCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[3]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            // Otherwise, set it to the top left corner of the horizontal corridor
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[10]);
                        }
                    }

                    continue;
                }

                // Top Right Corner of the corridor
                if (y == 2 && x == corridor.corridorLength - 1)
                {
                    // Check if the top left corner of the room is being overlapped by the top right corner of the corridor
                    Vector3Int corridorTopRightCorner = new Vector3Int(corridorStartXPos + corridor.corridorLength - 1, corridorStartYPos + 2, 0);

                    for (int j = 0; j < rooms.Length; j++)
                    {
                        Vector3Int roomTopLeftCorner = new Vector3Int(rooms[j].xPos, rooms[j].yPos + rooms[j].height - 1, 0);

                        // If it is change it to a top wall tile
                        if (corridorTopRightCorner == roomTopLeftCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[3]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            // Otherwise, set it to the top right corner of the horizontal corridor
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[11]);
                        }
                    }

                    continue;
                }

                // If the current selected tile is on the bottom side of the horizontal corridor, 
                // create a bottom wall tile
                if (y == 0)
                {
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[4]);
                }

                // If the current selected tile is on the top side of the horizontal corridor, 
                // create a top wall tile
                if (y == 2)
                {
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[3]);
                }
            }
        }
    }

    // Create the corridor game objects
    void InstantiateCorridors()
    {
        // Foreach corridor in the dungeon...
        for (int i = 0; i < corridors.Length; i++)
        {
            // Create a corridor game object with 2 different tilemaps. One for floor and one for wall
            GameObject corridorGameObject = new GameObject("Corridor " + i);

            GameObject tileGridForFloors = new GameObject("Floors");
            tileGridForFloors.AddComponent<Tilemap>();
            tileGridForFloors.AddComponent<TilemapRenderer>();

            GameObject tileGridForWalls = new GameObject("Walls");
            tileGridForWalls.AddComponent<Tilemap>();
            tileGridForWalls.AddComponent<TilemapRenderer>();
            tileGridForWalls.AddComponent<TilemapCollider2D>();
            tileGridForWalls.GetComponent<TilemapRenderer>().sortingOrder = 3;

            // If the direction of the corridor is going north or south then create a vertical corridor
            // or if the direction of the corridor is going east or west then create a horizontal corridor
            switch (corridors[i].direction)
            {
                case Direction.North:
                    InstantiateVerticalCorridors(tileGridForFloors, tileGridForWalls, corridors[i]);
                    break;
                case Direction.East:
                    InstantiateHorizontalCorridors(tileGridForFloors, tileGridForWalls, corridors[i]);
                    break;
                case Direction.South:
                    InstantiateVerticalCorridors(tileGridForFloors, tileGridForWalls, corridors[i], i);
                    break;
                case Direction.West:
                    InstantiateHorizontalCorridors(tileGridForFloors, tileGridForWalls, corridors[i]);
                    break;
            }

            // Set the tilemaps to be the child of the corridor game object
            tileGridForFloors.transform.SetParent(corridorGameObject.transform);
            tileGridForWalls.transform.SetParent(corridorGameObject.transform);

            // Set the corridor game object to be the child of the board holder
            corridorGameObject.transform.SetParent(boardHolder.transform);

            // Place the corridors in the correct positions
            switch (corridors[i].direction)
            {
                case Direction.North:
                    corridorGameObject.transform.position = new Vector3(corridors[i].startXPos, corridors[i].startYPos);
                    break;
                case Direction.East:
                    corridorGameObject.transform.position = new Vector3(corridors[i].startXPos, corridors[i].startYPos);
                    break;
                case Direction.South:
                    corridorGameObject.transform.position = new Vector3(corridors[i].EndPositionX, corridors[i].EndPositionY);
                    break;
                case Direction.West:
                    corridorGameObject.transform.position = new Vector3(corridors[i].EndPositionX, corridors[i].EndPositionY);
                    break;
            }
        }
    }

    // Create the outer walls for the dungeon
    void InstantiateOuterWalls()
    {
        // Create the tilemap object for the outer walls of the dungeon
        GameObject tilemapForVoidTiles = new GameObject("VoidTiles");
        tilemapForVoidTiles.AddComponent<Tilemap>();
        tilemapForVoidTiles.AddComponent<TilemapRenderer>();
        tilemapForVoidTiles.GetComponent<TilemapRenderer>().sortingOrder = -1;

        // Go through the tile jagged array
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                // If the tile type is null
                if (tiles[x][y] == TileType.NULL)
                {
                    // Set it to be a void tile
                    tilemapForVoidTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[9]);
                }
            }
        }

        // Set the outer walls game object to be the child of the board holder
        tilemapForVoidTiles.transform.SetParent(boardHolder.transform);
    }

    // Get the tile palette for the room
    private void GetTilePalette()
    {
        // If there is already a tile palette saved...
        if (tilePalette.Count > 0)
        {
            // ... Clear the tile palette list
            tilePalette.Clear();
        }
        
        string path = "Assets/2DProceduralDungeonGenerator/Resources/TilePalette.txt";
        reader = new StreamReader(path);

        // While the text string is not empty...
        while ((text = reader.ReadLine()) != null)
        {
            if (text == "Null")
            {
                tilePalette.Add(null);
            }

            // Find the tile GUID in the asset database
            string[] results = AssetDatabase.FindAssets(text + " t:Tile");

            // Using that GUID
            foreach (string guid in results)
            {
                // Find the tile and add it to the tile palette list
                string tilePath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = (TileBase)AssetDatabase.LoadAssetAtPath(tilePath, typeof(TileBase));

                if (asset.name == text)
                {
                    tilePalette.Add(asset);
                }
            }
        }
    }

}
#endif
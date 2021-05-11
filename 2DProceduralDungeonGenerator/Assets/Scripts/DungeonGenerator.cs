/*----------------------------------------------
    File Name: DungeonGenerator.cs
    Purpose: Generate dungeons for the developer
    Author: Logan Ryan
    Modified: 11/05/2021
------------------------------------------------
    Copyright 2021 Logan Ryan
----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    // Enum to store the type of tiles in the dungeon
    public enum TileType
    {
        NULL = 0,
        ROOM = 1,
        CORRIDOR = 2,
    }

    public int columns = 100;                               // Number of columns in the grid
    public int rows = 100;                                  // Number of rows in the grid
    public int numberOfRooms;                               // Number of rooms in the dungeon
    public IntRange corridorLength = new IntRange(3, 10);   // Lengths of the corridor
    public GameObject[] roomPrefabs;                        // Rooms that are in the dungeon

    private bool success = false;                           // Dungeon was successfully created
    private GameObject boardHolder;                         // GameObject that holds the dungeon
    private Room[] rooms;                                   // Array to store the rooms in the dungeon
    private Corridor[] corridors;                           // Array to store the corridors in the dungeon
    private TileType[][] tiles;                             // Jagged tile array that stores the type of tiles in the dungeon

    private StreamReader reader;                            // Reader for text files
    private string text;                                    // Line of text from the text file
    List<TileBase> tilePalette = new List<TileBase>();      // Tiles to use in the dungeon

    public void GenerateDungeon()
    {
        if (boardHolder == null)
        {
            boardHolder = GameObject.Find("BoardHolder");
        }
        
        if (boardHolder != null)
        {
            DestroyImmediate(boardHolder);
        }

        boardHolder = new GameObject("BoardHolder");
        boardHolder.AddComponent<Grid>();

        // Get Tile Palette
        if (tilePalette.Count == 0)
        {
            GetTilePalette();
        }

        do
        {
            SetupTilesArray();

            CreateRoomsAndCorridors();

            SetTilesValuesForRooms();
            SetTilesValuesForCorridors();
        } while (!success);

        InstantiateRooms();
        InstantiateCorridors();
        InstantiateOuterWalls();
    }

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

    void CreateRoomsAndCorridors()
    {
        success = true;
        
        // Create the rooms array with a size of the number of rooms
        rooms = new Room[numberOfRooms];

        // There should be one less corridor than there is rooms
        corridors = new Corridor[numberOfRooms - 1];

        // Create the first room and corridor
        rooms[0] = new Room();
        corridors[0] = new Corridor();

        int roomIndex = Random.Range(0, roomPrefabs.Length);

        BoundsInt roomBounds = roomPrefabs[roomIndex].GetComponentInChildren<Tilemap>().cellBounds;

        // Setup the first room
        rooms[0].SetupFirstRoom(roomBounds, roomPrefabs[roomIndex], columns, rows);

        // Setup the first corridor
        corridors[0].SetupCorridor(rooms[0], corridorLength, columns, rows, true);

        for (int i = 1; i < rooms.Length; i++)
        {
            // Create a room
            rooms[i] = new Room();

            // Setup the room based on the previous corridor.
            roomIndex = Random.Range(0, roomPrefabs.Length);

            roomBounds = roomPrefabs[roomIndex].GetComponentInChildren<Tilemap>().cellBounds;
            rooms[i].SetupRoom(roomBounds, roomPrefabs[roomIndex], columns, rows, corridors[i - 1]);

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
                    success = false;
                }
            }
        }
    }

    void CheckCorridor2CorridorCollision()
    {
        for (int i = 0; i < corridors.Length; i++)
        {
            for (int j = i + 1; j < corridors.Length; j++)
            {
                Rect room1Rect = new Rect();

                switch (corridors[i].direction)
                {
                    case Direction.North:
                        room1Rect = new Rect(corridors[i].startXPos, corridors[i].startYPos + 1, 3, corridors[i].corridorLength - 1);
                        break;
                    case Direction.East:
                        room1Rect = new Rect(corridors[i].startXPos + 1, corridors[i].startYPos, corridors[i].corridorLength - 1, 3);
                        break;
                    case Direction.South:
                        room1Rect = new Rect(corridors[i].startXPos, corridors[i].startYPos - 1, 3, corridors[i].corridorLength - 1);
                        break;
                    case Direction.West:
                        room1Rect = new Rect(corridors[i].startXPos - 1, corridors[i].startYPos, corridors[i].corridorLength - 1, 3);
                        break;
                }

                Rect room2Rect = new Rect();

                switch (corridors[j].direction)
                {
                    case Direction.North:
                        room2Rect = new Rect(corridors[j].startXPos, corridors[j].startYPos + 1, 3, corridors[j].corridorLength - 1);
                        break;
                    case Direction.East:
                        room2Rect = new Rect(corridors[j].startXPos + 1, corridors[j].startYPos, corridors[j].corridorLength - 1, 3);
                        break;
                    case Direction.South:
                        room2Rect = new Rect(corridors[j].startXPos, corridors[j].startYPos - 1, 3, corridors[j].corridorLength - 1);
                        break;
                    case Direction.West:
                        room2Rect = new Rect(corridors[j].startXPos - 1, corridors[j].startYPos, corridors[j].corridorLength - 1, 3);
                        break;
                }

                if (room1Rect.Overlaps(room2Rect))
                {
                    success = false;
                }
            }
        }
    }

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
                        
                        success = false;
                        return;
                    }
                    // The coordinates in the jagged array are based on the room's position and it's width and height
                    tiles[xCoord][yCoord] = TileType.ROOM;
                }
            }
        }
    }

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
                switch (rooms[i].enteringDirection)
                {
                    case Direction.North:
                        // Get the south face of the room
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
                        // Get the north face of the room
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

    void InstantiateVerticalCorridors(GameObject tileGridForFloors, GameObject tileGridForWalls, Corridor corridor, int index = 0)
    {
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

                        if (corridorBottomLeftCorner == roomTopLeftCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[1]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
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

                        if (corridorTopLeftCorner == roomBottomLeftCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[1]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
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

                        if (corridorBottomRightCorner == roomTopRightCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[2]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
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

                        if (corridorTopRightCorner == roomBottomRightCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[2]);

                            if (index > 0)
                            {
                                if (corridors[index - 1].direction == Direction.West)
                                {
                                    Vector3Int corridorBottomLeftCorner = new Vector3Int(corridors[index - 1].EndPositionX, corridors[index - 1].EndPositionY, 0);

                                    if (corridorTopRightCorner == corridorBottomLeftCorner)
                                    {
                                        tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[15]);
                                    }
                                }
                            }
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[15]);
                        }
                    }

                    continue;
                }

                if (x == 0)
                {
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[1]);
                }

                if (x == 2)
                {
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[2]);
                }
            }
        }
    }

    void InstantiateHorizontalCorridors(GameObject tileGridForFloors, GameObject tileGridForWalls, Corridor corridor)
    {
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

                        if (corridorBottomLeftCorner == roomBottomRightCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[4]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
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

                        if (corridorBottomRightCorner == roomBottomLeftCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[4]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
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

                        if (corridorTopLeftCorner == roomTopRightCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[3]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
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

                        if (corridorTopRightCorner == roomTopLeftCorner)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[3]);
                            break;
                        }
                        else if (j == rooms.Length - 1)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[11]);
                        }
                    }

                    continue;
                }

                if (y == 0)
                {
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[4]);
                }

                if (y == 2)
                {
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[3]);
                }
            }
        }
    }

    void InstantiateCorridors()
    {
        for (int i = 0; i < corridors.Length; i++)
        {
            GameObject corridorGameObject = new GameObject("Corridor " + i);

            GameObject tileGridForFloors = new GameObject("Floors");
            tileGridForFloors.AddComponent<Tilemap>();
            tileGridForFloors.AddComponent<TilemapRenderer>();

            GameObject tileGridForWalls = new GameObject("Walls");
            tileGridForWalls.AddComponent<Tilemap>();
            tileGridForWalls.AddComponent<TilemapRenderer>();
            tileGridForWalls.AddComponent<TilemapCollider2D>();
            tileGridForWalls.GetComponent<TilemapRenderer>().sortingOrder = 3;

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

            tileGridForFloors.transform.SetParent(corridorGameObject.transform);
            tileGridForWalls.transform.SetParent(corridorGameObject.transform);
            corridorGameObject.transform.SetParent(boardHolder.transform);

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

    void InstantiateOuterWalls()
    {
        GameObject tilemapForVoidTiles = new GameObject("VoidTiles");
        tilemapForVoidTiles.AddComponent<Tilemap>();
        tilemapForVoidTiles.AddComponent<TilemapRenderer>();
        tilemapForVoidTiles.GetComponent<TilemapRenderer>().sortingOrder = -1;


        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (tiles[x][y] == TileType.NULL)
                {
                    tilemapForVoidTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tilePalette[9]);
                }
            }
        }

        tilemapForVoidTiles.transform.SetParent(boardHolder.transform);
    }

    private void GetTilePalette()
    {
        string path = "Assets/Resources/TilePalette.txt";
        reader = new StreamReader(path);

        while ((text = reader.ReadLine()) != null)
        {
            if (text == "Null")
            {
                tilePalette.Add(null);
            }
            
            // Set Tile's
            string[] results = AssetDatabase.FindAssets(text + " t:Tile");

            foreach (string guid in results)
            {
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

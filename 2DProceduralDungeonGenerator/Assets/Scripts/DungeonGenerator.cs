using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DungeonGenerator : MonoBehaviour
{
    public enum TileType
    {
        NULL = 0,
        ROOM = 1,
        CORRIDOR = 2,
    }

    public int columns = 100;
    public int rows = 100;
    public int numberOfRooms;
    public IntRange corridorLength = new IntRange(3, 10);
    public GameObject[] roomPrefabs;

    private bool success = false;
    private GameObject boardHolder;
    private Room[] rooms;
    private Corridor[] corridors;
    private TileType[][] tiles;

    private StreamReader reader;
    private string text;
    List<TileBase> tilePalette = new List<TileBase>();

    // Start is called before the first frame update
    void Start()
    {
        boardHolder = new GameObject("BoardHolder");
        boardHolder.AddComponent<Grid>();

        // Get Tile Palette
        if (tilePalette.Count == 0)
        {
            GetTilePalette();
        }

        SetupTilesArray();

        do
        {
            CreateRoomsAndCorridors();
        } while (!success);
        

        SetTilesValuesForRooms();
        SetTilesValuesForCorridors();

        InstantiateRooms();
        InstantiateCorridors();
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
        for (int i = 0; i < rooms.Length; i++)
        {
            for (int j = i + 1; j < rooms.Length; j++)
            {
                if (rooms[i].xPos >= rooms[j].xPos && rooms[i].xPos <= rooms[j].xPos + rooms[j].width &&
                    rooms[i].yPos >= rooms[j].yPos && rooms[i].yPos <= rooms[j].yPos + rooms[j].height)
                {
                    //Debug.Log("Rooms Overlapping");
                    success = false;
                }
                else if (rooms[i].xPos >= rooms[j].xPos && rooms[i].xPos <= rooms[j].xPos + rooms[j].width &&
                        rooms[i].yPos + rooms[i].height >= rooms[j].yPos && rooms[i].yPos + rooms[i].height <= rooms[j].yPos + rooms[j].height)
                {
                    //Debug.Log("Rooms Overlapping");
                    success = false;
                }

                else if (rooms[i].xPos + rooms[i].width >= rooms[j].xPos && rooms[i].xPos + rooms[i].width <= rooms[j].xPos + rooms[j].width &&
                    rooms[i].yPos >= rooms[j].yPos && rooms[i].yPos <= rooms[j].yPos + rooms[j].height)
                {
                    //Debug.Log("Rooms Overlapping");
                    success = false;
                }

                else if (rooms[i].xPos + rooms[i].width >= rooms[j].xPos && rooms[i].xPos + rooms[i].width <= rooms[j].xPos + rooms[j].width &&
                     rooms[i].yPos + rooms[i].height >= rooms[j].yPos && rooms[i].yPos + rooms[i].height <= rooms[j].yPos + rooms[j].height)
                {
                    //Debug.Log("Rooms Overlapping");
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

                    if (xCoord < 0 || xCoord > tiles.Length)
                    {
                        Debug.Log("X Coord problem");
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

                // Set the tile at these coordinates to corridor
                tiles[xCoord][yCoord] = TileType.CORRIDOR;
            }
        }
    }

    void InstantiateRooms()
    {
        for (int i = 0; i < rooms.Length; i++)
        {
            Instantiate(rooms[i].roomTemplate, new Vector3(rooms[i].xPos, rooms[i].yPos, 0), rooms[i].roomTemplate.transform.rotation, boardHolder.transform);
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
            tileGridForWalls.GetComponent<TilemapRenderer>().sortingOrder = 1;

            switch (corridors[i].direction)
            {
                case Direction.North:
                    for (int y = 0; y < corridors[i].corridorLength; y++)
                    {
                        tileGridForFloors.GetComponent<Tilemap>().SetTile(new Vector3Int(1, y, 0), tilePalette[0]);
                    }

                    //Debug.Log(corridorGameObject.name);
                    //Debug.Log(corridors[i].startXPos + " - " + corridors[i].EndPositionX);
                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < corridors[i].corridorLength; y++)
                        {
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
                    break;
                case Direction.East:
                    for (int x = 0; x < corridors[i].corridorLength; x++)
                    {
                        tileGridForFloors.GetComponent<Tilemap>().SetTile(new Vector3Int(x, 1, 0), tilePalette[0]);
                    }

                    //Debug.Log(corridorGameObject.name);

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < corridors[i].corridorLength; x++)
                        {
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
                    break;
                case Direction.South:
                    for (int y = 0; y < corridors[i].corridorLength; y++)
                    {
                        tileGridForFloors.GetComponent<Tilemap>().SetTile(new Vector3Int(1, y, 0), tilePalette[0]);
                    }

                    for (int x = 0; x < 3; x++)
                    {
                        for (int y = 0; y < corridors[i].corridorLength; y++)
                        {
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
                    break;
                case Direction.West:
                    for (int x = 0; x < corridors[i].corridorLength; x++)
                    {
                        tileGridForFloors.GetComponent<Tilemap>().SetTile(new Vector3Int(x, 1, 0), tilePalette[0]);
                    }

                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < corridors[i].corridorLength; x++)
                        {
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

    private void GetTilePalette()
    {
        string path = "Assets/Resources/TilePalette.txt";
        reader = new StreamReader(path);

        while ((text = reader.ReadLine()) != null)
        {
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

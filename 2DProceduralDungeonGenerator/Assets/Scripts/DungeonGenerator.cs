using System.Collections;
using System.Collections.Generic;
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
    
    private GameObject boardHolder;
    private Room[] rooms;
    private Corridor[] corridors;
    private TileType[][] tiles;
    
    // Start is called before the first frame update
    void Start()
    {
        boardHolder = new GameObject("BoardHolder");
        boardHolder.AddComponent<Grid>();

        SetupTilesArray();

        CreateRoomsAndCorridors();
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
        // Create the rooms array with a size of the number of rooms
        rooms = new Room[numberOfRooms];

        // There should be one less corridor than there is rooms
        corridors = new Corridor[numberOfRooms - 1];

        // Create the first room and corridor
        rooms[0] = new Room();
        corridors[0] = new Corridor();

        BoundsInt roomBounds = roomPrefabs[0].GetComponentInChildren<Tilemap>().cellBounds;

        // Setup the first room
        // Room object should take in a room prefab
        rooms[0].SetupFirstRoom(roomBounds, columns, rows);

        // Setup the first corridor
        corridors[0].SetupCorridor(rooms[0], corridorLength, columns, rows, true);
        //Instantiate(roomPrefabs[0], new Vector3(rooms[0].xPos, rooms[0].yPos, 0), roomPrefabs[0].transform.rotation, boardHolder.transform);

        for (int i = 1; i < rooms.Length; i++)
        {
            // Create a room
            rooms[i] = new Room();

            // Setup the room based on the previous corridor.
            int roomIndex = Random.Range(0, roomPrefabs.Length);

            roomBounds = roomPrefabs[roomIndex].GetComponentInChildren<Tilemap>().cellBounds;
            rooms[i].SetupRoom(roomBounds, columns, rows, corridors[i - 1]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

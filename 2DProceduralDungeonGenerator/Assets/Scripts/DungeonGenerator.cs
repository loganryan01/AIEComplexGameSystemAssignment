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
    public GameObject[] roomPrefabs;
    
    private GameObject boardHolder;
    private Room[] rooms;
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

        // Create the first room
        rooms[0] = new Room();

        BoundsInt roomBounds = roomPrefabs[0].GetComponentInChildren<Tilemap>().cellBounds;

        // Set up the first room
        rooms[0].SetupFirstRoom(roomBounds, columns, rows);
        Instantiate(roomPrefabs[0], new Vector3(rooms[0].xPos, rooms[0].yPos, 0), roomPrefabs[0].transform.rotation, boardHolder.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

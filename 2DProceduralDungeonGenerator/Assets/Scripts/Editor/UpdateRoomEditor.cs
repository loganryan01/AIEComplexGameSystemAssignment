using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UpdateRoomEditor : EditorWindow
{
    private int width;
    private int height;
    private int numberOfAdditionalTiles;

    protected string text = " ";
    private string roomName = null;

    private bool gotData = false;

    private GameObject room;
    private GameObject floorTiles;
    private GameObject wallTiles;
    private GameObject additionalTilesPreview;
    private GameObject allTilesPreview;

    private List<TileBase> tiles = new List<TileBase>();
    private List<AdditionalTiles> additionalTiles = new List<AdditionalTiles>();

    protected StreamReader reader = null;

    [MenuItem("DungeonGenerator/Update Room")]
    static void ShowWindow()
    {
        GetWindow<UpdateRoomEditor>("Update Room Editor");
    }

    private void OnGUI()
    {
        // Get a room prefab
        room = (GameObject)EditorGUI.ObjectField(new Rect(0, 5, position.width, 20), "Room Prefab: ", room, typeof(GameObject), true);

        // Get Tile Palette
        if (tiles.Count == 0)
        {
            GetTilePalette();
        }

        if (room != null)
        {
            if (!gotData)
            {
                // Get the name of the room
                roomName = room.name;
                
                // Get the width and height of the prefab
                BoundsInt bounds = room.GetComponentInChildren<Tilemap>().cellBounds;

                width = bounds.size.x;
                height = bounds.size.y;

                // Get number of additional tiles and what tiles have been used
                Tilemap[] tilemap = room.GetComponentsInChildren<Tilemap>();
                TileBase[] tileArray = tilemap[2].GetTilesBlock(bounds);

                int xPos = 0;
                int yPos = 0;
                for (int index = 0; index < tileArray.Length; index++)
                {
                    if (tileArray[index] != null)
                    {
                        numberOfAdditionalTiles++;
                        

                        // Check if the tile exists in the list
                        if (additionalTiles.Count == 0)
                        {
                            additionalTiles.Add(new AdditionalTiles(tileArray[index], new Vector3Int(xPos, yPos, 0)));
                        }
                    }

                    xPos++;

                    if (xPos >= width)
                    {
                        xPos = 0;
                        yPos++;
                    }
                }

                gotData = true;

                PreviewAllTiles();
            }


            // Get the width and height of the room
            EditorGUI.BeginChangeCheck();
            roomName = EditorGUI.TextField(new Rect(0, 30, position.width, 20), "Room Name: ", roomName);
            width = EditorGUI.IntField(new Rect(0, 55, position.width, 20), "Width", width);
            height = EditorGUI.IntField(new Rect(0, 80, position.width, 20), "Height", height);

            // Get the additional tiles list
            numberOfAdditionalTiles = EditorGUI.IntField(new Rect(0, 105, position.width, 20), "Number of additional tiles", numberOfAdditionalTiles);

            // If there are meant to be no additional tiles, clear the whole list
            if (numberOfAdditionalTiles == 0)
            {
                additionalTiles.Clear();
            }

            if (numberOfAdditionalTiles > 0)
            {
                // Increase size of list
                while (additionalTiles.Count < numberOfAdditionalTiles)
                {
                    additionalTiles.Add(new AdditionalTiles(null, Vector3Int.zero));
                }

                // Decrease size of list
                while (additionalTiles.Count > numberOfAdditionalTiles)
                {
                    additionalTiles.RemoveAt(additionalTiles.Count - 1);
                }

                for (int i = 0; i < numberOfAdditionalTiles; i++)
                {
                    // Display tilebase box for each additional tile
                    additionalTiles[i].tile = (TileBase)EditorGUI.ObjectField(new Rect(0, 25 * i + 130, position.width - 135, 20), "Tile: ", additionalTiles[i].tile, typeof(TileBase), true);
                    additionalTiles[i].position =
                        EditorGUI.Vector3IntField(new Rect(position.width - 130, 25 * i + 130, position.width - 280, 20), "", additionalTiles[i].position);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                DisableAllTiles();

                if (allTilesPreview == null)
                {
                    PreviewAllTiles();
                }
            }

            EditorGUILayout.Space(25 * numberOfAdditionalTiles + 130);

            if (GUILayout.Button("Update"))
            {
                UpdateRoom();
            }
        }
        
    }

    private void OnDestroy()
    {
        DisableAllTiles();
    }

    private void DisableAllTiles()
    {
        if (allTilesPreview != null)
        {
            DestroyImmediate(allTilesPreview);
        }
    }

    private void PreviewAllTiles()
    {
        allTilesPreview = new GameObject("RoomPreview");

        PreviewFloor();
        PreviewWalls();
        PreviewAdditionalTiles();

        floorTiles.transform.SetParent(allTilesPreview.transform);
        wallTiles.transform.SetParent(allTilesPreview.transform);
        additionalTilesPreview.transform.SetParent(allTilesPreview.transform);

        allTilesPreview.transform.SetParent(GameObject.Find("Grid").transform);
    }

    private void PreviewAdditionalTiles()
    {
        additionalTilesPreview = new GameObject("AdditionalTiles");
        additionalTilesPreview.AddComponent<Tilemap>();
        additionalTilesPreview.AddComponent<TilemapRenderer>();

        foreach (var additionalTile in additionalTiles)
        {
            additionalTilesPreview.GetComponent<Tilemap>().SetTile(additionalTile.position, additionalTile.tile);
        }
    }

    private void PreviewWalls()
    {
        wallTiles = new GameObject("Walls");
        wallTiles.AddComponent<Tilemap>();
        wallTiles.AddComponent<TilemapRenderer>();

        int maxX = width - 1;
        int maxY = height - 1;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                switch (y)
                {
                    case 0 when x == 0:
                        wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[7]);
                        break;
                    case 0 when x < maxX:
                        wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[4]);
                        break;
                    case 0 when x == maxX:
                        wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[8]);
                        break;
                    default:
                        if (x == 0)
                        {
                            wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[1]);
                        }

                        if (x == maxX)
                        {
                            wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[2]);
                        }
                        break;
                }

                if (y == maxY && x == 0)
                    wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[5]);
                else if (y == maxY && x < maxX)
                    wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[3]);
                else if (y == maxY && x == maxX)
                    wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[6]);

            }
        }
    }

    private void PreviewFloor()
    {
        floorTiles = new GameObject("Floors");
        floorTiles.AddComponent<Tilemap>();
        floorTiles.AddComponent<TilemapRenderer>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                floorTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[0]);
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
                    tiles.Add(asset);
                }
            }
        }
    }

    private void UpdateRoom()
    {
        room = new GameObject(roomName);

        GameObject tileGridForFloors = new GameObject("Floors");
        tileGridForFloors.AddComponent<Tilemap>();
        tileGridForFloors.AddComponent<TilemapRenderer>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tileGridForFloors.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[0]);
            }
        }

        GameObject tileGridForWalls = new GameObject("Walls");
        tileGridForWalls.AddComponent<Tilemap>();
        tileGridForWalls.AddComponent<TilemapRenderer>();
        tileGridForWalls.AddComponent<TilemapCollider2D>();
        tileGridForWalls.GetComponent<TilemapRenderer>().sortingOrder = 1;

        int maxX = width - 1;
        int maxY = height - 1;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                switch (y)
                {
                    case 0 when x == 0:
                        tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[7]);
                        break;
                    case 0 when x < maxX:
                        tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[4]);
                        break;
                    case 0 when x == maxX:
                        tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[8]);
                        break;
                    default:
                        if (x == 0)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[1]);
                        }

                        if (x == maxX)
                        {
                            tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[2]);
                        }
                        break;
                }

                if (y == maxY && x == 0)
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[5]);
                else if (y == maxY && x < maxX)
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[3]);
                else if (y == maxY && x == maxX)
                    tileGridForWalls.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[6]);

            }
        }

        GameObject tileGridForAdditional = new GameObject("AdditionalTiles");
        tileGridForAdditional.AddComponent<Tilemap>();
        tileGridForAdditional.AddComponent<TilemapRenderer>();
        tileGridForAdditional.AddComponent<TilemapCollider2D>();
        tileGridForAdditional.GetComponent<TilemapRenderer>().sortingOrder = 2;

        foreach (var additionalTile in additionalTiles)
        {
            tileGridForAdditional.GetComponent<Tilemap>().SetTile(additionalTile.position, additionalTile.tile);
        }

        tileGridForFloors.transform.SetParent(room.transform);
        tileGridForWalls.transform.SetParent(room.transform);
        tileGridForAdditional.transform.SetParent(room.transform);

        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Rooms"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Rooms");
        }

        string localPath = "Assets/Prefabs/Rooms/" + room.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        //PrefabUtility.SaveAsPrefabAsset(room, localPath);
        PrefabUtility.SavePrefabAsset(room);

        DestroyImmediate(room);

        Close();
    }
}

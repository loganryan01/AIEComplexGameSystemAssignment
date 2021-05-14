﻿/*-----------------------------------------
    File Name: UpdateRoomEditor.cs
    Purpose: Update existing created rooms.
    Author: Logan Ryan
    Modified: 14/05/2021
-------------------------------------------
    Copyright 2021 Logan Ryan
-----------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UpdateRoomEditor : EditorWindow
{
    private int width = 3;                                                          // Width of the room
    private int height = 3;                                                         // Height of the room
    private int numberOfAdditionalTiles;                                            // Number of additional tiles for the room

    protected string text = " ";                                                    // Line of text from the tile palette text file
    private string roomName = null;                                                 // The name of the room

    private GameObject room;                                                        // Gameobject for the room prefab
    private GameObject floorTiles;                                                  // Gameobject to store the floor tiles for preview
    private GameObject wallTiles;                                                   // Gameobject to store the wall tiles for preview
    private GameObject additionalTilesPreview;                                      // Gameobject to store the additional tiles for preview
    private GameObject allTilesPreview;                                             // Gameobject to store all the tiles for preview

    private List<TileBase> tiles = new List<TileBase>();                            // List of tiles in the room
    private List<AdditionalTiles> additionalTiles = new List<AdditionalTiles>();    // List of additional tiles

    protected StreamReader reader = null;                                           // Reader for the tile palette text file

    // Create room editor window
    [MenuItem("DungeonGenerator/Update Room")]
    static void ShowWindow()
    {
        GetWindow<UpdateRoomEditor>("Update Room Editor");
    }

    private void OnEnable()
    {
        // Get Tile Palette
        GetTilePalette();
    }

    private void Update()
    {
        if (room != null)
        {
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
            }
        }
    }

    private void OnGUI()
    {
        // Get a room prefab
        EditorGUI.BeginChangeCheck();
        room = (GameObject)EditorGUI.ObjectField(new Rect(0, 5, position.width, 20), "Room Prefab: ", room, typeof(GameObject), true);
        if (EditorGUI.EndChangeCheck())
        {
            // Clear the additional tiles list
            additionalTiles.Clear();
            numberOfAdditionalTiles = 0;
            
            // Get the name of the room
            roomName = room.name;

            // Get the width and height of the prefab
            BoundsInt bounds = room.GetComponentInChildren<Tilemap>().cellBounds;

            width = bounds.size.x;
            height = bounds.size.y;

            // Get number of additional tiles and what tiles have been used
            Tilemap[] tilemap = room.GetComponentsInChildren<Tilemap>();
            TileBase[] tileArray = tilemap[2].GetTilesBlock(bounds);

            // Add the additional tile to the additional tile list
            int xPos = 0;
            int yPos = 0;
            for (int index = 0; index < tileArray.Length; index++)
            {
                if (tileArray[index] != null)
                {
                    numberOfAdditionalTiles++;
                    additionalTiles.Add(new AdditionalTiles(tileArray[index], new Vector3Int(xPos, yPos, 0)));
                }

                xPos++;

                if (xPos >= width)
                {
                    xPos = 0;
                    yPos++;
                }
            }

            // Create a preview of the room
            DisableAllTiles();

            if (allTilesPreview == null)
            {
                PreviewAllTiles();
            }
        }

        // If the user has chosen a room to modify ...
        if (room != null)
        {
            // Get the width and height of the room
            EditorGUI.BeginChangeCheck();
            roomName = EditorGUI.TextField(new Rect(0, 30, position.width, 20), "Room Name: ", roomName);
            width = EditorGUI.IntField(new Rect(0, 55, position.width, 20), "Width", width);
            height = EditorGUI.IntField(new Rect(0, 80, position.width, 20), "Height", height);

            // Get the additional tiles list
            numberOfAdditionalTiles = EditorGUI.IntField(new Rect(0, 105, position.width, 20), "Number of additional tiles", numberOfAdditionalTiles);

            // If the number of additional is greater than zero...
            if (numberOfAdditionalTiles > 0)
            {
                // For every additional tile...
                for (int i = 0; i < numberOfAdditionalTiles; i++)
                {
                    // ... Display additional tile fields
                    additionalTiles[i].tile = (TileBase)EditorGUI.ObjectField(new Rect(0, 25 * i + 130, (position.width / 4) + 25, 20), "Tile: ", additionalTiles[i].tile, typeof(TileBase), true);
                    additionalTiles[i].position =
                        EditorGUI.Vector3IntField(new Rect(0 + (position.width / 4) + 30, 25 * i + 130, position.width / 4, 20), "", additionalTiles[i].position);
                }
            }

            // If the user makes a change...
            if (EditorGUI.EndChangeCheck())
            {
                // Refresh the preview of the room
                DisableAllTiles();

                if (allTilesPreview == null)
                {
                    PreviewAllTiles();
                }
            }

            EditorGUILayout.Space(25 * numberOfAdditionalTiles + 130);

            // If the user clicks the button "Update"...
            if (GUILayout.Button("Update"))
            {
                // ... Update the room
                UpdateRoom();
            }
        }
        
    }

    private void OnDestroy()
    {
        // Destory the preview when the window closes
        DisableAllTiles();
    }

    // Destroys the preview object
    private void DisableAllTiles()
    {
        if (allTilesPreview != null)
        {
            DestroyImmediate(allTilesPreview);
        }
    }

    // Shows a preview of the room
    private void PreviewAllTiles()
    {
        allTilesPreview = new GameObject("RoomPreview");
        allTilesPreview.AddComponent<Grid>();

        PreviewFloor();
        PreviewWalls();
        PreviewAdditionalTiles();

        // Set each tilemap object to be the child of the room preview
        floorTiles.transform.SetParent(allTilesPreview.transform);
        wallTiles.transform.SetParent(allTilesPreview.transform);
        additionalTilesPreview.transform.SetParent(allTilesPreview.transform);
    }

    // Show a preview of the additional tiles in the room
    private void PreviewAdditionalTiles()
    {
        additionalTilesPreview = new GameObject("AdditionalTiles");
        additionalTilesPreview.AddComponent<Tilemap>();
        additionalTilesPreview.AddComponent<TilemapRenderer>();

        // Foreach additional tile in the room...
        foreach (var additionalTile in additionalTiles)
        {
            // ... Set the additional tile in the correct position on the tilemap 
            additionalTilesPreview.GetComponent<Tilemap>().SetTile(additionalTile.position, additionalTile.tile);
        }
    }

    // Show a preview of the wall tiles in the room
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
                    // If the loop is at the bottom left corner of the room...
                    case 0 when x == 0:
                        // ... set the tile to be the room bottom left corner wall tile
                        wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[7]);
                        break;
                    // If the loop is between the bottom left corner and bottom right corner of the room...
                    case 0 when x < maxX:
                        // ... set the tile to be the room bottom wall tile
                        wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[4]);
                        break;
                    // If the loop is at the bottom right corner of the room...
                    case 0 when x == maxX:
                        // ... set the tile to be the room bottom right corner wall tile
                        wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[8]);
                        break;
                    // Otherwise...
                    default:
                        // If the loop x position is at the bottom of the room...
                        if (x == 0)
                        {
                            // ... set the tile to be the room left wall tile
                            wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[1]);
                        }

                        // If the loop x position is at the top of the room...
                        if (x == maxX)
                        {
                            // ... set the tile to be the room right wall tile
                            wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[2]);
                        }
                        break;
                }

                // If the loop is at the top left corner of the room...
                if (y == maxY && x == 0)
                    // ... set the tile to be the room top left corner wall tile
                    wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[5]);
                // If the loop is between the top left corner and top right corner of the room...
                else if (y == maxY && x < maxX)
                    // .. set the tile to be the room top wall tile
                    wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[3]);
                // If the loop is at the top right corner of the room...
                else if (y == maxY && x == maxX)
                    // ... set the tile to be the room top right corner wall tile
                    wallTiles.GetComponent<Tilemap>().SetTile(new Vector3Int(x, y, 0), tiles[6]);

            }
        }
    }

    // Show a preview of the floor tiles in the room
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

    // Get the tile palette for the room
    private void GetTilePalette()
    {
        // If there is already a tile palette saved...
        if (tiles.Count > 0)
        {
            // ... Clear the tile palette list
            tiles.Clear();
        }

        string path = "Assets/2DProceduralDungeonGenerator/Resources/TilePalette.txt";
        reader = new StreamReader(path);

        // While the text string is not empty...
        while ((text = reader.ReadLine()) != null)
        {
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
                    tiles.Add(asset);
                }
            }
        }
    }

    // Update the room prefab
    private void UpdateRoom()
    {
        // Find the room prefab in the asset database
        string[] results = AssetDatabase.FindAssets(roomName);

        foreach (var guid in results)
        {
            string roomPath = AssetDatabase.GUIDToAssetPath(guid);

            // Delete the room prefab
            AssetDatabase.DeleteAsset(roomPath);
        }
        
        // Create a new room
        room = new GameObject(roomName);
        room.AddComponent<Grid>();

        // Create floor tilemap for the room object
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

        // Create wall tilemap for the room object
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

        // Create tilemap for the additional tiles of the room object
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

        // If the folder doesn't exist..
        if (!AssetDatabase.IsValidFolder("Assets/2DProceduralDungeonGenerator/Prefabs/Rooms"))
        {
            // ... Create the folder
            AssetDatabase.CreateFolder("Assets/2DProceduralDungeonGenerator/Prefabs", "Rooms");
        }

        // Save the room as a prefab in the folder
        string localPath = "Assets/2DProceduralDungeonGenerator/Prefabs/Rooms/" + room.name + ".prefab";
        localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
        PrefabUtility.SaveAsPrefabAsset(room, localPath);

        // Destroy the room preview
        DestroyImmediate(room);

        // Close the editor window
        Close();
    }
}

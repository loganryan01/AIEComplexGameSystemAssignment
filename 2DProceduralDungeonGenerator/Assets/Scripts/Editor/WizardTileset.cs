/*-----------------------------------------------------
    File Name: WizardTileset.cs
    Purpose: Create a tile pallette for the room editor
    Author: Logan Ryan
    Modified: 14/05/2021
-------------------------------------------------------
    Copyright 2021 Logan Ryan
-----------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;
using System.IO;

public class WizardTileset : ScriptableWizard
{
    [Header("Room Tiles")]
    [Tooltip("The floor tile for the dungeon")]
    public TileBase floorTile;
    [Tooltip("The left wall tile for the room and vertical corridors")]
    public TileBase leftWallTile;
    [Tooltip("The right wall for the room and vertical corridors")]
    public TileBase rightWallTile;
    [Tooltip("The top wall for the room and horizontal corridors")]
    public TileBase topWallTile;
    [Tooltip("The bottom wall for the room and horizontal corridors")]
    public TileBase bottomWallTile;
    [Tooltip("The top left corner tile for the room")]
    public TileBase topLeftCornerWallTile;
    [Tooltip("The top right corner tile for the room")]
    public TileBase topRightCornerWallTile;
    [Tooltip("The bottom left corner tile for the room")]
    public TileBase bottomLeftCornerWallTile;
    [Tooltip("The bottom right corner tile for the room")]
    public TileBase bottomRightCornerWallTile;
    [Tooltip("The void for the dungeon")]
    public TileBase voidTile;

    [Header("Horizontal Corridor Tiles")]
    [Tooltip("The top left corner tile for horizontal corridor")]
    public TileBase horizontalCorridorTopLeftTile;
    [Tooltip("The top right corner tile for horizontal corridor")]
    public TileBase horizontalCorridorTopRightTile;
    [Tooltip("The bottom left corner tile for horizontal corridor")]
    public TileBase horizontalCorridorBottomLeftTile;
    [Tooltip("The bottom right corner tile for horizontal corridor")]
    public TileBase horizontalCorridorBottomRightTile;

    [Header("Vertical Corridor Tiles")]
    [Tooltip("The top left corner tile for vertical corridor")]
    public TileBase verticalCorridorTopLeftTile;
    [Tooltip("The top right corner tile for vertical corridor")]
    public TileBase verticalCorridorTopRightTile;
    [Tooltip("The bottom left corner tile for vertical corridor")]
    public TileBase verticalCorridorBottomLeftTile;
    [Tooltip("The bottom right corner tile for vertical corridor")]
    public TileBase verticalCorridorBottomRightTile;

    [Header("Optional Tile")]
    [Tooltip("The door tiles for the dungeon")]
    public TileBase doorTile;

    // Create scriptable wizard
    [MenuItem("DungeonGenerator/Set Tileset", false, 0)]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<WizardTileset>("Tileset", "Save");
    }

    private void OnWizardUpdate()
    {
        // If the scriptable wizard is missing a tile ...
        if (floorTile == null || leftWallTile == null || rightWallTile == null || topWallTile == null ||
            bottomWallTile == null || topLeftCornerWallTile == null || topRightCornerWallTile == null ||
            bottomLeftCornerWallTile == null || bottomRightCornerWallTile == null|| voidTile == null ||
            horizontalCorridorTopLeftTile == null || horizontalCorridorTopRightTile == null || horizontalCorridorBottomLeftTile == null ||
            horizontalCorridorBottomRightTile == null || verticalCorridorTopLeftTile == null || verticalCorridorTopRightTile == null ||
            verticalCorridorBottomLeftTile == null || verticalCorridorBottomRightTile == null)
        {
            // ... Write an error message saying that the wizard is missing tiles
            errorString = "Missing tiles";
        }
        else
        {
            errorString = "";
        }
    }

    private void OnWizardCreate()
    {
        string path = "Assets/2DProceduralDungeonGenerator/Resources/TilePalette.txt";
        string doorTileName = "Null";

        // If the user wants to have a door tile in the dungeon...
        if (doorTile != null)
        {
            // ... Get the name of the door tile
            doorTileName = doorTile.name;
        }

        string[] names = { floorTile.name,
                           leftWallTile.name,
                           rightWallTile.name,
                           topWallTile.name,
                           bottomWallTile.name,
                           topLeftCornerWallTile.name,
                           topRightCornerWallTile.name,
                           bottomLeftCornerWallTile.name,
                           bottomRightCornerWallTile.name,
                           voidTile.name,
                           horizontalCorridorTopLeftTile.name,
                           horizontalCorridorTopRightTile.name,
                           horizontalCorridorBottomLeftTile.name,
                           horizontalCorridorBottomRightTile.name,
                           verticalCorridorTopLeftTile.name,
                           verticalCorridorTopRightTile.name,
                           verticalCorridorBottomLeftTile.name,
                           verticalCorridorBottomRightTile.name,
                           doorTileName };

        // Write the names into a text file as a tile palette
        StreamWriter writer = new StreamWriter(path, false);
        foreach (string name in names)
        {
            writer.WriteLine(name);
        }

        writer.Close();

        // Save the text file 
        AssetDatabase.ImportAsset(path);
    }
}

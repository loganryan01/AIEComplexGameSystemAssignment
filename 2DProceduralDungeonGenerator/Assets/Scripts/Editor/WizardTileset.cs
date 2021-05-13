/*-----------------------------------------------------
    File Name: WizardTileset.cs
    Purpose: Create a tile pallette for the room editor
    Author: Logan Ryan
    Modified: 13/05/2021
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
    public TileBase floorTile;
    public TileBase leftWallTile;
    public TileBase rightWallTile;
    public TileBase topWallTile;
    public TileBase bottomWallTile;
    public TileBase topLeftCornerWallTile;
    public TileBase topRightCornerWallTile;
    public TileBase bottomLeftCornerWallTile;
    public TileBase bottomRightCornerWallTile;
    public TileBase voidTile;

    [Header("Horizontal Corridor Tiles")]
    public TileBase horizontalCorridorTopLeftTile;
    public TileBase horizontalCorridorTopRightTile;
    public TileBase horizontalCorridorBottomLeftTile;
    public TileBase horizontalCorridorBottomRightTile;

    [Header("Vertical Corridor Tiles")]
    public TileBase verticalCorridorTopLeftTile;
    public TileBase verticalCorridorTopRightTile;
    public TileBase verticalCorridorBottomLeftTile;
    public TileBase verticalCorridorBottomRightTile;

    [Header("Optional Tile")]
    public TileBase doorTile;

    [MenuItem("DungeonGenerator/Set Tileset", false, 0)]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<WizardTileset>("Tileset", "Save");
    }

    private void OnWizardUpdate()
    {
        if (floorTile == null)
            errorString = "There is no floor tile";
        else if (leftWallTile == null)
            errorString = "There is no left wall tile";
        else if (rightWallTile == null)
            errorString = "There is no right wall tile";
        else if (topWallTile == null)
            errorString = "There is no top wall tile";
        else if (bottomWallTile == null)
            errorString = "There is no bottom wall tile";
        else if (topLeftCornerWallTile == null)
            errorString = "There is no top left corner wall tile";
        else if (topRightCornerWallTile == null)
            errorString = "There is no top right corner wall tile";
        else if (bottomLeftCornerWallTile == null)
            errorString = "There is no bottom left corner wall tile";
        else if (bottomRightCornerWallTile == null)
            errorString = "There is no bottom right corner wall tile";
        else if (voidTile == null)
            errorString = "There is no void tile";
        else
            errorString = "";
    }

    private void OnWizardCreate()
    {
        string path = "Assets/Resources/TilePalette.txt";
        string doorTileName = "Null";

        if (doorTile != null)
        {
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

        StreamWriter writer = new StreamWriter(path, false);
        foreach (string name in names)
        {
            writer.WriteLine(name);
        }

        writer.Close();

        AssetDatabase.ImportAsset(path);
    }
}

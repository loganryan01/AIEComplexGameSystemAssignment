/*--------------------------------------------------------
    File Name: DungeonGeneratorEditor.cs
    Purpose: Add a button for the dungeon generator script
    Author: Logan Ryan
    Modified: 11/05/2021
----------------------------------------------------------
    Copyright 2021 Logan Ryan
--------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DungeonGenerator))]
public class DungeonGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DungeonGenerator dungeonGenerator = (DungeonGenerator)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Generate Dungeon"))
        {
            dungeonGenerator.GenerateDungeon();
        }
    }
}

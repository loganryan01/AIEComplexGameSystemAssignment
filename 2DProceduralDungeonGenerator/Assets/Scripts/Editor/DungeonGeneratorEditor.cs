/*--------------------------------------------------------
    File Name: DungeonGeneratorEditor.cs
    Purpose: Add a button for the dungeon generator script
    Author: Logan Ryan
    Modified: 13/05/2021
----------------------------------------------------------
    Copyright 2021 Logan Ryan
--------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LabeledArrayAttribute))]
public class LabeledArrayDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label);
    }

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(rect, label, property);
        try
        {
            char[] charArr = label.text.ToCharArray();
            int number = charArr[8] - 47;
            string name = "Room " + number.ToString();
            EditorGUI.PropertyField(rect, property, new GUIContent(name), true);
        }
        catch
        {
            EditorGUI.PropertyField(rect, property, label, true);
        }
        EditorGUI.EndProperty();
    }
}

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

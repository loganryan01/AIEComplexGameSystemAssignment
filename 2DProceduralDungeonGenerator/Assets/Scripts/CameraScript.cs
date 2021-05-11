/*----------------------------------------------
    File Name: CameraScript.cs
    Purpose: Position the camera near the player
    Author: Logan Ryan
    Modified: 11/05/2021
------------------------------------------------
    Copyright 2021 Logan Ryan
----------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject player;   // Player object
    public Vector3 offset;      // Offset from the player

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position + offset;
    }
}

/*-----------------------------------------
    File Name: MainMenuScript.cs
    Purpose: Control the menu's in the game
    Author: Logan Ryan
    Modified: 14/05/2021
-------------------------------------------
    Copyright 2021 Logan Ryan
-----------------------------------------*/
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    // Start the game
    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }

    // Quit the game
    public void QuitGame()
    {
        Application.Quit();
    }
}

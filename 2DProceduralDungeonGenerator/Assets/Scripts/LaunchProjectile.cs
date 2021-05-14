/*----------------------------------------------
    File Name: LaunchProjectile.cs
    Purpose: Launches a projectile at the player
    Author: Logan Ryan
    Modified: 14/05/2021
------------------------------------------------
    Copyright 2021 Logan Ryan
----------------------------------------------*/
using UnityEngine;

public class LaunchProjectile : MonoBehaviour
{
    public GameObject projectilePrefab;     // The projectile that the object is firing
    public float waitTime;                  // How long it takes for the object to recharge before firing again

    float currentTime;                      // How long the object has waited

    // Start is called before the first frame update
    private void Start()
    {
        currentTime = waitTime;
    }

    // Update is called once per frame
    void Update()
    {
        // If the enemy can see the player
        if (GetComponent<EnemyController>().seePlayer)
        {
            // Increase the current time
            currentTime += Time.deltaTime;

            // If the current time is greater than or equal to the wait time
            if (currentTime >= waitTime)
            {
                // Launch a projectile at the player
                Instantiate(projectilePrefab, transform.position, Quaternion.identity);

                // Reset the timer
                currentTime = 0;
            }
        }
    }

    private void OnDestroy()
    {
        // When the enemy gets destroyed, destroy all the projectiles
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");

        foreach (var projectile in projectiles)
        {
            Destroy(projectile);
        }
    }
}

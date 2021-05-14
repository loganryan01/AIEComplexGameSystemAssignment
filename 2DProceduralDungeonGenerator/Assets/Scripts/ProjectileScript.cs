/*-----------------------------------------------------
    File Name: ProjectileScript.cs
    Purpose: Control the projectile that has been fired
    Author: Logan Ryan
    Modified: 14/05/2021
-------------------------------------------------------
    Copyright 2021 Logan Ryan
-----------------------------------------------------*/
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    GameObject target;      // The target for the projectile
    float speed = 2;        // The speed of the projectile
    
    // Start is called before the first frame update
    void Start()
    {
        // Set the target to be the player
        target = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        // Move towards the player
        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the projectile collides with the player...
        if (collision.gameObject.CompareTag("Player"))
        {
            // ... destroy the projectile
            Destroy(gameObject);
        }
    }
}

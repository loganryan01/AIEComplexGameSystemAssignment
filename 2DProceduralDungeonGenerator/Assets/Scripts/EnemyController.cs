/*------------------------------------------
    File Name: EnemyController.cs
    Purpose: Control the enemy's in the game
    Author: Logan Ryan
    Modified: 14/05/2021
--------------------------------------------
    Copyright 2021 Logan Ryan
------------------------------------------*/
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 10.0f;     // Speed of the enemy
    public bool hasKey;             // Does the enemy have the key
    [HideInInspector]
    public bool seePlayer = false;  // Can the enemy see the player?
    private GameObject player;      // Player object
    private Rigidbody2D rb;         // Enemy rigidbody
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the player object and the enemy's rigidbody
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;

        // If the enemy can see the player, then move towards the player
        if (seePlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, step);
        }

        // If the enemy is moving left, then the enemy's sprite will then flip to face left
        if (rb.velocity.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else
        {
            GetComponent<SpriteRenderer>().flipX = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If the player collides with the player...
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = player.GetComponent<PlayerController>();

            // ... and they don't have the sword and is not invincible ...
            if (playerController.swordEquipped == false && !playerController.invincible)
            {
                // ... the player loses 1 life and become invincible
                PlayerStats.Instance.TakeDamage(1.0f);
                StartCoroutine(playerController.Invincible());
            }
            else if (playerController.swordEquipped)
            {
                // If the player does have the sword equipped then destroy the enemy
                if (hasKey)
                {
                    // If the enemy does have a key then give the key to the player
                    playerController.hasKey = true;
                }
                
                Destroy(gameObject);
            }
        }
    }
}

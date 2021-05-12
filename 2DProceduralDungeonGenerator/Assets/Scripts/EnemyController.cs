/*------------------------------------------
    File Name: EnemyController.cs
    Purpose: Control the enemy's in the game
    Author: Logan Ryan
    Modified: 12/05/2021
--------------------------------------------
    Copyright 2021 Logan Ryan
------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float speed = 10.0f;
    [HideInInspector]
    public bool seePlayer = false;
    private GameObject player;
    private Rigidbody2D rb;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        float step = speed * Time.deltaTime;

        if (seePlayer)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.transform.position, step);
        }

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
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController playerController = player.GetComponent<PlayerController>();

            if (playerController.swordEquipped == false && !playerController.invincible)
            {
                Debug.Log("Lose 1 life");
                PlayerStats.Instance.TakeDamage(1.0f);
                StartCoroutine(playerController.Invincible());
            }
            else if (playerController.swordEquipped)
            {
                Destroy(gameObject);
            }
        }
    }
}

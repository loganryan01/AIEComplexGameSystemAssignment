using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchProjectile : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float waitTime;

    float currentTime;

    private void Start()
    {
        currentTime = waitTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<EnemyController>().seePlayer)
        {
            currentTime += Time.deltaTime;

            if (currentTime >= waitTime)
            {
                Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                currentTime = 0;
            }
        }
    }

    private void OnDestroy()
    {
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");

        foreach (var projectile in projectiles)
        {
            Destroy(projectile);
        }
    }
}

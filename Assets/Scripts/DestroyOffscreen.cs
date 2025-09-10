using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyIfAbovePlayer : MonoBehaviour
{
    public float offsetAbove = 30f;
    private Transform player;
    private float searchTimer = 0f;
    private float maxSearchTime = 10f;

void Update()
{
    if (player == null)
    {
        searchTimer += Time.deltaTime;
        if (searchTimer < maxSearchTime)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        return;
    }

    if (transform.position.y > player.position.y + offsetAbove)
    {
        Destroy(gameObject);
    }
}
}
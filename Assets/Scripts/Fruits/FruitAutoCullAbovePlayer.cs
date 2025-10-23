using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitAutoCullAbovePlayer : MonoBehaviour
{
    public float offsetAbove = 40f;
    private Transform player;
    void Update()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
            return;
        }
        if (transform.position.y > player.position.y + offsetAbove)
            Destroy(gameObject);
    }
} 

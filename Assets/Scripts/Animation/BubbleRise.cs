using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleRise : MonoBehaviour
{
    public float speed = 2f;
    public float lifeTime = 2f;

    void Start()
    {
        GetComponent<Rigidbody2D>().velocity = Vector2.up * speed;
        Destroy(gameObject, lifeTime);
    }
}

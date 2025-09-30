using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class MedusaController : MonoBehaviour
{
    public float speed = 2f;
    public float destroyHeight = 30f;

    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // медуза не тонет
        rb.mass = 0.2f;      // легче капибары
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // чтобы не вертелась при ударе

        // Ищем игрока (капибару)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        // Двигаем строго вверх через физику
        rb.velocity = new Vector2(0, speed);

        // Уничтожаем, если далеко выше игрока
        if (player != null && transform.position.y > player.position.y + destroyHeight)
        {
            Destroy(gameObject);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class MedusaController : MonoBehaviour
{
    public float speed = 2f;
    public float destroyHeight = 30f;

    private Transform player;

    void Start()
    {
        // Ищем игрока (капибару)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {       // Двигаем строго вверх
        transform.Translate(Vector2.up * speed * Time.deltaTime);

        // Если есть игрок - уничтожаем объект, когда он далеко выше игрока 
        if (player != null && transform.position.y > player.position.y + destroyHeight)
        {
            Destroy(gameObject);
        }
    }
}

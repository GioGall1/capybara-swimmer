using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedusaController : MonoBehaviour
{
    public float speed = 2f;
    public float destroyHeight = 30f;

    [Header("Breath damage")]
    public float breathDamage = 10f;     // сколько процентов снимаем за удар
    public float hitCooldown = 0.75f;    // задержка между повторными списаниями, сек
    private float _lastHitTime = -999f;

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
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        TryApplyBreathDamage();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        TryApplyBreathDamage();
    }

    private void TryApplyBreathDamage()
    {
        if (Time.time < _lastHitTime + hitCooldown) return;

        // Пытаемся найти менеджер дыхания на сцене или на игроке (через синглтон)
        BreathingManager breath = BreathingManager.Instance;
        if (breath == null)
        {
            // как запасной вариант, попробуем найти на объекте игрока по тегу
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) breath = playerObj.GetComponent<BreathingManager>();
        }

        if (breath != null)
        {
            breath.ChangeBreath(-breathDamage);
            _lastHitTime = Time.time;

            // 🫧 Выпускаем пузыри при столкновении с медузой
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                var swim = playerObj.GetComponent<CapybaraSwimController>();
                if (swim != null)
                {
                    swim.SpawnHitBubbles(8); // количество пузырей, можно регулировать
                }
            }
        }
        else
        {
            Debug.LogWarning("[MedusaController] Не найден BreathingManager для списания воздуха");
        }
    }
}
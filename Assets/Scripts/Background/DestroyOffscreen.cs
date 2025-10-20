// DestroyIfAbovePlayer.cs
using UnityEngine;

public class DestroyIfAbovePlayer : MonoBehaviour
{
    public float offsetAbove = 30f;
    private static Transform player; // кэш на всех чанках

    private void Awake()
    {
        // пробуем найти сразу
        if (player == null)
        {
            var po = GameObject.FindGameObjectWithTag("Player");
            if (po) player = po.transform;
        }
    }

    private void Update()
    {
        // если вдруг потеряли ссылку — пытаемся снова (без таймера)
        if (player == null)
        {
            var po = GameObject.FindGameObjectWithTag("Player");
            if (po) player = po.transform;
            return;
        }

        // если чанк стал выше игрока на offset — удаляем
        if (transform.position.y > player.position.y + offsetAbove)
        {
            Destroy(gameObject);
        }
    }
}
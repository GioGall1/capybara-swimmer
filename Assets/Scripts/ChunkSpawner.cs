using UnityEngine;

public class ChunkSpawner : MonoBehaviour
{
    public GameObject chunkPrefab;
    public Vector2 spawnOffset = new Vector2(0f, -100f);

    private bool hasSpawned = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasSpawned && other.CompareTag("Player"))
        {
            hasSpawned = true;

            // Позиция родительского чанка
            Vector2 currentChunkPos = transform.parent != null 
                ? (Vector2)transform.parent.position 
                : (Vector2)transform.position;

            // Новая позиция: ниже текущей
            Vector2 spawnPos = currentChunkPos + spawnOffset;

            // Спавним новый чанк
            Instantiate(chunkPrefab, spawnPos, Quaternion.identity);

            // Лог в консоль
            Debug.Log($"[ChunkSpawner] Создан чанк на позиции: {spawnPos}");
        }
    }
}
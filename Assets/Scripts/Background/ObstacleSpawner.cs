using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public Transform spawnArea; // твой ObstacleSpawnArea
    public int numberOfObstacles = 5;

    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty;

    void Start()
    {
        SpawnObstacles();
    }

    void SpawnObstacles()
    {
        // Получаем ширину и центр области спавна
        float width = spawnArea.localScale.x;
        float height = spawnArea.localScale.y;
        Vector2 center = spawnArea.position;

        // Настраиваем сложность
        int maxObstacles = GetObstacleCountByDifficulty();
        int passageIndex = Random.Range(0, numberOfObstacles); // Оставим одну “дырку”

        float stepX = width / numberOfObstacles;
        float startX = center.x - width / 2f;

        for (int i = 0; i < numberOfObstacles; i++)
        {
            if (i == passageIndex) continue; // здесь будет проход

            if (i >= maxObstacles) break; // ограничение по сложности

            float spawnX = startX + stepX * i + stepX / 2f;
            float spawnY = center.y;

            Vector2 spawnPos = new Vector2(spawnX, spawnY);
            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

            Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        }
    }

    int GetObstacleCountByDifficulty()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy: return 2;
            case Difficulty.Medium: return 3;
            case Difficulty.Hard: return 4;
            default: return 2;
        }
    }
}
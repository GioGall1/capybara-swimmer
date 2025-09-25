using UnityEngine;

public class BackgroundSpawner : MonoBehaviour
{
    public Transform backgroundSpawnPoint;
    public GameObject[] backgroundPrefabs;

    void Start()
    {
        SpawnBackground(); // Вызов прямо при старте
    }

    public void SpawnBackground()
    {
        if (backgroundPrefabs.Length == 0 || backgroundSpawnPoint == null)
        {
            Debug.LogWarning("Нет префабов или точки спавна!");
            return;
        }

        int index = Random.Range(0, backgroundPrefabs.Length);
        GameObject background = Instantiate(backgroundPrefabs[index]);

        background.transform.position = backgroundSpawnPoint.position;

        // ВРЕМЕННО: задай свой размер
        background.transform.localScale = new Vector3(4.5f, 3.8f, 1f);

        Debug.Log("Заспавнили фон: " + background.name);
    }
}
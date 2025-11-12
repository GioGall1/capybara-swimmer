using UnityEngine;

public class SeaweedSpawner : MonoBehaviour
{
    [Range(0f, 1f)]
    public float spawnChance = 0.3f; // шанс появления водорослей
    public GameObject seaweedPrefab;

    void Start()
    {
        TrySpawnSeaweed();
    }

    private void TrySpawnSeaweed()
    {
        // шанс появления
        if (Random.value > spawnChance) return;

        Transform spawnPoint = transform.Find("SeaweedSpawnPoint");
        if (spawnPoint == null || seaweedPrefab == null) return;

        // создаем водоросль
        GameObject seaweed = Instantiate(seaweedPrefab, spawnPoint.position, Quaternion.identity, transform);

        // случайный размер
        float scale = Random.Range(0.8f, 1.5f);
        seaweed.transform.localScale = new Vector3(scale, scale, 1f);

        // добавляем покачивание
        var swing = seaweed.AddComponent<SeaweedSwing>();
        swing.swingSpeed = Random.Range(0.5f, 1.2f);
        swing.swingAmplitude = Random.Range(5f, 15f);
    }
}
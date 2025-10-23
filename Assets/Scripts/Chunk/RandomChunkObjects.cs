using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomChunkObjects : MonoBehaviour
{
    public GameObject coconutPrefab;
    public GameObject bubblePrefab;
    public BoxCollider2D waterArea;

    void Start()
    {
        ClearPreviousObjects();

        TrySpawn(coconutPrefab, 0, 6);  // от 0 до 6 кокосов
        TrySpawnOnce(bubblePrefab, 0.50f);  // 50% шанс и только один
    }

    void TrySpawn(GameObject prefab, int minAmount, int maxAmount)
    {
        if (waterArea != null)
        {
            int amount = GetCoconutCountByProbability();
            Bounds bounds = waterArea.bounds;

            for (int i = 0; i < amount; i++)
            {
                Vector3 basePos = GetRandomPointInBounds(bounds);
                Vector3 randomPos = new Vector3(basePos.x, basePos.y, prefab.transform.position.z);
                GameObject obj = Instantiate(prefab, randomPos, Quaternion.identity);
                obj.transform.SetParent(transform);
            }
        }
    }

    void TrySpawnOnce(GameObject prefab, float chance)
    {
        if (Random.value < chance && waterArea != null)
        {
            Bounds bounds = waterArea.bounds;

            Vector3 basePos = GetRandomPointInBounds(bounds);
            Vector3 randomPos = new Vector3(basePos.x, basePos.y, prefab.transform.position.z);
            GameObject obj = Instantiate(prefab, randomPos, Quaternion.identity);
            obj.transform.SetParent(transform);
        }
    }

    void ClearPreviousObjects()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    Vector3 GetRandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(-0.5f, 0.5f) // легкое Z-смещение для предотвращения наложений
        );
    }

    int GetCoconutCountByProbability()
    {
        float roll = Random.value;

        if (roll < 0.15f) return 0;       // 15% шанс
        else if (roll < 0.65f) return 2;  // 50% шанс
        else return 3;                    // 35% шанс (оставшиеся)
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedusaSpawner : MonoBehaviour
{
    [Header("Префаб медузы")]
    public GameObject medusaPrefab;

    [Header("Зона спавна (пустышка с BoxCollider2D)")]
    public Transform medusaSpawnZone;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (medusaPrefab == null || medusaSpawnZone == null) return;

            BoxCollider2D zoneCollider = medusaSpawnZone.GetComponent<BoxCollider2D>();
            if (zoneCollider != null)
            {
                // случайная точка по линии нижней границы
                float randomX = Random.Range(zoneCollider.bounds.min.x, zoneCollider.bounds.max.x);
                float spawnY = zoneCollider.bounds.min.y;

                Vector2 spawnPos = new Vector2(randomX, spawnY);

                Instantiate(medusaPrefab, spawnPos, Quaternion.identity);
                Debug.Log($"[MedusaSpawner] Медуза заспавнена в точке: {spawnPos}");
            }
        }
    }
}
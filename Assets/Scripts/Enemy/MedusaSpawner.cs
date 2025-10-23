using System.Collections.Generic;
using UnityEngine;

public class MedusaSpawner : MonoBehaviour
{
    [Header("Префаб медузы")]
    public GameObject medusaPrefab;

    [Header("Зона спавна (пустышка с BoxCollider2D)")]
    public Transform medusaSpawnZone; // зона может быть этим же объектом или дочерним

    [Header("Количество медуз на спавн")]
    public int minCount = 1;
    public int maxCount = 5;

    [Header("Пауза между срабатываниями (сек)")]
    public float cooldown = 0f; // 0 = без задержки

    [Header("Ограничения")]
    public bool spawnOnce = true;      // true = один раз на этот чанк
    public float minSeparation = 0.75f; // минимальная дистанция между точками появления по X
    public float yOffset = 0.05f;      // небольшой сдвиг вверх, чтобы не застревать в линии

    [Header("Пакетный спавн c интервалом")]
    public float staggerDelay = 1f; // задержка между медузами в одном пакете

    private bool _spawned = false;
    private float _lastSpawnTime = -9999f;
    private bool _spawning = false; // идёт ли уже процесс спавна (корутина)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (spawnOnce && (_spawned || _spawning)) return; // уже спавнили или запланированно
        if (Time.time < _lastSpawnTime + cooldown) return;

        StartCoroutine(SpawnRoutine());
    }

    private System.Collections.IEnumerator SpawnRoutine()
    {
        _spawning = true;

        if (medusaPrefab == null)
        {
            Debug.LogWarning("[MedusaSpawner] medusaPrefab is null");
            _spawning = false; yield break;
        }

        Transform zoneT = medusaSpawnZone != null ? medusaSpawnZone : transform;
        BoxCollider2D box = zoneT.GetComponent<BoxCollider2D>();
        if (box == null)
        {
            Debug.LogWarning("[MedusaSpawner] medusaSpawnZone has no BoxCollider2D");
            _spawning = false; yield break;
        }

        int count = Mathf.Clamp(Random.Range(minCount, maxCount + 1), 1, 100);

        // подготовим точки появления
        Bounds b = box.bounds;
        float y = b.min.y;
        float minX = b.min.x;
        float maxX = b.max.x;

        List<float> xs = new List<float>(count);
        int safety = 0;
        while (xs.Count < count && safety < 500)
        {
            safety++;
            float candidate = Random.Range(minX, maxX);
            bool ok = true;
            for (int i = 0; i < xs.Count; i++)
            {
                if (Mathf.Abs(xs[i] - candidate) < minSeparation) { ok = false; break; }
            }
            if (ok) xs.Add(candidate);
        }
        if (xs.Count < count)
        {
            xs.Clear();
            if (count == 1) xs.Add((minX + maxX) * 0.5f);
            else for (int i = 0; i < count; i++) { float t = i / (float)(count - 1); xs.Add(Mathf.Lerp(minX, maxX, t)); }
        }

        // спавним по одному с интервалом
        for (int i = 0; i < xs.Count; i++)
        {
            Vector3 spawnPos = new Vector3(xs[i], y, 0f) + Vector3.up * yOffset;
            GameObject medusa = Instantiate(medusaPrefab, spawnPos, Quaternion.identity);
            medusa.transform.SetParent(transform.root, true);

            if (i < xs.Count - 1 && staggerDelay > 0f)
                yield return new WaitForSeconds(staggerDelay);
        }

        Debug.Log($"[MedusaSpawner] Spawned {xs.Count} medusas (stagger {staggerDelay}s)");

        _spawned = true;
        _lastSpawnTime = Time.time;
        _spawning = false;

        if (spawnOnce)
        {
            // больше не срабатывать: отключим коллайдер или сам компонент
            var col = GetComponent<Collider2D>();
            if (col) col.enabled = false; else this.enabled = false;
        }
    }
}
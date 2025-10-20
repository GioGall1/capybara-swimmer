using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChunkSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Prefab чанка (используется, если useResources = false)")]
    public GameObject chunkPrefab;

    [Tooltip("Если включено — префаб будет загружен из папки Resources по пути ниже")]
    public bool useResources = false;

    [Tooltip("Путь внутри Resources без расширения, например 'Prefabs/Chunk/Chunk_100'")]
    public string resourcesPath = "";

    [Tooltip("Если несколько префабов — перечисли пути внутри Resources. Если пусто, будет использован 'resourcesPath'.")]
    public string[] resourcePaths;

    [Tooltip("Не повторять один и тот же чанк подряд при случайном выборе из списка")] 
    public bool avoidImmediateRepeat = true;

    private GameObject _lastPrefabAsset;

    public Vector2 spawnOffset = new Vector2(0f, -100f);
    public bool spawnNextOnStart = false;
    [SerializeField] private float immediateCleanupMargin = 5f;

    private bool hasSpawned = false;

    private void Awake()
    {
        // 🔹 если включён режим Resources — проверим входные данные
        if (useResources)
        {
            bool hasList = resourcePaths != null && resourcePaths.Length > 0;
            bool hasSingle = !string.IsNullOrEmpty(resourcesPath);

            if (!hasList && !hasSingle)
            {
                Debug.LogError("[ChunkSpawner] useResources=ON, но не задан ни Resources Path, ни список resourcePaths.");
                enabled = false; return;
            }

            // Если задан только одиночный путь — предварительно загрузим, чтобы отловить опечатки раньше
            if (!hasList && hasSingle)
            {
                var loaded = Resources.Load<GameObject>(resourcesPath);
                if (loaded == null)
                {
                    Debug.LogError($"[ChunkSpawner] Resources.Load('{resourcesPath}') вернул null. Проверь путь и расположение префаба под Assets/Resources.");
                    enabled = false; return;
                }
                chunkPrefab = loaded; // сохраним как дефолт
                _lastPrefabAsset = null; // сброс
                Debug.Log($"[ChunkSpawner] Загружен префаб из Resources: {resourcesPath}");
            }
        }
        else
        {
            // 🔹 режим с прямой ссылкой на asset-префаб
            if (chunkPrefab == null)
            {
                Debug.LogError("[ChunkSpawner] Chunk Prefab не задан!");
                enabled = false; return;
            }
            if (chunkPrefab.scene.IsValid())
            {
                Debug.LogError("[ChunkSpawner] 'Chunk Prefab' указывает на объект сцены. Используй prefab-asset или включи useResources.");
                enabled = false; return;
            }
        }
    }

    private void OnEnable() => hasSpawned = false;

    private void Start()
    {
        if (spawnNextOnStart)
            SpawnNextChunk();
    }

    private GameObject PickPrefabAsset()
    {
        if (useResources)
        {
            // Если задан список путей – выбираем случайный, избегая мгновенного повтора
            if (resourcePaths != null && resourcePaths.Length > 0)
            {
                int tries = Mathf.Max(1, resourcePaths.Length * 2);
                for (int a = 0; a < tries; a++)
                {
                    int idx = Random.Range(0, resourcePaths.Length);
                    var path = resourcePaths[idx];
                    if (string.IsNullOrEmpty(path)) continue;
                    var loaded = Resources.Load<GameObject>(path);
                    if (loaded == null) continue;
                    if (avoidImmediateRepeat && _lastPrefabAsset == loaded && resourcePaths.Length > 1)
                        continue; // попробуем другой
                    _lastPrefabAsset = loaded;
                    return loaded;
                }
                // Фолбэк: вернём первое валидное
                foreach (var path in resourcePaths)
                {
                    var loaded = string.IsNullOrEmpty(path) ? null : Resources.Load<GameObject>(path);
                    if (loaded != null) { _lastPrefabAsset = loaded; return loaded; }
                }
                return null;
            }
            // Иначе – одиночный путь
            return Resources.Load<GameObject>(resourcesPath);
        }
        // Режим без Resources – используем заданный asset
        return chunkPrefab;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasSpawned && other.CompareTag("Player"))
            SpawnNextChunk();
    }

    private void SpawnNextChunk()
    {
        hasSpawned = true;

        Vector2 currentChunkPos = transform.parent ? (Vector2)transform.parent.position : (Vector2)transform.position;
        Vector2 spawnPos = currentChunkPos + spawnOffset;

        // Выбираем prefab-ассет (из списка или по одиночному пути/полю)
        var prefabToSpawn = PickPrefabAsset();
        if (prefabToSpawn == null || prefabToSpawn.scene.IsValid())
        {
            Debug.LogError("[ChunkSpawner] Спавн отменён — не удалось выбрать валидный prefab-ассет.");
            return;
        }

        CleanupOldChunks(currentChunkPos.y);

        GameObject newChunk = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        int chunkId = newChunk.GetInstanceID();

        #if UNITY_EDITOR
        var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(newChunk);
        Debug.Log($"[ChunkSpawner] Создан чанк {chunkId} из '{path}' на позиции {spawnPos}");
        #else
        Debug.Log($"[ChunkSpawner] Создан чанк {chunkId} на позиции {spawnPos}");
        #endif

        // Спавним фрукты
        var fruitZones = newChunk.GetComponentsInChildren<FruitLineZoneTrigger>(true);
        foreach (var zone in fruitZones)
        {
            if (!zone) continue;
            zone.spawnOnPlayerEnter = false;
            zone.spawnOnlyOnce = true;
            zone.deterministic = false;
            zone.usePerChunkRandom = true;
            zone.ForceSpawn(chunkId);
        }
    }

    private void CleanupOldChunks(float currentChunkY)
    {
        var chunks = FindObjectsOfType<DestroyIfAbovePlayer>(true);
        int removed = 0;

        foreach (var ch in chunks)
        {
            if (!ch) continue;
            if (ch.transform.position.y > currentChunkY + immediateCleanupMargin)
            {
                Destroy(ch.gameObject);
                removed++;
            }
        }

        if (removed > 0)
            Debug.Log($"[ChunkSpawner] Cleanup: удалено старых чанков = {removed}");
    }
}
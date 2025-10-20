using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class FruitLineZoneTrigger : MonoBehaviour
{
    public enum LineOrientation { Vertical, DiagonalNE, DiagonalNW }
    public enum FruitSelectionMode { RandomPerLine, FixedIndex }

    [Header("Что спавним (один тип на линию)")]
    public List<GameObject> fruitPrefabs = new List<GameObject>();
    public FruitSelectionMode fruitSelectMode = FruitSelectionMode.RandomPerLine;
    public int fixedFruitIndex = 0;

    [Header("Линии: количество и длина")]
    [Range(1, 8)] public int minLines = 1;
    [Range(1, 8)] public int maxLines = 4;
    [Range(3, 50)] public int minCountPerLine = 8;
    [Range(3, 50)] public int maxCountPerLine = 14;

    [Header("Расстояние между фруктами в линии")]
    public float spacingMin = 0.6f;
    public float spacingMax = 1.4f;

    [Header("Какие ориентации разрешены")]
    public bool allowVertical = true;
    public bool allowDiagonalNE = true;
    public bool allowDiagonalNW = true;

    [Header("Поведение")]
    public bool spawnOnlyOnce = true;
    public Transform parentForFruits;
    [Tooltip("Если включено — зона спавнит по входу игрока. По умолчанию спавним принудительно из ChunkSpawner")] 
    public bool spawnOnPlayerEnter = false;

    [Header("Опционально: детерминированность")]
    public bool deterministic = false;
    public bool usePerChunkRandom = true;
    public int chunkId = 0;
    public int seedOffset = 987654321;

    private bool _spawned;
    private bool _spawning;
    private BoxCollider2D _box;
    private readonly List<GameObject> _spawnedList = new List<GameObject>();
    private System.Random _rng;

    private void Awake()
    {
        _box = GetComponent<BoxCollider2D>();
        EnsureParent();
        if (_box != null) _box.isTrigger = true;
    }

    private void EnsureParent()
    {
        if (parentForFruits == null)
        {
            var root = GetComponentInParent<DestroyIfAbovePlayer>();
            parentForFruits = root ? root.transform : transform.root;
        }
    }

    private void InitRng()
{
    // 1) Полностью детерминированный режим — оставляем как есть
    if (deterministic)
    {
        _rng = new System.Random(chunkId + seedOffset);
        return;
    }

    // 2) Пер-чанковая случайность БЕЗ привязки ко времени
    if (usePerChunkRandom)
    {
        unchecked
        {
            // Хешируем только стабильные вещи:
            // - уникальный chunkId, который мы передаём из ChunkSpawner (GetInstanceID нового чанка)
            // - instanceID самой зоны (чтобы разные зоны в чанке давали разные последовательности)
            int seed = 146959810;              // старт (произвольная константа)
            seed = seed * 16777619 ^ chunkId;  // чанк уникализирует сид
            seed = seed * 16777619 ^ GetInstanceID();
            seed = seed * 16777619 ^ Mathf.RoundToInt(transform.position.x * 100f);
            seed = seed * 16777619 ^ Mathf.RoundToInt(transform.position.y * 100f);
            // ВАЖНО: НИКАКОГО Environment.TickCount, никаких кадров/времени
            _rng = new System.Random(seed);
        }
        return;
    }

    // 3) Фолбэк на UnityEngine.Random (если оба флага выключены)
    _rng = null;
}

    public void ForceSpawn(int? overrideChunkId = null)
    {
        if (_spawning) return;

        _spawning = true;
        _spawned  = false; // разрешаем новый прогон независимо от прошлых срабатываний
        if (overrideChunkId.HasValue) chunkId = overrideChunkId.Value;
        InitRng();
        EnsureParent();
        ClearSpawned();  // ВАЖНО: чистим старые фрукты, если зона переиспользуется
        Debug.Log($"[FruitZone:{name}] ForceSpawn -> chunkId={chunkId}, prefabs={fruitPrefabs?.Count ?? 0}");
        SpawnLines();
        _spawned = true;
        _spawning = false;
    }

    private void OnDisable()
    {
        ClearSpawned();
        _spawning = false;
    }

    private void OnDestroy()
    {
        // Страховка: если зону/чанк уничтожают напрямую — удалить все заспавнённые фрукты
        ClearSpawned();
    }

    private void ClearSpawned()
    {
        for (int i = _spawnedList.Count - 1; i >= 0; i--)
            if (_spawnedList[i]) Destroy(_spawnedList[i]);
        _spawnedList.Clear();
    }

    private void OnEnable()
    {
        _spawned = false;
        _spawning = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!spawnOnPlayerEnter) return;           // по умолчанию выключено — спавним из ChunkSpawner
        if (!other.CompareTag("Player")) return;
        if (_spawning || (_spawned && spawnOnlyOnce)) return;

        _spawning = true;
        InitRng();
        EnsureParent();
        ClearSpawned();
        SpawnLines();
        _spawned = true;
        _spawning = false;
    }

    // === Validation helpers ===
    private bool IsPrefabAsset(GameObject go)
    {
        if (go == null) return false;
        // Если объект принадлежит сцене, это не ассет-перефаб
        return !go.scene.IsValid();
    }

    private List<GameObject> BuildValidPrefabList()
    {
        var valid = new List<GameObject>();
        if (fruitPrefabs == null)
        {
            Debug.LogWarning($"[FruitZone:{name}] fruitPrefabs == null");
            return valid;
        }
        for (int i = 0; i < fruitPrefabs.Count; i++)
        {
            var fp = fruitPrefabs[i];
            if (fp == null)
            {
                Debug.LogWarning($"[FruitZone:{name}] fruitPrefabs[{i}] == null — пропущен");
                continue;
            }
            if (!IsPrefabAsset(fp))
            {
                Debug.LogWarning($"[FruitZone:{name}] fruitPrefabs[{i}] указывает на сценовый объект (не ассет). Перетащи ПРЕФАБ из Project (синяя иконка). Объект: {fp.name}");
                continue;
            }
            valid.Add(fp);
        }
        return valid;
    }

    private void SpawnLines()
    {
        if (_box == null) return;

        // Строим валидный список префабов (только ассеты из Project)
        var validPrefabs = BuildValidPrefabList();
        if (validPrefabs.Count == 0)
        {
            Debug.LogWarning($"[FruitZone:{name}] Нет валидных fruitPrefabs — спавн пропущен");
            return;
        }

        var orientations = CollectAllowedOrientations();
        Bounds b = _box.bounds;
        int lines = Mathf.Clamp(RandRangeInt(minLines, maxLines), 1, 999);
        Debug.Log($"[FruitZone:{name}] lines={lines} bounds.center={b.center} size={b.size}");

        for (int l = 0; l < lines; l++)
        {
            var o = orientations[RandRangeInt(0, orientations.Count - 1)];
            int count = Mathf.Clamp(RandRangeInt(minCountPerLine, maxCountPerLine), 1, 999);
            float spacing = Mathf.Max(0.01f, RandRangeFloat(spacingMin, spacingMax));

            var fruitPrefab = PickFruitPrefabForLine(validPrefabs);
            if (fruitPrefab == null) continue;

            SpawnOneLine(b, o, count, spacing, fruitPrefab);
        }
    }

    private GameObject PickFruitPrefabForLine(List<GameObject> validPrefabs)
    {
        if (validPrefabs == null || validPrefabs.Count == 0) return null;
        if (fruitSelectMode == FruitSelectionMode.FixedIndex)
        {
            if (fixedFruitIndex < 0 || fixedFruitIndex >= validPrefabs.Count) return null;
            return validPrefabs[fixedFruitIndex];
        }
        return validPrefabs[RandRangeInt(0, validPrefabs.Count - 1)];
    }

    private void SpawnOneLine(Bounds b, LineOrientation o, int count, float spacing, GameObject fruitPrefab)
    {
        Vector2 dir = GetDir(o).normalized;
        float total = spacing * (count - 1);
        float dx = Mathf.Abs(dir.x) * total;
        float dy = Mathf.Abs(dir.y) * total;

        float minX = (float)(b.min.x + dx * 0.5f);
        float maxX = (float)(b.max.x - dx * 0.5f);
        float minY = (float)(b.min.y + dy * 0.5f);
        float maxY = (float)(b.max.y - dy * 0.5f);

        if (minX > maxX || minY > maxY) return;

        Vector2 center = new Vector2(
            RandRangeFloat(minX, maxX),
            RandRangeFloat(minY, maxY)
        );
        Vector2 start = center - dir * (total * 0.5f);
        Debug.Log($"[FruitZone:{name}] line center={center} count={count} spacing={spacing}");

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = start + dir * (i * spacing);
            if (fruitPrefab == null)
            {
                Debug.LogWarning($"[FruitZone:{name}] fruitPrefab == null — линия пропущена");
                return;
            }
            var go = Instantiate(fruitPrefab, pos, Quaternion.identity, parentForFruits);

            // 1) Гарантируем корректного родителя (должен быть корень чанка)
            if (go.transform.parent != parentForFruits)
                go.transform.SetParent(parentForFruits, true);

            // 2) Опционально добавим авто-очистку, если скрипт существует в проекте
            var cullType = System.Type.GetType("FruitAutoCullAbovePlayer");
            if (cullType != null && go.GetComponent(cullType) == null)
            {
                var comp = go.AddComponent(cullType);
                // Попробуем установить публичное поле offsetAbove, если оно есть
                var field = cullType.GetField("offsetAbove");
                if (field != null && field.FieldType == typeof(float))
                    field.SetValue(comp, 40f);
            }

            _spawnedList.Add(go);
        }
    }

    private static Vector2 GetDir(LineOrientation o)
    {
        return o switch
        {
            LineOrientation.Vertical => Vector2.up,
            LineOrientation.DiagonalNE => new Vector2(1f, 1f),
            LineOrientation.DiagonalNW => new Vector2(-1f, 1f),
            _ => Vector2.up
        };
    }

    private List<LineOrientation> CollectAllowedOrientations()
    {
        var list = new List<LineOrientation>();
        if (allowVertical) list.Add(LineOrientation.Vertical);
        if (allowDiagonalNE) list.Add(LineOrientation.DiagonalNE);
        if (allowDiagonalNW) list.Add(LineOrientation.DiagonalNW);
        return list;
    }

    private int RandRangeInt(int minInclusive, int maxInclusive)
    {
        return _rng == null ? UnityEngine.Random.Range(minInclusive, maxInclusive + 1) : _rng.Next(minInclusive, maxInclusive + 1);
    }

    private float RandRangeFloat(float minInclusive, float maxInclusive)
    {
        return _rng == null ? UnityEngine.Random.Range(minInclusive, maxInclusive)
                            : (float)(_rng.NextDouble() * (maxInclusive - minInclusive) + minInclusive);
    }
}
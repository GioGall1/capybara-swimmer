using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider2D))]
public class ObstacleLineZoneTrigger : MonoBehaviour
{
    public enum LineOrientation { Horizontal, Vertical, DiagonalNE, DiagonalNW }
    public enum ObstacleSelectionMode { RandomPerLine, FixedIndex }

    [Header("Что спавним (один тип на линию)")]
    public List<GameObject> obstaclePrefabs = new List<GameObject>();
    public ObstacleSelectionMode obstacleSelectMode = ObstacleSelectionMode.RandomPerLine;
    public int fixedObstacleIndex = 0;

    [Header("Линии: количество и длина")]
    [Range(1, 8)] public int minLines = 1;
    [Range(1, 8)] public int maxLines = 4;
    [Range(3, 50)] public int minCountPerLine = 3;
    [Range(3, 50)] public int maxCountPerLine = 8;

    [Header("Расстояние между объектами в линии")]
    public float spacingMin = 1f;
    public float spacingMax = 2.5f;

    [Header("Какие ориентации разрешены")]
    public bool allowHorizontal = true;
    public bool allowVertical = false;
    public bool allowDiagonalNE = false;
    public bool allowDiagonalNW = false;

    [Header("Поведение")]
    public bool spawnOnlyOnce = true;
    public Transform parentForObstacles;
    public bool spawnOnPlayerEnter = false;
    [Tooltip("Шанс появления препятствий на чанке (0 = никогда, 1 = всегда)")]
    [Range(0f, 1f)] public float spawnChance = 0.5f;

    [Header("Опционально: детерминированность")]
    public bool deterministic = false;
    public bool usePerChunkRandom = true;
    public int chunkId = 0;
    public int seedOffset = 99999;

    private bool _spawned;
    private bool _spawning;
    private BoxCollider2D _box;
    private readonly List<GameObject> _spawnedList = new List<GameObject>();
    private System.Random _rng;

    private void Awake()
    {
        _box = GetComponent<BoxCollider2D>();
        EnsureParent();
        
    }

    private void EnsureParent()
    {
        if (parentForObstacles == null)
        {
            var root = GetComponentInParent<DestroyIfAbovePlayer>();
            parentForObstacles = root ? root.transform : transform.root;
        }
    }

    private void InitRng()
    {
        if (deterministic)
        {
            _rng = new System.Random(chunkId + seedOffset);
            return;
        }
        if (usePerChunkRandom)
        {
            unchecked
            {
                int seed = 123456789;
                seed = seed * 31 ^ chunkId;
                seed = seed * 31 ^ GetInstanceID();
                seed = seed * 31 ^ Mathf.RoundToInt(transform.position.x * 100f);
                seed = seed * 31 ^ Mathf.RoundToInt(transform.position.y * 100f);
                _rng = new System.Random(seed);
            }
            return;
        }
        _rng = null;
    }

    private void OnEnable()
    {
        _spawned = false;
        _spawning = false;
    }

    private void OnDisable()
    {
        ClearSpawned();
        _spawning = false;
    }

    private void OnDestroy()
    {
        ClearSpawned();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!spawnOnPlayerEnter) return;
        if (!other.CompareTag("Player")) return;
        if (_spawning || (_spawned && spawnOnlyOnce)) return;

        ForceSpawn();
    }

    public void ForceSpawn(int? overrideChunkId = null)
    {
        // проверка вероятности спавна препятствий
        if (spawnChance < 1f)
        {
            float roll = (float) (UnityEngine.Random.value);
            if (roll > spawnChance)
            {
                Debug.Log($"🎲 [ObstacleZone:{name}] Препятствия НЕ будут заспавнены (шанс={spawnChance:P0}, roll={roll:F2})");
                return;
            }
        }

        Debug.Log("🚀 ForceSpawn вызван для чанка " + chunkId);
        Debug.Log($"[ObstacleZone:{name}] Prefabs={obstaclePrefabs?.Count}, Parent={parentForObstacles?.name}");

        _spawning = true;
        _spawned = false;
        if (overrideChunkId.HasValue) chunkId = overrideChunkId.Value;
        InitRng();
        EnsureParent();
        ClearSpawned();
        SpawnLines();
        _spawned = true;
        _spawning = false;
    }

    private void ClearSpawned()
    {
        for (int i = _spawnedList.Count - 1; i >= 0; i--)
            if (_spawnedList[i]) Destroy(_spawnedList[i]);
        _spawnedList.Clear();
    }

    private List<LineOrientation> CollectAllowedOrientations()
    {
        var list = new List<LineOrientation>();
        if (allowHorizontal) list.Add(LineOrientation.Horizontal);
        if (allowVertical) list.Add(LineOrientation.Vertical);
        if (allowDiagonalNE) list.Add(LineOrientation.DiagonalNE);
        if (allowDiagonalNW) list.Add(LineOrientation.DiagonalNW);
        return list;
    }

    private void SpawnLines()
    {
        if (_box == null) return;

        var validPrefabs = BuildValidPrefabList();
        if (validPrefabs.Count == 0) 
        {
            Debug.LogWarning("⚠️ Нет валидных префабов для спавна препятствий");
            return;
        }

        var orientations = CollectAllowedOrientations();
        if (orientations.Count == 0) 
        {
            Debug.LogWarning("⚠️ Нет разрешённых направлений для линий");
            return;
        }

        Bounds b = _box.bounds;
        int lines = RandRangeInt(minLines, maxLines);
        Debug.Log($"[ObstacleZone:{name}] lines={lines} bounds.center={b.center} size={b.size}");

        for (int l = 0; l < lines; l++)
        {
            var o = orientations[RandRangeInt(0, orientations.Count - 1)];
            int count = RandRangeInt(minCountPerLine, maxCountPerLine);
            float spacing = RandRangeFloat(spacingMin, spacingMax);
            var obstacle = PickObstaclePrefabForLine(validPrefabs);
            if (obstacle == null) continue;
            SpawnOneLine(b, o, count, spacing, obstacle);
        }
    }

    private void SpawnOneLine(Bounds b, LineOrientation o, int count, float spacing, GameObject obstaclePrefab)
    {
        Vector2 dir = GetDir(o).normalized;
        float total = spacing * (count - 1);
        float dx = Mathf.Abs(dir.x) * total;
        float dy = Mathf.Abs(dir.y) * total;

        Debug.Log($"📏 Bounds: ({b.min.x:F2}, {b.min.y:F2}) to ({b.max.x:F2}, {b.max.y:F2}) | dx={dx:F2}, dy={dy:F2}");
        Debug.Log($"[Line] Orientation={o} count={count} spacing={spacing}");

        float minX = b.min.x + dx * 0.5f;
        float maxX = b.max.x - dx * 0.5f;
        float minY = b.min.y + dy * 0.5f;
        float maxY = b.max.y - dy * 0.5f;

        if (minX > maxX || minY > maxY) 
        {
            Debug.LogWarning("⛔ Недостаточно места для спавна линии препятствий");
            return;
        }

        Vector2 center = new Vector2(
            RandRangeFloat(minX, maxX),
            RandRangeFloat(minY, maxY)
        );
        Vector2 start = center - dir * (total * 0.5f);

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = start + dir * (i * spacing);
            var go = Instantiate(obstaclePrefab, pos, Quaternion.identity, parentForObstacles);
            // гарантируем, что у заспавненного препятствия коллайдер не в режиме триггера
            var col = go.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = false;

            if (go.transform.parent != parentForObstacles)
                go.transform.SetParent(parentForObstacles, true);
            Debug.Log($"[Spawned] Obstacle at {pos}");
        }
    }

    private GameObject PickObstaclePrefabForLine(List<GameObject> validPrefabs)
    {
        if (validPrefabs == null || validPrefabs.Count == 0) return null;
        if (obstacleSelectMode == ObstacleSelectionMode.FixedIndex)
        {
            if (fixedObstacleIndex < 0 || fixedObstacleIndex >= validPrefabs.Count) return null;
            return validPrefabs[fixedObstacleIndex];
        }
        return validPrefabs[RandRangeInt(0, validPrefabs.Count - 1)];
    }

    private List<GameObject> BuildValidPrefabList()
    {
        var valid = new List<GameObject>();
        foreach (var prefab in obstaclePrefabs)
        {
            if (prefab != null && !prefab.scene.IsValid()) valid.Add(prefab);
        }
        return valid;
    }

    private Vector2 GetDir(LineOrientation o)
    {
        return o switch
        {
            LineOrientation.Horizontal => Vector2.right,
            LineOrientation.Vertical => Vector2.up,
            LineOrientation.DiagonalNE => new Vector2(1f, 1f),
            LineOrientation.DiagonalNW => new Vector2(-1f, 1f),
            _ => Vector2.right
        };
    }

    private int RandRangeInt(int minInclusive, int maxInclusive)
    {
        return _rng == null ? UnityEngine.Random.Range(minInclusive, maxInclusive + 1)
                            : _rng.Next(minInclusive, maxInclusive + 1);
    }

    private float RandRangeFloat(float minInclusive, float maxInclusive)
    {
        return _rng == null ? UnityEngine.Random.Range(minInclusive, maxInclusive)
                            : (float)(_rng.NextDouble() * (maxInclusive - minInclusive) + minInclusive);
    }
}
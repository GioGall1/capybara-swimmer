using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(EdgeCollider2D))]
[RequireComponent(typeof(WaterTriggerHandler))]
public class InteractableWater : MonoBehaviour
{

    [Header("Springs")]
    [SerializeField] private float _spriteConstant = 1.4f;
    [SerializeField] private float _damping = 1.1f;
    [SerializeField] private float _spread = 6.5f;
    [SerializeField, Range(1, 10)] private int _wavePropogationIterations = 8;
    [SerializeField, Range(0f, 20f)] private float _speedMult = 5.5f;

    [Header("Force")]
    public float ForceMultiplier = 0.2f;
    [Range(1f, 50f)] public float MaxForce = 5f;

    [Header("Collision")]
    [SerializeField, Range(1f, 10f)] private float _playerCollisionRadiusMult = 4.15f;

    [Header("Mesh Generation")]
    [Range(2, 500)] public int NumOfXVertices = 70;
    public float Width = 10f;
    public float Height = 4f;
    public Material WaterMaterial;

    private const int NUM_OF_Y_VERTICES = 2;

    [Header("Gizmo")]
    public Color GizmoColor = Color.white;

    private Mesh _mesh;
    private MeshRenderer _meshRenderer;
    private MeshFilter _meshFilter;
    private Vector3[] _vertices;
    private int[] _topVerticesIndex;
    private EdgeCollider2D _coll;
    

    private class WaterPoint
    {
        public float velocity, acceleration, pos, targetHeight;
    }

    private List<WaterPoint> _waterPoints = new List<WaterPoint>();

    private void Awake()
    {
        _coll = GetComponent<EdgeCollider2D>();
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Start()
    {
        _coll = GetComponent<EdgeCollider2D>();

        GenerateMesh();
        CreateWaterPoints();
    }

    private void Update()
{
    if (Input.GetMouseButtonDown(0))
    {
        Vector2 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        SplashAtPoint(worldClick, 10f); // 3f — сила, можно менять
    }
}

public void SplashAtPoint(Vector2 worldPosition, float force)
{
    float radius = 0.5f;

    for (int i = 0; i < _waterPoints.Count; i++)
    {
        Vector2 vertexWorldPos = transform.TransformPoint(_vertices[_topVerticesIndex[i]]);
        float distSqr = (vertexWorldPos - worldPosition).sqrMagnitude;
        float radiusSqr = radius * radius;

        if (distSqr <= radiusSqr)
        {
            _waterPoints[i].velocity = force;

            // DEBUG: покажем попадание
            Debug.DrawRay(vertexWorldPos, Vector2.up * 0.3f, Color.yellow, 1f);
        }
    }

    Debug.DrawRay(worldPosition, Vector2.up * 0.5f, Color.magenta, 1f); // точка клика
}

    private void Reset()
    {
        _coll = GetComponent<EdgeCollider2D>();
        _coll.isTrigger = true;
    }

    private void FixedUpdate()
    {
        for (int i = 1; i < _waterPoints.Count - 1; i++)
        {
            WaterPoint point = _waterPoints[i];

            float x = point.pos - point.targetHeight;
            float acceleration = -_spriteConstant * x - _damping * point.velocity;
            point.pos += point.velocity * _speedMult * Time.fixedDeltaTime;
            _vertices[_topVerticesIndex[i]].y = point.pos;
            point.velocity += acceleration * _speedMult * Time.fixedDeltaTime;
        }

        for (int j = 0; j < _wavePropogationIterations; j++)
        {
            for (int i = 1; i < _waterPoints.Count - 1; i++)
            {
                float leftDelta = _spread * (_waterPoints[i].pos - _waterPoints[i - 1].pos) * _speedMult * Time.fixedDeltaTime;
                _waterPoints[i - 1].velocity += leftDelta;

                float rightDelta = _spread * (_waterPoints[i].pos - _waterPoints[i + 1].pos) * _speedMult * Time.fixedDeltaTime;
                _waterPoints[i + 1].velocity += rightDelta;
            }
        }

        _mesh.vertices = _vertices;

        Debug.DrawLine(
    transform.TransformPoint(_vertices[_topVerticesIndex[10]]),
    transform.TransformPoint(_vertices[_topVerticesIndex[10]] + Vector3.down * 0.5f),
    Color.cyan
);
    }

    public void Splash(Collider2D collision, float force)
{
    float radius = collision.bounds.extents.x * _playerCollisionRadiusMult;
    Vector2 center = collision.transform.position;

    bool hitSomething = false; // для дебага

    for (int i = 0; i < _waterPoints.Count; i++)
    {
        Vector2 vertexWorldPos = transform.TransformPoint(_vertices[_topVerticesIndex[i]]);

        float distSqr = (vertexWorldPos - center).sqrMagnitude;
        float radiusSqr = radius * radius;

        // Визуализируем зону поражения (на 1 секунду)
        // Debug.DrawLine(vertexWorldPos, center, Color.cyan, 1f);

        if (distSqr <= radiusSqr)
        {
            _waterPoints[i].velocity = force;
            hitSomething = true;

            // // Показать отладку: зелёные точки — попали
            // Debug.DrawRay(vertexWorldPos, Vector2.up * 0.3f, Color.green, 1f);
        }
        else
        {
            // // Показать отладку: красные точки — мимо
            // Debug.DrawRay(vertexWorldPos, Vector2.up * 0.3f, Color.red, 1f);
        }
    }

    if (!hitSomething)
    {
        Debug.LogWarning($"[Splash] Ни одна точка не попала в зону воздействия. Center: {center}, Radius: {radius}");
    }
    else
    {
        Debug.Log($"[Splash] Вода деформирована силой {force} в точке {center}");
    }
}

    private bool IsPointInsideCircle(Vector2 point, Vector2 center, float radius)
    {
        float distanceSquadred = (point - center).sqrMagnitude;
        return distanceSquadred <= radius * radius;
    }
    public void ResetEdgeCollider()
{
    if (_vertices == null || _topVerticesIndex == null || _topVerticesIndex.Length == 0) return;

    if (_coll == null)
        _coll = GetComponent<EdgeCollider2D>(); // <- добавь это, если вдруг он не инициализирован

    if (_coll == null)
    {
        Debug.LogWarning("EdgeCollider2D не найден на объекте воды!");
        return;
    }

    Vector2[] newPoints = new Vector2[2];

    Vector2 firstPoint = new Vector2(_vertices[_topVerticesIndex[0]].x, _vertices[_topVerticesIndex[0]].y);
    newPoints[0] = firstPoint;

    Vector2 secondPoint = new Vector2(_vertices[_topVerticesIndex[_topVerticesIndex.Length - 1]].x,
                                      _vertices[_topVerticesIndex[_topVerticesIndex.Length - 1]].y);
    newPoints[1] = secondPoint;

    _coll.offset = Vector2.zero;
    _coll.points = newPoints;
}

    public void GenerateMesh()
    {
        _mesh = new Mesh();

        _vertices = new Vector3[NumOfXVertices * NUM_OF_Y_VERTICES];
        _topVerticesIndex = new int[NumOfXVertices];

        for (int y = 0; y < NUM_OF_Y_VERTICES; y++)
        {
            for (int x = 0; x < NumOfXVertices; x++)
            {
                float xPos = (x / (float)(NumOfXVertices - 1)) * Width - Width / 2;
                float yPos = (y / (float)(NUM_OF_Y_VERTICES - 1)) * Height - Height / 2;
                _vertices[y * NumOfXVertices + x] = new Vector3(xPos, yPos, 0f);

                if (y == NUM_OF_Y_VERTICES - 1)
                {
                    _topVerticesIndex[x] = y * NumOfXVertices + x;
                }
            }
        }

        int[] triangles = new int[(NumOfXVertices - 1) * (NUM_OF_Y_VERTICES - 1) * 6];
        int index = 0;

        for (int y = 0; y < NUM_OF_Y_VERTICES - 1; y++)
        {
            for (int x = 0; x < NumOfXVertices - 1; x++)
            {
                int bottomLeft = y * NumOfXVertices + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + NumOfXVertices;
                int topRight = topLeft + 1;

                triangles[index++] = bottomLeft;
                triangles[index++] = topLeft;
                triangles[index++] = bottomRight;

                triangles[index++] = bottomRight;
                triangles[index++] = topLeft;
                triangles[index++] = topRight;
            }
        }

        Vector2[] uvs = new Vector2[_vertices.Length];
        for (int i = 0; i < _vertices.Length; i++)
        {
            uvs[i] = new Vector2((_vertices[i].x + Width / 2) / Width, (_vertices[i].y + Height / 2) / Height);
        }

        if (_meshRenderer == null) _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshFilter == null) _meshFilter = GetComponent<MeshFilter>();

        _meshRenderer.material = WaterMaterial;
        _mesh.vertices = _vertices;
        _mesh.triangles = triangles;
        _mesh.uv = uvs;
        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        _meshFilter.mesh = _mesh;
    }

    private void CreateWaterPoints()
    {
        _waterPoints.Clear();

        for (int i = 0; i < _topVerticesIndex.Length; i++)
        {
            _waterPoints.Add(new WaterPoint
            {
                pos = _vertices[_topVerticesIndex[i]].y,
                targetHeight = _vertices[_topVerticesIndex[i]].y,
            });
        }
    }
}


[CustomEditor(typeof(InteractableWater))]
public class InteractableWaterEditor : Editor
{
    private InteractableWater _water;

    private void OnEnable()
    {
        _water = (InteractableWater)target;
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);
        root.Add(new VisualElement { style = { height = 10 } });

        if (_water != null)
        {
            root.Add(new Button(() => _water.GenerateMesh()) { text = "Generate Mesh" });
            root.Add(new Button(() => _water.ResetEdgeCollider()) { text = "Place Edge Collider" });
        }

        return root;
    }

    private void ChangeDimensions(ref float width, ref float height, float calculatedWidthMax, float calculatedHeightMax)
    {
        width = Mathf.Max(0.1f, calculatedWidthMax);
        height = Mathf.Max(0.1f, calculatedHeightMax);
    }

    private void OnSceneGUI()
    {
        Handles.color = Color.green;
        Vector3 center = _water.transform.position;
        Vector3 size = new Vector3(_water.Width, _water.Height, 0.1f);
        Handles.DrawWireCube(center, size);

        float handleSize = HandleUtility.GetHandleSize(center) * 0.1f;
        Vector3 snap = Vector3.one * 0.1f;

        Vector3[] corners = new Vector3[4];
        corners[0] = center + new Vector3(-_water.Width / 2, -_water.Height / 2, 0);
        corners[1] = center + new Vector3(_water.Width / 2, -_water.Height / 2, 0);
        corners[2] = center + new Vector3(-_water.Width / 2, _water.Height / 2, 0);
        corners[3] = center + new Vector3(_water.Width / 2, _water.Height / 2, 0);

        // Bottom-left
        EditorGUI.BeginChangeCheck();
        Vector3 newBottomLeft = Handles.FreeMoveHandle(corners[0], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.Width, ref _water.Height, corners[1].x - newBottomLeft.x, corners[0].y - newBottomLeft.y);
            _water.transform.position += (newBottomLeft - corners[0]) / 2;
            _water.GenerateMesh();
        }

        // Bottom-right
        EditorGUI.BeginChangeCheck();
        Vector3 newBottomRight = Handles.FreeMoveHandle(corners[1], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.Width, ref _water.Height, newBottomRight.x - corners[0].x, corners[1].y - newBottomRight.y);
            _water.transform.position += (newBottomRight - corners[1]) / 2;
            _water.GenerateMesh();
        }

        // Top-left
        EditorGUI.BeginChangeCheck();
        Vector3 newTopLeft = Handles.FreeMoveHandle(corners[2], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.Width, ref _water.Height, corners[3].x - newTopLeft.x, newTopLeft.y - corners[0].y);
            _water.transform.position += (newTopLeft - corners[2]) / 2;
            _water.GenerateMesh();
        }

        // Top-right
        EditorGUI.BeginChangeCheck();
        Vector3 newTopRight = Handles.FreeMoveHandle(corners[3], handleSize, snap, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            ChangeDimensions(ref _water.Width, ref _water.Height, newTopRight.x - corners[2].x, newTopRight.y - corners[1].y);
            _water.transform.position += (newTopRight - corners[3]) / 2;
            _water.GenerateMesh();
        }
    }
}
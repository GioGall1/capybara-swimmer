#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

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

        DrawCornerHandle(ref corners[0], corners[1].x - corners[0].x, corners[0].y - corners[0].y);
        DrawCornerHandle(ref corners[1], corners[1].x - corners[0].x, corners[1].y - corners[1].y);
        DrawCornerHandle(ref corners[2], corners[3].x - corners[2].x, corners[2].y - corners[0].y);
        DrawCornerHandle(ref corners[3], corners[3].x - corners[2].x, corners[3].y - corners[1].y);
    }

    private void DrawCornerHandle(ref Vector3 corner, float widthMax, float heightMax)
    {
        EditorGUI.BeginChangeCheck();
        Vector3 newCorner = Handles.FreeMoveHandle(corner, 0.1f, Vector3.one * 0.1f, Handles.CubeHandleCap);
        if (EditorGUI.EndChangeCheck())
        {
            _water.Width = Mathf.Max(0.1f, widthMax);
            _water.Height = Mathf.Max(0.1f, heightMax);
            _water.transform.position += (newCorner - corner) / 2;
            _water.GenerateMesh();
        }
    }
}
#endif
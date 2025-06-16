using UnityEngine;
using UnityEditor;

public static class CopyPasteTransformComponent
{
    struct TransformData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public TransformData(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
        }
    }

    struct RectTransformData
    {
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;
        public Quaternion localRotation;
        public Vector3 localScale;

        public RectTransformData(RectTransform rt)
        {
            anchoredPosition = rt.anchoredPosition;
            sizeDelta = rt.sizeDelta;
            anchorMin = rt.anchorMin;
            anchorMax = rt.anchorMax;
            pivot = rt.pivot;
            localRotation = rt.localRotation;
            localScale = rt.localScale;
        }

        public void ApplyTo(RectTransform rt)
        {
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = sizeDelta;
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.localRotation = localRotation;
            rt.localScale = localScale;
        }
    }

    private static TransformData _data;
    private static RectTransformData _rectData;
    private static Vector3? _dataCenter;

    [MenuItem("Edit/Copy Transform Values &c", false, -101)]
    public static void CopyTransformValues()
    {
        if (Selection.gameObjects.Length == 0) return;
        var selectionTr = Selection.gameObjects[0].transform;
        _data = new TransformData(selectionTr.localPosition, selectionTr.localRotation, selectionTr.localScale);
    }

    [MenuItem("Edit/Paste Transform Values &v", false, -101)]
    public static void PasteTransformValues()
    {
        foreach (var selection in Selection.gameObjects)
        {
            Transform selectionTr = selection.transform;
            Undo.RecordObject(selectionTr, "Paste Transform Values");
            selectionTr.localPosition = _data.localPosition;
            selectionTr.localRotation = _data.localRotation;
            selectionTr.localScale = _data.localScale;
        }
    }

    [MenuItem("Edit/Copy RectTransform Values &r", false, -101)]
    public static void CopyRectTransformValues()
    {
        if (Selection.gameObjects.Length == 0) return;
        var rt = Selection.gameObjects[0].GetComponent<RectTransform>();
        if (rt == null) return;
        _rectData = new RectTransformData(rt);
    }

    [MenuItem("Edit/Paste RectTransform Values &t", false, -101)]
    public static void PasteRectTransformValues()
    {
        foreach (var go in Selection.gameObjects)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) continue;
            Undo.RecordObject(rt, "Paste RectTransform Values");
            _rectData.ApplyTo(rt);
        }
    }

    [MenuItem("Edit/Copy Center Position &k", false, -101)]
    public static void CopyCenterPosition()
    {
        if (Selection.gameObjects.Length == 0) return;
        var render = Selection.gameObjects[0].GetComponent<Renderer>();
        if (render == null) return;
        _dataCenter = render.bounds.center;
    }

    [MenuItem("Edit/Paste Center Position &l", false, -101)]
    public static void PasteCenterPosition()
    {
        if (_dataCenter == null) return;
        foreach (var selection in Selection.gameObjects)
        {
            Undo.RecordObject(selection.transform, "Paste Center Position");
            selection.transform.position = _dataCenter.Value;
        }
    }

    public static class InvertActiveInHierarchy
    {
        [MenuItem("Edit/Invert Active _F3", false, -101)]
        public static void Process()
        {
            foreach (var selectedObject in Selection.gameObjects)
            {
                selectedObject.SetActive(!selectedObject.activeSelf);
            }
        }
    }
}

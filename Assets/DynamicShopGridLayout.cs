using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class DynamicShopGridLayout : MonoBehaviour
{
    [Header("References")]
    public RectTransform content;
    public RectTransform viewport;

    [Header("Grid")]
    [Min(1)] public int columns = 4;
    public Vector2 cellSize = new Vector2(240f, 230f);
    [Min(0f)] public float horizontalSpacing = 20f;
    [Min(0f)] public float verticalSpacing = 20f;

    [Header("Padding")]
    [Min(0f)] public float leftPadding = 20f;
    [Min(0f)] public float rightPadding = 20f;
    [Min(0f)] public float topPadding = 20f;
    [Min(0f)] public float bottomPadding = 20f;

    [Header("Behavior")]
    public bool controlChildSize = true;
    public bool includeInactiveChildren;
    public bool rebuildContinuouslyInEditor = true;
    public bool clampContentToViewport = true;

    private readonly List<RectTransform> items = new List<RectTransform>();

    private void Reset()
    {
        if (content == null)
            content = GetComponent<RectTransform>();

        if (viewport == null && transform.parent != null)
            viewport = transform.parent as RectTransform;
    }

    private void OnEnable()
    {
        RebuildNow();
    }

    private void OnValidate()
    {
        columns = Mathf.Max(1, columns);
        RebuildNow();
    }

    private void OnTransformChildrenChanged()
    {
        RebuildNow();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying && rebuildContinuouslyInEditor)
            RebuildNow();
    }

    [ContextMenu("Rebuild Layout Now")]
    public void RebuildNow()
    {
        if (content == null)
            return;

        EnsureTopAnchoredContent(content);
        CollectItems();
        PositionItems();
        ResizeContentHeight();
    }

    private void CollectItems()
    {
        items.Clear();

        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform child = content.GetChild(i) as RectTransform;
            if (child == null)
                continue;

            if (!includeInactiveChildren && !child.gameObject.activeSelf)
                continue;

            items.Add(child);
        }
    }

    private void PositionItems()
    {
        for (int i = 0; i < items.Count; i++)
        {
            RectTransform item = items[i];
            int row = i / columns;
            int col = i % columns;

            EnsureTopLeftAnchoredItem(item);

            if (controlChildSize)
                item.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, cellSize.x);

            if (controlChildSize)
                item.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellSize.y);

            float x = leftPadding + col * (cellSize.x + horizontalSpacing);
            float y = topPadding + row * (cellSize.y + verticalSpacing);

            item.anchoredPosition = new Vector2(x, -y);
        }
    }

    private void ResizeContentHeight()
    {
        int rowCount = (int)Math.Ceiling(items.Count / (float)columns);

        float neededHeight = topPadding + bottomPadding;
        if (rowCount > 0)
            neededHeight += rowCount * cellSize.y + Mathf.Max(0, rowCount - 1) * verticalSpacing;

        if (clampContentToViewport && viewport != null)
            neededHeight = Mathf.Max(neededHeight, viewport.rect.height + 1f);

        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, neededHeight);

        float neededWidth = leftPadding + rightPadding + columns * cellSize.x + Mathf.Max(0, columns - 1) * horizontalSpacing;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, neededWidth);
    }

    private static void EnsureTopAnchoredContent(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
    }

    private static void EnsureTopLeftAnchoredItem(RectTransform rect)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
    }
}
using UnityEngine;

[ExecuteAlways]
public class ManualScrollContentFitter : MonoBehaviour
{
    [Header("References")]
    public RectTransform content;
    public RectTransform viewport;

    [Header("Padding")]
    public float topPadding = 20f;
    public float bottomPadding = 20f;

    [Header("Update")]
    public bool updateContinuously = true;

    private void Reset()
    {
        if (content == null)
            content = GetComponent<RectTransform>();

        if (viewport == null && transform.parent != null)
            viewport = transform.parent as RectTransform;
    }

    private void LateUpdate()
    {
        if (updateContinuously)
            RebuildHeight();
    }

    public void RebuildHeight()
    {
        if (content == null || viewport == null)
            return;

        // This expects a top-anchored content setup typical for ScrollRect:
        // anchorMin=(0,1), anchorMax=(1,1), pivot=(0.5,1), anchoredPosition.y = 0
        float lowestY = 0f;
        bool foundChild = false;

        Vector3[] corners = new Vector3[4];
        for (int i = 0; i < content.childCount; i++)
        {
            RectTransform child = content.GetChild(i) as RectTransform;
            if (child == null || !child.gameObject.activeSelf)
                continue;

            child.GetWorldCorners(corners);
            for (int c = 0; c < 4; c++)
            {
                Vector3 local = content.InverseTransformPoint(corners[c]);
                if (!foundChild || local.y < lowestY)
                {
                    lowestY = local.y;
                    foundChild = true;
                }
            }
        }

        float viewportHeight = viewport.rect.height;
        float contentNeeded = topPadding + bottomPadding;

        if (foundChild)
            contentNeeded += -lowestY;

        // Keep content at least as tall as viewport so ScrollRect clamps correctly.
        float targetHeight = Mathf.Max(viewportHeight + 1f, contentNeeded);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
    }
}
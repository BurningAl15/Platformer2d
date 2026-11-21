using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>Provides a safe default selection for keyboard and gamepad menus.</summary>
public static class UISelectionUtility
{
    public static void SelectDefault(GameObject preferred, Transform scope = null)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return;

        GameObject candidate = IsSelectable(preferred) ? preferred : FindFirstSelectable(scope);
        eventSystem.SetSelectedGameObject(null);
        if (candidate != null)
            eventSystem.SetSelectedGameObject(candidate);
    }

    /// <summary>
    /// Keeps gamepad/keyboard navigation inside a panel: its visible buttons are
    /// chained top to bottom (wrapping), so focus never jumps to another panel.
    /// </summary>
    public static void ContainNavigation(Transform scope)
    {
        if (scope == null) return;
        var buttons = new List<Selectable>();
        foreach (Selectable selectable in scope.GetComponentsInChildren<Selectable>(false))
            if (selectable.IsInteractable() && selectable.isActiveAndEnabled) buttons.Add(selectable);
        buttons.Sort((a, b) => b.transform.position.y.CompareTo(a.transform.position.y));
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = buttons[(i - 1 + buttons.Count) % buttons.Count],
                selectOnDown = buttons[(i + 1) % buttons.Count]
            };
        }
    }

    private static readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    /// <summary>
    /// True when a raycast-target UI element sits under the screen point. This
    /// raycasts directly because IsPointerOverGameObject is unreliable for touch
    /// with the Input System UI module, especially on the frame of the press.
    /// </summary>
    public static bool IsScreenPointOverUI(Vector2 screenPosition)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;
        var pointer = new PointerEventData(eventSystem) { position = screenPosition };
        raycastResults.Clear();
        eventSystem.RaycastAll(pointer, raycastResults);
        bool hit = raycastResults.Count > 0;
        raycastResults.Clear();
        return hit;
    }

    private static GameObject FindFirstSelectable(Transform scope)
    {
        if (scope == null) return null;

        foreach (Selectable selectable in scope.GetComponentsInChildren<Selectable>(true))
        {
            if (selectable != null && selectable.gameObject.activeInHierarchy && selectable.IsInteractable())
                return selectable.gameObject;
        }

        return null;
    }

    private static bool IsSelectable(GameObject candidate)
    {
        if (candidate == null || !candidate.activeInHierarchy) return false;
        Selectable selectable = candidate.GetComponent<Selectable>();
        return selectable != null && selectable.IsInteractable();
    }

    /// <summary>
    /// Shrinks a fixed-size menu box so it fits inside its full-screen parent with a margin,
    /// counting parts drawn outside the box such as a header tab. Wide phones (about 2.2:1)
    /// give the canvas less height than the 1080 the boxes were authored for.
    /// Returns the parent size it fitted for, so callers can refit when it changes.
    /// </summary>
    public static Vector2 FitBoxToParent(RectTransform box, float margin = 40f)
    {
        if (box == null || box.parent is not RectTransform parent) return Vector2.zero;
        Canvas.ForceUpdateCanvases();
        Vector2 size = parent.rect.size;
        box.localScale = Vector3.one;
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(box);
        float halfHeight = Mathf.Max(Mathf.Abs(bounds.max.y), Mathf.Abs(bounds.min.y));
        float halfWidth = Mathf.Max(Mathf.Abs(bounds.max.x), Mathf.Abs(bounds.min.x));
        if (halfHeight > 0f && halfWidth > 0f)
        {
            float scale = Mathf.Min(1f, (size.y * 0.5f - margin) / halfHeight, (size.x * 0.5f - margin) / halfWidth);
            box.localScale = new Vector3(scale, scale, 1f);
        }
        return size;
    }
}

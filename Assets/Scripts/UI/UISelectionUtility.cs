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
}

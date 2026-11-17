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

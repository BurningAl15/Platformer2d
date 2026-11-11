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

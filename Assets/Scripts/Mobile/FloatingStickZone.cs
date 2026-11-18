using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

/// <summary>
/// Invisible touch area for the on-screen controls. It never catches UI raycasts:
/// it reads touches directly and only claims a touch that starts inside its rect
/// with no interactive UI (buttons, menus, panels) under it, so every menu keeps
/// working whatever its canvas mode. With a stick assigned, the stick base jumps
/// under the finger and the drag drives the existing OnScreenStick; with a button
/// assigned, the touch presses that OnScreenButton (jump zone).
/// The zone also stays idle while the game is paused, the level has ended or time
/// is frozen (Game Over, ads).
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class FloatingStickZone : MonoBehaviour
{
    [Tooltip("Optional: stick driven by this zone.")]
    [SerializeField] private OnScreenStick stick;
    [Tooltip("Container moved under the finger; returns home on release.")]
    [SerializeField] private RectTransform stickBase;
    [Tooltip("Optional: button pressed by this zone (jump zone).")]
    [SerializeField] private OnScreenButton button;

    private const int NoTouch = int.MinValue;
    private const int MouseId = -1;
    private static readonly List<RaycastResult> hits = new List<RaycastResult>();

    private RectTransform area;
    private Vector2 home;
    private int touchId = NoTouch;
    private PointerEventData eventData;

    private void Awake()
    {
        area = (RectTransform)transform;
        if (stickBase != null) home = stickBase.anchoredPosition;
        // The zone must never take UI raycasts from menus.
        foreach (Graphic graphic in GetComponents<Graphic>()) graphic.raycastTarget = false;
        if (button == null) button = GetComponent<OnScreenButton>();
    }

    private void OnDisable() => Release();

    private void Update()
    {
        if (GameplayBlocked() || !ParentGroupsInteractive())
        {
            Release();
            return;
        }

        if (touchId != NoTouch)
        {
            if (TryGetPointer(touchId, out Vector2 position, out bool pressed) && pressed)
            {
                if (position != eventData.position)
                {
                    eventData.position = position;
                    if (stick != null) stick.OnDrag(eventData);
                }
            }
            else
            {
                Release();
            }
            return;
        }

        Touchscreen screen = Touchscreen.current;
        if (screen != null)
        {
            foreach (var touch in screen.touches)
            {
                if (touch.press.wasPressedThisFrame && TryBegin(touch.touchId.ReadValue(), touch.position.ReadValue()))
                    return;
            }
        }
#if UNITY_EDITOR
        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            TryBegin(MouseId, mouse.position.ReadValue());
#endif
    }

    private bool TryBegin(int id, Vector2 position)
    {
        if (!RectTransformUtility.RectangleContainsScreenPoint(area, position, null)) return false;
        if (IsOverInteractiveUI(position)) return false;

        touchId = id;
        eventData = new PointerEventData(EventSystem.current) { pointerId = id, position = position };

        if (stick != null && stickBase != null)
        {
            var parent = (RectTransform)stickBase.parent;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, position, null, out Vector2 local))
                stickBase.localPosition = local;
            stick.OnPointerDown(eventData);
        }
        if (button != null) button.OnPointerDown(eventData);
        return true;
    }

    private void Release()
    {
        if (touchId == NoTouch) return;
        touchId = NoTouch;
        if (stick != null) stick.OnPointerUp(eventData);
        if (button != null) button.OnPointerUp(eventData);
        if (stickBase != null) stickBase.anchoredPosition = home;
    }

    private static bool TryGetPointer(int id, out Vector2 position, out bool pressed)
    {
        position = default;
        pressed = false;
        if (id == MouseId)
        {
            Mouse mouse = Mouse.current;
            if (mouse == null) return false;
            position = mouse.position.ReadValue();
            pressed = mouse.leftButton.isPressed;
            return true;
        }

        Touchscreen screen = Touchscreen.current;
        if (screen == null) return false;
        foreach (var touch in screen.touches)
        {
            if (touch.touchId.ReadValue() != id) continue;
            position = touch.position.ReadValue();
            pressed = touch.press.isPressed;
            return true;
        }
        return false;
    }

    /// <summary>True when a button, scroll view or other pointer-handling UI is under the point.</summary>
    private bool IsOverInteractiveUI(Vector2 position)
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null) return false;
        hits.Clear();
        eventSystem.RaycastAll(new PointerEventData(eventSystem) { position = position }, hits);
        bool interactive = false;
        foreach (RaycastResult hit in hits)
        {
            GameObject go = hit.gameObject;
            if (go == null || go.transform.IsChildOf(transform)) continue;
            if (ExecuteEvents.GetEventHandler<IPointerDownHandler>(go) != null
                || ExecuteEvents.GetEventHandler<IPointerClickHandler>(go) != null
                || ExecuteEvents.GetEventHandler<IDragHandler>(go) != null)
            {
                interactive = true;
                break;
            }
        }
        hits.Clear();
        return interactive;
    }

    private bool ParentGroupsInteractive()
    {
        Transform t = transform;
        while (t != null)
        {
            if (t.TryGetComponent(out CanvasGroup group))
            {
                if (!group.blocksRaycasts || !group.interactable) return false;
                if (group.ignoreParentGroups) break;
            }
            t = t.parent;
        }
        return true;
    }

    private static bool GameplayBlocked()
    {
        if (Time.timeScale == 0f) return true;
        if (PauseMenu._instance != null && PauseMenu._instance.isPaused) return true;
        return LevelManager._instance != null && LevelManager._instance.stopGame;
    }
}

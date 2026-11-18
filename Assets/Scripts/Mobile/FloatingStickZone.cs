using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

/// <summary>
/// Invisible touch zone for the on-screen controls. With a stick assigned, the
/// stick base jumps under the finger and the drag is forwarded to the existing
/// OnScreenStick; without one it only gates a sibling OnScreenButton (jump zone).
/// The zone stops catching touches while the game is paused, the level has ended
/// or time is frozen (Game Over, ads), so menus always get the input.
/// </summary>
[RequireComponent(typeof(Image))]
public class FloatingStickZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Tooltip("Optional: leave empty for a plain button zone.")]
    [SerializeField] private OnScreenStick stick;
    [Tooltip("Container moved under the finger; returns home on release.")]
    [SerializeField] private RectTransform stickBase;

    private const int NoPointer = int.MinValue;

    private Image zone;
    private Vector2 home;
    private int pointerId = NoPointer;

    private void Awake()
    {
        zone = GetComponent<Image>();
        if (stickBase != null) home = stickBase.anchoredPosition;
    }

    private void OnDisable() => Release(null);

    private void Update()
    {
        bool blocked = GameplayBlocked();
        if (zone.raycastTarget == !blocked) return;
        zone.raycastTarget = !blocked;
        if (blocked) Release(null);
    }

    private static bool GameplayBlocked()
    {
        if (Time.timeScale == 0f) return true;
        if (PauseMenu._instance != null && PauseMenu._instance.isPaused) return true;
        return LevelManager._instance != null && LevelManager._instance.stopGame;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (stick == null || stickBase == null || pointerId != NoPointer) return;
        pointerId = eventData.pointerId;

        var area = (RectTransform)stickBase.parent;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(area, eventData.position, eventData.pressEventCamera, out Vector2 local))
            stickBase.localPosition = local;
        stick.OnPointerDown(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (stick != null && eventData.pointerId == pointerId)
            stick.OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId == pointerId)
            Release(eventData);
    }

    private void Release(PointerEventData eventData)
    {
        if (pointerId == NoPointer) return;
        pointerId = NoPointer;
        if (stick != null) stick.OnPointerUp(eventData);
        if (stickBase != null) stickBase.anchoredPosition = home;
    }
}

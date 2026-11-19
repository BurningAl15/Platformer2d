using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Shows which key or gamepad button triggers a menu action, following the
/// device the player used last. Hidden on touch screens without a gamepad.
/// </summary>
[RequireComponent(typeof(Image))]
public class InputGlyph : MonoBehaviour
{
    [SerializeField] private Sprite keyboardSprite;
    [SerializeField] private Sprite gamepadSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    private void OnEnable() => Refresh();

    private void Update() => Refresh();

    private void Refresh()
    {
        bool pad = InputDeviceTracker.UsingGamepad;
        bool show = pad || !PlatformFeatures.IsMobile;      // touch-only phones have no keys to show
        Sprite sprite = pad ? gamepadSprite : keyboardSprite;
        image.enabled = show && sprite != null;
        if (image.enabled && image.sprite != sprite) image.sprite = sprite;
    }
}

/// <summary>Remembers whether the last real input came from a gamepad or a keyboard/mouse.</summary>
public static class InputDeviceTracker
{
    private static bool initialized;
    private static bool usingGamepad;

    public static bool UsingGamepad
    {
        get
        {
            if (!initialized) Initialize();
            return usingGamepad;
        }
    }

    private static void Initialize()
    {
        initialized = true;
        usingGamepad = MobileGameplayControls.HasPhysicalGameController() && Keyboard.current == null;
        InputSystem.onEvent += (eventPtr, device) =>
        {
            if (device is Gamepad && MobileGameplayControls.IsPhysicalGameController(device))
            {
                // Ignore analog noise: a button or stick must actually move.
                if (eventPtr.EnumerateChangedControls(device, 0.4f).GetEnumerator().MoveNext()) usingGamepad = true;
            }
            else if (device is Keyboard || device is Mouse)
            {
                if (eventPtr.EnumerateChangedControls(device, 0.01f).GetEnumerator().MoveNext()) usingGamepad = false;
            }
        };
    }
}

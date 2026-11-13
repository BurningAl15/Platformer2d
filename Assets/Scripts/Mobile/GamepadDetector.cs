using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadDetector : MonoBehaviour
{
    public static GamepadDetector Instance { get; private set; }

    [Tooltip("Log physical controller connection changes.")]
    [SerializeField] private bool debugLog = true;
    private bool connected;
    private bool refreshRequested = true;
    private float nextRefresh;

#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaClass androidInput;
    private bool supportsExternal;
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
#if UNITY_ANDROID && !UNITY_EDITOR
        androidInput = new AndroidJavaClass("android.view.InputDevice");
        using var version = new AndroidJavaClass("android.os.Build$VERSION");
        supportsExternal = version.GetStatic<int>("SDK_INT") >= 29;
#endif
    }

    private void OnEnable() => InputSystem.onDeviceChange += DeviceChanged;
    private void OnDisable() => InputSystem.onDeviceChange -= DeviceChanged;

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
#if UNITY_ANDROID && !UNITY_EDITOR
        androidInput?.Dispose();
#endif
    }

    private void DeviceChanged(InputDevice device, InputDeviceChange change)
    {
        if (device.native && (device is Gamepad || device is Joystick))
            refreshRequested = true;
    }

    private void OnApplicationFocus(bool focused)
    {
        if (focused) refreshRequested = true;
    }

    private void Update()
    {
        if (!refreshRequested && Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + 0.5f;
        refreshRequested = false;
        bool present = ControllerPresent();
        if (present == connected) return;
        connected = present;
        if (debugLog) Debug.Log($"[GamepadDetector] Physical controller connected={connected}.");
        PlatformDetector.Instance?.ForceReconfigure();
    }

    public bool HasPhysicalGamepad() => connected;

    private bool ControllerPresent()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // Query the OS because some phones also expose built-in input as a Unity joystick.
        const int gamepad = 0x401;
        const int joystick = 0x01000010;
        foreach (int id in androidInput.CallStatic<int[]>("getDeviceIds"))
        {
            using var device = androidInput.CallStatic<AndroidJavaObject>("getDevice", id);
            if (device == null || device.Call<bool>("isVirtual")) continue;
            int sources = device.Call<int>("getSources");
            if ((sources & gamepad) != gamepad && (sources & joystick) != joystick) continue;
            if (!supportsExternal || device.Call<bool>("isExternal")) return true;
        }
#else
        foreach (InputDevice device in InputSystem.devices)
            if (device.native && device.enabled && (device is Gamepad || device is Joystick))
                return true;
#endif
        return false;
    }
}

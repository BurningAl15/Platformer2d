using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using System.Collections.Generic;

/// <summary>
/// Detecta la conexión y desconexión de gamepads físicos (GameSir X5s,
/// Xbox, PlayStation, etc.) y oculta/muestra los controles on-screen
/// de forma automática.
///
/// Lógica:
/// - Si hay al menos un Gamepad/Joystick físico conectado → ocultar UI mobile.
/// - Si no hay ningún Gamepad → mostrar UI mobile (solo si es plataforma mobile).
///
/// Se apoya en PlatformDetector para saber si mostrar o no la UI mobile.
///
/// SETUP REQUERIDO EN UNITY EDITOR:
/// Agregar este componente al mismo GameObject de AppBootstrap o
/// PlatformDetector (cualquiera que persista entre escenas con DontDestroyOnLoad).
/// </summary>
public class GamepadDetector : MonoBehaviour
{
    public static GamepadDetector Instance { get; private set; }

    [Tooltip("Si está activo, loguea eventos de conexión/desconexión de gamepads.")]
    [SerializeField] private bool debugLog = true;

    // Cache for logs/state application; the public query also checks the live
    // device list so startup order cannot leave it stale.
    private bool physicalGamepadConnected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // No necesita DontDestroyOnLoad propio; se asume que vive en
        // el mismo GameObject que AppBootstrap (que sí tiene DDOL).
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChanged;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChanged;
    }

    private void Start()
    {
        // Check devices connected before this listener was installed too. Some
        // mobile controllers (including GameSir models) are exposed as Joystick.
        RefreshGamepadState();
    }

    // ───────────────────────────────────────────────
    //  API pública
    // ───────────────────────────────────────────────

    /// <summary>
    /// Retorna true si hay al menos un Gamepad físico activo y conectado.
    /// Los controles on-screen (virtual) no cuentan como Gamepad.
    /// </summary>
    public bool HasPhysicalGamepad()
    {
        return HasPhysicalGamepadFromDevices();
    }

    private static bool IsOnScreenDevice(InputDevice device)
    {
        if (device == null) return false;

        string name = device.name.ToLowerInvariant();
        string display = device.displayName.ToLowerInvariant();
        if (name.Contains("onscreen") || display.Contains("onscreen") ||
            name.Contains("virtual") || display.Contains("virtual"))
        {
            return true;
        }

        foreach (InternedString usage in device.usages)
        {
            if (string.Equals(usage.ToString(), "OnScreen", System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    // ───────────────────────────────────────────────
    //  Detección
    // ───────────────────────────────────────────────

    private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        if (!(device is Gamepad) && !(device is Joystick)) return;

        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Reconnected:
            case InputDeviceChange.Enabled:
                if (debugLog)
                    Debug.Log($"[GamepadDetector] Input device changed: name='{device.displayName}', layout='{device.layout}', onScreen={IsOnScreenDevice(device)}, physicalMatch={IsPhysicalGamepad(device)}.");
                // Let OnScreenControl set its usage before classifying its virtual pad.
                CancelInvoke(nameof(RefreshGamepadState));
                Invoke(nameof(RefreshGamepadState), 0.1f);
                break;

            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Disabled:
                if (debugLog)
                    Debug.Log($"[GamepadDetector] Input device removed or disabled: name='{device.displayName}', layout='{device.layout}'.");
                CancelInvoke(nameof(RefreshGamepadState));
                Invoke(nameof(RefreshGamepadState), 0.1f);
                break;
        }
    }

    private void RefreshGamepadState()
    {
        physicalGamepadConnected = HasPhysicalGamepadFromDevices();
        if (debugLog)
            Debug.Log($"[GamepadDetector] Physical gamepad present={physicalGamepadConnected}; devices=[{GetPhysicalGamepadSummary()}].");
        ApplyGamepadState();
    }

    private static string GetPhysicalGamepadSummary()
    {
        var detectedDevices = new List<string>();
        foreach (InputDevice device in InputSystem.devices)
        {
            if (IsPhysicalGamepad(device))
                detectedDevices.Add($"{device.displayName} ({device.layout})");
        }

        return detectedDevices.Count == 0 ? "none" : string.Join(", ", detectedDevices);
    }

    private bool HasPhysicalGamepadFromDevices()
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            if (IsPhysicalGamepad(device)) return true;
        }
        return false;
    }

    private static bool IsPhysicalGamepad(InputDevice device)
    {
        if (device == null || !device.added || !device.enabled || IsOnScreenDevice(device))
            return false;
        if (!(device is Gamepad) && !(device is Joystick))
            return false;

        string name = device.name.ToLowerInvariant();
        string display = device.displayName.ToLowerInvariant();
        string interfaceName = device.description.interfaceName.ToLowerInvariant();
        string deviceClass = device.description.deviceClass.ToLowerInvariant();

        // Mobile backends can expose generic Gamepad/Joystick devices without
        // physical hardware. Only treat devices with a recognizable product or
        // display identity as controllers (e.g. "GameSir X5s" or "Xbox Wireless Controller").
        string product = (device.description.product ?? string.Empty).ToLowerInvariant();
        bool hasIdentifiableController =
            (!string.IsNullOrWhiteSpace(product) && !IsGenericJoystickIdentity(product)) ||
            (!string.IsNullOrWhiteSpace(display) && !IsGenericJoystickIdentity(display)) ||
            (!string.IsNullOrWhiteSpace(name) && !IsGenericJoystickIdentity(name));
        if (!hasIdentifiableController) return false;

        return !name.Contains("virtual") && !name.Contains("onscreen") && !name.Contains("touch") &&
               !display.Contains("virtual") && !display.Contains("onscreen") && !display.Contains("touch") &&
               !interfaceName.Contains("onscreen") && !interfaceName.Contains("virtual") &&
               !deviceClass.Contains("onscreen") && !deviceClass.Contains("virtual") &&
               !product.Contains("virtual") && !product.Contains("onscreen") && !product.Contains("touch") &&
               !name.Contains("sensor") && !display.Contains("sensor") && !product.Contains("sensor") &&
               !display.Contains("accelerometer") && !display.Contains("gyro");
    }

    private static bool IsGenericJoystickIdentity(string identity)
    {
        return identity.Contains("joystick") || identity.Contains("generic") ||
               identity.Contains("unknown") || identity.Contains("android") ||
               identity.Contains("touchscreen") || identity.Contains("touch screen") ||
               identity == "controller" || identity == "gamepad";
    }

    /// <summary>
    /// Aplica el estado correcto de la UI mobile según si hay gamepad o no.
    /// </summary>
    private void ApplyGamepadState()
    {
        if (PlatformDetector.Instance == null) return;

        physicalGamepadConnected = HasPhysicalGamepadFromDevices();
        bool hasGamepad = physicalGamepadConnected;

        if (hasGamepad)
        {
            // Gamepad conectado → siempre ocultar controles virtuales
            // Usamos ForceSet para que funcione también en plataforma mobile
            PlatformDetector.Instance.ForceSetMobileUIActive(false, false);
            if (debugLog)
                Debug.Log("[GamepadDetector] Virtual controls hidden because a physical gamepad is active.");
        }
        else
        {
            // Sin gamepad → dejar que PlatformDetector decida según la plataforma
            PlatformDetector.Instance.ForceReconfigure();
            if (debugLog)
                Debug.Log("[GamepadDetector] No physical gamepad. PlatformDetector is restoring scene-appropriate mobile UI.");
        }
    }
}

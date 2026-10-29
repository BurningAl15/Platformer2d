using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Detecta la conexión y desconexión de gamepads físicos (GameSir X5s,
/// Xbox, PlayStation, etc.) y oculta/muestra los controles on-screen
/// de forma automática.
///
/// Lógica:
/// - Si hay al menos un Gamepad conectado → ocultar UI mobile.
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
        // Aplicar estado inicial al arrancar la escena
        ApplyGamepadState();
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
        foreach (InputDevice device in InputSystem.devices)
        {
            if ((device is Gamepad || device is Joystick) && device.enabled &&
                !IsOnScreenDevice(device))
                return true;
        }
        return false;
    }

    private static bool IsOnScreenDevice(InputDevice device)
    {
        InternedString onScreenUsage = new InternedString("OnScreen");
        foreach (InternedString usage in device.usages)
        {
            if (usage == onScreenUsage)
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
                    Debug.Log($"[GamepadDetector] Gamepad conectado: {device.displayName}");
                // On-screen controls register a virtual Gamepad before marking it
                // with the OnScreen usage. Wait briefly before checking the device list.
                CancelInvoke(nameof(ApplyGamepadState));
                Invoke(nameof(ApplyGamepadState), 0.05f);
                break;

            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Disabled:
                if (debugLog)
                    Debug.Log($"[GamepadDetector] Gamepad desconectado: {device.displayName}");
                CancelInvoke(nameof(ApplyGamepadState));
                Invoke(nameof(ApplyGamepadState), 0.05f);
                break;
        }
    }

    /// <summary>
    /// Aplica el estado correcto de la UI mobile según si hay gamepad o no.
    /// </summary>
    private void ApplyGamepadState()
    {
        if (PlatformDetector.Instance == null) return;

        bool hasGamepad = HasPhysicalGamepad();

        if (hasGamepad)
        {
            // Gamepad conectado → siempre ocultar controles virtuales
            // Usamos ForceSet para que funcione también en plataforma mobile
            PlatformDetector.Instance.ForceSetMobileUIActive(false, false);
            if (debugLog)
                Debug.Log("[GamepadDetector] Controles on-screen OCULTADOS (gamepad activo).");
        }
        else
        {
            // Sin gamepad → dejar que PlatformDetector decida según la plataforma
            PlatformDetector.Instance.ForceReconfigure();
            if (debugLog)
                Debug.Log("[GamepadDetector] Sin gamepad — PlatformDetector reconfigura UI mobile.");
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

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
        return Gamepad.current != null;
    }

    // ───────────────────────────────────────────────
    //  Detección
    // ───────────────────────────────────────────────

    private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad) return;

        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Reconnected:
            case InputDeviceChange.Enabled:
                if (debugLog)
                    Debug.Log($"[GamepadDetector] Gamepad conectado: {device.displayName}");
                ApplyGamepadState();
                break;

            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Disabled:
                if (debugLog)
                    Debug.Log($"[GamepadDetector] Gamepad desconectado: {device.displayName}");
                ApplyGamepadState();
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

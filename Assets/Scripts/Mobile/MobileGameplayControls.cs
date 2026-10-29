using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Controles táctiles móviles serializados (joystick y botones de salto/pausa).
/// Se ocultan automáticamente cuando se detecta un mando físico conectado (USB-C o Bluetooth).
///
/// SOPORTA:
/// 1. Modo serializado (preferido): colocado en UI-Canvas con CanvasGroup y OnScreen controls.
/// 2. Modo fallback: genera los controles dinámicamente si la escena no tiene UI mobile serializada.
/// </summary>
public sealed class MobileGameplayControls : MonoBehaviour
{
    private const string GameplayScenePrefix = "Level_";

    [Header("Referencias Serializadas (opcional, asignadas en Prefab)")]
    [SerializeField] private CanvasGroup controlsCanvasGroup;
    [SerializeField] private OnScreenStick onScreenStick;
    [SerializeField] private OnScreenButton jumpButton;
    [SerializeField] private OnScreenButton pauseButton;

    public static bool IsMobile()
    {
#if UNITY_ANDROID || UNITY_IOS
        return true;
#else
        return Application.isMobilePlatform;
#endif
    }

    private void Awake()
    {
        if (controlsCanvasGroup == null)
            controlsCanvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        InputSystem.onDeviceChange += OnDeviceChanged;
        RefreshVisibility();
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChanged;
    }

    private void OnDeviceChanged(InputDevice device, InputDeviceChange change)
    {
        if (!IsPhysicalGameController(device))
            return;

        CancelInvoke(nameof(RefreshVisibility));
        Invoke(nameof(RefreshVisibility), 0.05f);
    }

    public void RefreshVisibility()
    {
        if (controlsCanvasGroup == null)
            return;

        bool visible = IsMobile() && IsGameplayScene(SceneManager.GetActiveScene().name)
            && !HasPhysicalGameController();

        controlsCanvasGroup.alpha = visible ? 1f : 0f;
        controlsCanvasGroup.blocksRaycasts = visible;
        controlsCanvasGroup.interactable = visible;
    }

    public static bool HasPhysicalGameController()
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            if (IsPhysicalGameController(device) && device.enabled)
                return true;
        }
        return false;
    }

    public static bool IsPhysicalGameController(InputDevice device)
    {
        if (!(device is Gamepad) && !(device is Joystick))
            return false;

        // OnScreenStick/Button crean un Gamepad virtual con uso "OnScreen".
        InternedString onScreenUsage = new InternedString("OnScreen");
        foreach (InternedString usage in device.usages)
        {
            if (usage == onScreenUsage)
                return false;
        }
        return true;
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return sceneName.StartsWith(GameplayScenePrefix, System.StringComparison.OrdinalIgnoreCase);
    }
}

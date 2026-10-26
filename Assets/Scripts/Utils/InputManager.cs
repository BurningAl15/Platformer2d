using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Hub central de input del juego. Singleton con DontDestroyOnLoad.
///
/// SOPORTA:
/// - Teclado (WASD / flechas / Space / Escape)
/// - Gamepad físico (Xbox, PlayStation, GameSir X5s y similares USB-C / Bluetooth)
///   Los bindings están definidos en PlayerInputSystem.inputactions.
/// - Controles on-screen (mobile): funcionan automáticamente si los
///   GameObjects de joystick y botón tienen OnScreenStick / OnScreenButton
///   apuntando a "<Gamepad>/leftStick" y "<Gamepad>/buttonSouth".
///
/// EVENTOS (subscribir con +=, desuscribir con -=):
///   OnJumpPressed    — botón de salto presionado
///   OnJumpReleased   — botón de salto soltado
///   OnPausePressed   — botón de pausa presionado
///   OnSubmitPressed  — botón de submit (UI) presionado
///
/// PROPIEDADES:
///   MoveInput        — Vector2 del eje de movimiento (Player map)
///   NavigationInput  — Vector2 del eje de navegación (UI map)
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInputSystem inputActions;

    // ───────────────────────────────────────────────
    //  Eventos públicos
    // ───────────────────────────────────────────────
    public System.Action OnJumpPressed;
    public System.Action OnJumpReleased;
    public System.Action OnPausePressed;
    public System.Action OnSubmitPressed;

    // ───────────────────────────────────────────────
    //  Propiedades de lectura
    // ───────────────────────────────────────────────

    /// <summary>
    /// Eje de movimiento del jugador. Unifica teclado, gamepad y on-screen stick.
    /// </summary>
    public Vector2 MoveInput => inputActions?.Player.Move.ReadValue<Vector2>() ?? Vector2.zero;

    /// <summary>
    /// Eje de navegación de UI. Unifica teclado, gamepad y D-Pad.
    /// </summary>
    public Vector2 NavigationInput
    {
        get
        {
            if (inputActions?.UI.Navigate == null) return MoveInput;
            return inputActions.UI.Navigate.ReadValue<Vector2>();
        }
    }

    // ───────────────────────────────────────────────
    //  Ciclo de vida
    // ───────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        inputActions = new PlayerInputSystem();

        inputActions.Player.Jump.performed += _ => OnJumpPressed?.Invoke();
        inputActions.Player.Jump.canceled  += _ => OnJumpReleased?.Invoke();
        inputActions.UI.Pause.performed    += _ => OnPausePressed?.Invoke();
        inputActions.UI.Submit.performed   += _ => OnSubmitPressed?.Invoke();
    }

    private void OnEnable()
    {
        EnablePlayerInput();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    // ───────────────────────────────────────────────
    //  Control de action maps
    // ───────────────────────────────────────────────

    /// <summary>
    /// Activa Player + UI (modo gameplay normal).
    /// </summary>
    public void EnablePlayerInput()
    {
        inputActions?.Player.Enable();
        inputActions?.UI.Enable();
    }

    /// <summary>
    /// Solo UI activo. Usar durante menús, pausa y cinemáticas.
    /// </summary>
    public void EnableUIOnlyInput()
    {
        inputActions?.Player.Disable();
        inputActions?.UI.Enable();
    }

    /// <summary>
    /// Todo desactivado. Usar durante transiciones de escena.
    /// </summary>
    public void DisableAllInput()
    {
        inputActions?.Player.Disable();
        inputActions?.UI.Disable();
    }

    // ───────────────────────────────────────────────
    //  Retrocompatibilidad
    // ───────────────────────────────────────────────

    /// <summary>
    /// Alias de NavigationInput. Mantenido por retrocompatibilidad.
    /// </summary>
    public Vector2 GetNavigationInput() => NavigationInput;
}

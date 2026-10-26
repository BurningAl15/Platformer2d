using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

/// <summary>
/// Helper de referencia para los controles on-screen.
/// Agregar este script al GameObject raíz de la UI mobile de gameplay.
///
/// ──────────────────────────────────────────────────────────────────
/// SETUP COMPLETO EN UNITY EDITOR:
/// ──────────────────────────────────────────────────────────────────
///
/// JOYSTICK (movimiento):
/// 1. Seleccionar el GameObject del joystick visual.
/// 2. Add Component → OnScreenStick (Unity.InputSystem).
/// 3. Control Path = "<Gamepad>/leftStick"
/// 4. Movement Range = 50 (ajustar al tamaño del joystick en pantalla)
///
/// BOTÓN SALTO:
/// 1. Seleccionar el GameObject del botón de salto.
/// 2. Add Component → OnScreenButton (Unity.InputSystem).
/// 3. Control Path = "<Gamepad>/buttonSouth"
///
/// BOTÓN PAUSA (opcional, si hay un botón de pausa en la UI mobile):
/// 1. Add Component → OnScreenButton.
/// 2. Control Path = "<Gamepad>/start"
///
/// ──────────────────────────────────────────────────────────────────
/// CÓMO FUNCIONA:
/// ──────────────────────────────────────────────────────────────────
/// OnScreenStick y OnScreenButton inyectan input virtual en el
/// Input System como si fuera un gamepad real. InputManager lee
/// ese input a través de inputActions.Player.Move y .Jump, igual
/// que con teclado o gamepad físico. No se necesita código adicional.
///
/// GamepadDetector oculta estos controles automáticamente cuando
/// detecta un gamepad físico conectado (USB-C o Bluetooth).
/// ──────────────────────────────────────────────────────────────────
/// </summary>
public class OnScreenControlsSetup : MonoBehaviour
{
    [Header("Referencias (solo para validación en editor)")]
    [Tooltip("El GameObject que tiene el componente OnScreenStick")]
    [SerializeField] private OnScreenStick joystick;
    [Tooltip("El GameObject que tiene el componente OnScreenButton de salto")]
    [SerializeField] private OnScreenButton jumpButton;
    [Tooltip("(Opcional) Botón de pausa on-screen")]
    [SerializeField] private OnScreenButton pauseButton;

    private void Start()
    {
        ValidateSetup();
    }

    private void ValidateSetup()
    {
#if UNITY_EDITOR
        if (joystick == null)
            Debug.LogWarning("[OnScreenControlsSetup] No hay OnScreenStick asignado. " +
                             "El joystick virtual no funcionará.", this);

        if (jumpButton == null)
            Debug.LogWarning("[OnScreenControlsSetup] No hay OnScreenButton de salto asignado. " +
                             "El botón de salto virtual no funcionará.", this);
#endif
    }
}

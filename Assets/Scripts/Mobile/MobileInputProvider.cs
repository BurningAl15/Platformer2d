using UnityEngine;
using UnityEngine.InputSystem.OnScreen;

/// <summary>
/// Proveedor de input para controles on-screen (mobile).
///
/// ARQUITECTURA:
/// Los botones/joystick on-screen de Unity Input System (OnScreenStick,
/// OnScreenButton) se conectan directamente al Input System y alimentan
/// las mismas Actions que el teclado o el gamepad físico.
/// Este script NO necesita hacer nada para el joystick o el botón de salto
/// si esos controles tienen OnScreenStick / OnScreenButton asignados.
///
/// Lo que SÍ hace este script:
/// - Expone SetMoveInput() / SetJumpPressed() para casos donde
///   se usen botones custom que no sean OnScreenStick/OnScreenButton.
/// - Sirve de documentación del setup esperado en la escena.
///
/// SETUP REQUERIDO EN UNITY EDITOR (ver handoff.md):
/// 1. En el GameObject del joystick virtual, agregar componente OnScreenStick
///    y asignar: Control Path = "<Gamepad>/leftStick"
/// 2. En el GameObject del botón de salto, agregar OnScreenButton
///    y asignar: Control Path = "<Gamepad>/buttonSouth"
/// 3. Estos controles alimentarán automáticamente el InputManager
///    sin ningún código adicional.
/// </summary>
public class MobileInputProvider : MonoBehaviour
{
    // Input que puede ser alimentado desde botones on-screen custom
    // (alternativos al OnScreenStick/OnScreenButton nativo)
    private Vector2 joystickInput;
    private bool jumpPressed;

    /// <summary>
    /// Llamar desde el joystick on-screen si NO se usa OnScreenStick nativo.
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        joystickInput = input;
    }

    /// <summary>
    /// Llamar desde el botón de salto on-screen si NO se usa OnScreenButton nativo.
    /// </summary>
    public void SetJumpPressed(bool pressed)
    {
        jumpPressed = pressed;
    }

    public Vector2 GetMoveInput() => joystickInput;
    public bool GetJumpPressed() => jumpPressed;
}

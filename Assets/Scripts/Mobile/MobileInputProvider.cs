using UnityEngine;

public class MobileInputProvider : MonoBehaviour
{
    private Vector2 joystickInput;
    private bool jumpPressed;
    
    public Vector2 GetMoveInput() => joystickInput;
    public bool GetJumpPressed() => jumpPressed;
}


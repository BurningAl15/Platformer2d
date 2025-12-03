using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInputSystem inputActions;

    public System.Action OnJumpPressed;
    public System.Action OnJumpReleased;
    public System.Action OnPausePressed;
    public System.Action OnSubmitPressed;
    
    [SerializeField] private MobileInputProvider mobileInput;
    
    public Vector2 MoveInput => inputActions.Player.Move.ReadValue<Vector2>();

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
        
        inputActions.Player.Jump.performed += ctx => OnJumpPressed?.Invoke();
        inputActions.Player.Jump.canceled += ctx => OnJumpReleased?.Invoke();
        inputActions.UI.Pause.performed += ctx => OnPausePressed?.Invoke();
        inputActions.UI.Submit.performed += ctx => OnSubmitPressed?.Invoke();
    }

    private void OnEnable()
    {
        EnablePlayerInput();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    public void EnablePlayerInput()
    {
        inputActions.Player.Enable();
        inputActions.UI.Enable();
    }

    public void EnableUIOnlyInput()
    {
        inputActions.Player.Disable();
        inputActions.UI.Enable();
    }

    public void DisableAllInput()
    {
        inputActions.Player.Disable();
        inputActions.UI.Disable();
    }
    
    public Vector2 GetNavigationInput()
    {
        if (inputActions.UI.Navigate != null)
        {
            return inputActions.UI.Navigate.ReadValue<Vector2>();
        }
        return inputActions.Player.Move.ReadValue<Vector2>();
    }
}
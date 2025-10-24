using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;
    
    [Header("Input Values")]
    public Vector2 moveInput;
    public bool jumpPressed;
    public bool jumpHeld;
    public bool dashPressed;
    
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    
    private void Awake()
    {
        // Get actions from the input action asset
        if (inputActions != null)
        {
            moveAction = inputActions.FindAction("Move");
            jumpAction = inputActions.FindAction("Jump");
            dashAction = inputActions.FindAction("Attack"); // Using Attack action for dash
        }
    }
    
    private void OnEnable()
    {
        if (inputActions != null)
        {
            inputActions.Enable();
        }
    }
    
    private void OnDisable()
    {
        if (inputActions != null)
        {
            inputActions.Disable();
        }
    }
    
    private void Update()
    {
        if (moveAction == null || jumpAction == null || dashAction == null) return;
        
        // Get input values
        moveInput = moveAction.ReadValue<Vector2>();
        jumpPressed = jumpAction.WasPressedThisFrame();
        jumpHeld = jumpAction.IsPressed();
        dashPressed = dashAction.WasPressedThisFrame();
        
    }
    
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }
    
    public bool GetJumpPressed()
    {
        return jumpPressed;
    }
    
    public bool GetJumpHeld()
    {
        return jumpHeld;
    }
    
    public bool GetDashPressed()
    {
        return dashPressed;
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using TarodevController;

public class PlayerOrbManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    
    [Header("Current Orb State")]
    [SerializeField] private OrbStats currentOrbStats;
    [SerializeField] private int currentStackCount = 0;
    [SerializeField] private OrbAbility currentAbility = OrbAbility.Jump;
    
    [Header("Jump Counter")]
    [SerializeField] private int remainingAirJumps = 0;
    
    [Header("Dash Settings")]
    [SerializeField] private float dashCooldownTimer = 0f;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private Vector2 dashDirection;
    [SerializeField] private float dashTimer = 0f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private Rigidbody2D playerRigidbody;
    private Vector2 originalVelocity;
    
    private void Awake()
    {
        // Get components
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
            
        playerRigidbody = GetComponent<Rigidbody2D>();
        
        // Don't change the initial sprite color - let the original sprite stay as is
    }
    
    private void Update()
    {
        // Only handle dash if we have dash ability
        if (currentAbility == OrbAbility.Dash)
        {
            HandleDashInput();
            UpdateDashCooldown();
            UpdateDashMovement();
        }
    }
    
    private void HandleDashInput()
    {
        if (currentAbility != OrbAbility.Dash) return;
        if (isDashing) return;
        if (dashCooldownTimer > 0) return;
        
        var keyboard = Keyboard.current;
        var gamepad = Gamepad.current;
        
        bool dashInput = false;
        Vector2 inputDirection = Vector2.zero;
        
        // Check keyboard input
        if (keyboard != null)
        {
            dashInput = keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame;
            
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) inputDirection.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) inputDirection.x += 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) inputDirection.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) inputDirection.y -= 1f;
        }
        
        // Check gamepad input
        if (gamepad != null)
        {
            dashInput = dashInput || gamepad.buttonWest.wasPressedThisFrame; // Left bumper
            Vector2 gamepadInput = gamepad.leftStick.ReadValue();
            if (gamepadInput.magnitude > inputDirection.magnitude)
            {
                inputDirection = gamepadInput;
            }
        }
        
        if (dashInput && inputDirection.magnitude > 0.1f)
        {
            StartDash(inputDirection.normalized);
        }
    }
    
    private void StartDash(Vector2 direction)
    {
        if (currentOrbStats == null || playerRigidbody == null) return;
        
        isDashing = true;
        dashDirection = direction;
        dashTimer = currentOrbStats.dashDistance / currentOrbStats.dashSpeed;
        dashCooldownTimer = currentOrbStats.dashCooldown;
        
        // Store original velocity
        originalVelocity = playerRigidbody.linearVelocity;
        
        if (debugMode)
        {
            Debug.Log($"Started dash in direction: {direction} for {dashTimer} seconds");
        }
    }
    
    private void UpdateDashCooldown()
    {
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
    }
    
    private void UpdateDashMovement()
    {
        if (!isDashing) return;
        if (playerRigidbody == null) return;
        
        dashTimer -= Time.deltaTime;
        
        if (dashTimer <= 0)
        {
            EndDash();
        }
        else
        {
            // Apply dash movement - only override horizontal movement, preserve vertical
            float dashSpeed = currentOrbStats.dashSpeed * currentStackCount;
            Vector2 dashVelocity = dashDirection * dashSpeed;
            playerRigidbody.linearVelocity = new Vector2(dashVelocity.x, playerRigidbody.linearVelocity.y);
        }
    }
    
    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
        
        // Only restore velocity if we have a valid rigidbody
        if (playerRigidbody != null)
        {
            // Restore original velocity (but maintain dash momentum)
            playerRigidbody.linearVelocity = dashDirection * currentOrbStats.dashSpeed * 0.5f;
        }
        
        if (debugMode)
        {
            Debug.Log("Dash ended");
        }
    }
    
    public void CollectOrb(OrbStats orbStats)
    {
        if (orbStats == null) return;
        
        // Check if this is a different color orb
        if (currentOrbStats != null && currentOrbStats.orbColor != orbStats.orbColor)
        {
            // Switch to new color and reset stacks
            currentStackCount = 1;
            currentOrbStats = orbStats;
            currentAbility = orbStats.ability;
            
            // Don't change color since player is already red
            
            if (debugMode)
            {
                Debug.Log($"Switched to {orbStats.orbName} (new color), stacks reset to 1");
            }
        }
        else
        {
            // Same color orb - increase stacks
            if (currentOrbStats == null)
            {
                currentOrbStats = orbStats;
                currentAbility = orbStats.ability;
                
                // Don't change color since player is already red
            }
            
            currentStackCount = Mathf.Min(currentStackCount + 1, orbStats.maxStacks);
            
            if (debugMode)
            {
                Debug.Log($"Collected {orbStats.orbName}, stacks: {currentStackCount}");
            }
        }
        
        // Update air jump counter for red orbs
        if (orbStats.ability == OrbAbility.Jump)
        {
            remainingAirJumps = currentStackCount;
        }
        
        // Reset dash cooldown when collecting dash orbs
        if (orbStats.ability == OrbAbility.Dash)
        {
            dashCooldownTimer = 0f;
        }
    }
    
    public int GetCurrentStackCount()
    {
        return currentStackCount;
    }
    
    public OrbAbility GetCurrentAbility()
    {
        return currentAbility;
    }
    
    public OrbStats GetCurrentOrbStats()
    {
        return currentOrbStats;
    }
    
    public bool CanJump()
    {
        // Player can always jump, orbs just enhance the power
        return true;
    }
    
    public float GetJumpPowerMultiplier()
    {
        if (currentAbility != OrbAbility.Jump || currentOrbStats == null) return 1f;
        return Mathf.Pow(currentOrbStats.jumpPowerMultiplier, currentStackCount);
    }
    
    public bool CanDash()
    {
        return currentAbility == OrbAbility.Dash && currentOrbStats != null && currentStackCount > 0 && dashCooldownTimer <= 0 && !isDashing;
    }
    
    public bool CanAirJump()
    {
        return remainingAirJumps > 0;
    }
    
    public void ConsumeAirJump()
    {
        if (remainingAirJumps > 0)
        {
            remainingAirJumps--;
            if (debugMode)
            {
                Debug.Log($"Used air jump, remaining: {remainingAirJumps}");
            }
        }
    }
    
    public void ResetAirJumps()
    {
        remainingAirJumps = 0;
        if (debugMode)
        {
            Debug.Log("Air jumps reset (landed on ground)");
        }
    }
    
    public int GetRemainingAirJumps()
    {
        return remainingAirJumps;
    }
}

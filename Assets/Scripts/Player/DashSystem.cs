using UnityEngine;
using TarodevController;

/// <summary>
/// Handles all dash-related functionality for the player.
/// Extracted from PlayerOrbManager for better modularity.
/// </summary>
public class DashSystem : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private int remainingDashes = 0;
    [SerializeField] private float dashCooldownTimer = 0f;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private Vector2 dashDirection;
    [SerializeField] private float dashTimer = 0f;
    
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private ScriptableStats playerStats;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Events
    public System.Action<int> OnDashCountChanged;
    
    private Rigidbody2D playerRigidbody;
    
    // Properties
    public bool IsDashing => isDashing;
    public int RemainingDashes => remainingDashes;
    
    private void Awake()
    {
        // Get components
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        if (inputHandler == null)
            inputHandler = GetComponent<PlayerInputHandler>();
        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
            
        playerRigidbody = GetComponent<Rigidbody2D>();
    }
    
    private void Update()
    {
        HandleDashInput();
        UpdateDashCooldown();
    }
    
    private void FixedUpdate()
    {
        UpdateDashMovement();
    }
    
    private void HandleDashInput()
    {
        if (inputHandler == null) 
        {
            if (debugMode)
            {
                Debug.Log("DashSystem: InputHandler is null!");
            }
            return;
        }
        
        bool dashInput = inputHandler.GetDashPressed();
        if (debugMode && dashInput)
        {
            Debug.Log($"DashSystem: Dash input detected! Remaining dashes: {remainingDashes}, Is dashing: {isDashing}");
        }
        
        if (isDashing) return;
        if (remainingDashes <= 0) 
        {
            if (debugMode && dashInput)
            {
                Debug.Log($"DashSystem: Dash input detected but no remaining dashes: {remainingDashes}");
            }
            return;
        }
        
        Vector2 inputDirection = inputHandler.GetMoveInput();
        
        if (dashInput)
        {
            Vector2 dashDirection = CalculateDashDirection(inputDirection);
            StartDash(dashDirection);
        }
    }
    
    private Vector2 CalculateDashDirection(Vector2 inputDirection)
    {
        // Only allow horizontal dashes (left/right)
        if (Mathf.Abs(inputDirection.x) > 0.1f)
        {
            // Use horizontal input direction only (ignore vertical)
            Vector2 direction = inputDirection.x > 0 ? Vector2.right : Vector2.left;
            if (debugMode)
            {
                Debug.Log($"DashSystem: Using horizontal input direction: {direction}");
            }
            return direction;
        }
        else
        {
            // No horizontal input, use character facing direction from sprite
            Vector2 direction = GetFacingDirection();
            if (debugMode)
            {
                Debug.Log($"DashSystem: Using character facing direction: {direction}");
            }
            return direction;
        }
    }
    
    private Vector2 GetFacingDirection()
    {
        if (playerSpriteRenderer != null)
        {
            // Check if sprite is flipped (facing left)
            bool facingLeft = playerSpriteRenderer.flipX;
            return facingLeft ? Vector2.left : Vector2.right;
        }
        else
        {
            // Fallback: check transform scale
            return transform.localScale.x < 0 ? Vector2.left : Vector2.right;
        }
    }
    
    private void StartDash(Vector2 direction)
    {
        if (playerStats == null) 
        {
            Debug.LogError("DashSystem: PlayerStats is null! Cannot start dash.");
            return;
        }
        if (playerRigidbody == null) 
        {
            Debug.LogError("DashSystem: PlayerRigidbody is null! Cannot start dash.");
            return;
        }
        
        isDashing = true;
        dashDirection = direction;
        dashTimer = playerStats.DashDistance / playerStats.DashSpeed;
        dashCooldownTimer = playerStats.DashCooldown;
        
        if (debugMode)
        {
            Debug.Log($"DashSystem: Dash timer calculation: {playerStats.DashDistance} / {playerStats.DashSpeed} = {dashTimer}");
        }
        
        // Consume one dash
        remainingDashes--;
        OnDashCountChanged?.Invoke(remainingDashes);
        
        // Play dash SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDashSFX();
        }
        
        if (debugMode)
        {
            Debug.Log($"DashSystem: Started dash in direction: {direction} for {dashTimer} seconds, remaining dashes: {remainingDashes}");
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
        
        dashTimer -= Time.fixedDeltaTime;
        
        if (dashTimer <= 0)
        {
            EndDash();
        }
        else
        {
            // Check for wall collision during dash
            if (CheckWallCollision())
            {
                if (debugMode)
                {
                    Debug.Log("DashSystem: Dash ended early due to wall collision");
                }
                EndDash();
                return;
            }
            
            // During dash: suspend gravity, maintain horizontal movement
            float dashSpeed = playerStats.DashSpeed;
            Vector2 dashVelocity = dashDirection * dashSpeed;
            
            // Apply dash velocity with NO vertical movement (gravity suspended)
            playerRigidbody.linearVelocity = dashVelocity;
        }
    }
    
    private bool CheckWallCollision()
    {
        if (playerController == null) return false;
        
        // Use the same collision detection as PlayerController but for walls
        Collider2D col = GetComponent<Collider2D>();
        if (col == null) return false;
        
        // Check for horizontal wall collision in dash direction
        Vector2 rayDirection = dashDirection;
        float rayDistance = 0.3f; // Increased distance to reduce false positives
        
        // Cast a ray in the dash direction to detect walls
        RaycastHit2D hit = Physics2D.BoxCast(
            col.bounds.center, 
            col.bounds.size, 
            0, 
            rayDirection, 
            rayDistance, 
            ~playerStats.PlayerLayer
        );
        
        // Only end dash if we hit a solid wall (not triggers)
        return hit.collider != null && !hit.collider.isTrigger;
    }
    
    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
        
        // Sync PlayerController's frame velocity with current rigidbody velocity
        if (playerController != null && playerRigidbody != null)
        {
            // Get the current rigidbody velocity
            Vector2 currentVelocity = playerRigidbody.linearVelocity;
            
            // Preserve horizontal momentum but reduce it slightly for more natural feel
            // Keep vertical velocity as-is to maintain natural physics
            Vector2 newVelocity = new Vector2(currentVelocity.x * 0.8f, currentVelocity.y);
            playerRigidbody.linearVelocity = newVelocity;
            
            if (debugMode)
            {
                Debug.Log($"DashSystem: Dash ended - synced velocity: {newVelocity}");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.Log("DashSystem: Dash ended - returning control to PlayerController");
            }
        }
    }
    
    /// <summary>
    /// Adds dashes to the player's remaining count.
    /// </summary>
    public void AddDashes(int count)
    {
        remainingDashes += count;
        OnDashCountChanged?.Invoke(remainingDashes);
        
        if (debugMode)
        {
            Debug.Log($"DashSystem: Added {count} dashes. Total: {remainingDashes}");
        }
    }
    
    /// <summary>
    /// Resets the dash count to zero.
    /// </summary>
    public void ResetDashCount()
    {
        remainingDashes = 0;
        OnDashCountChanged?.Invoke(remainingDashes);
        
        if (debugMode)
        {
            Debug.Log("DashSystem: Dash count reset to 0");
        }
    }
    
    /// <summary>
    /// Checks if the player can dash.
    /// </summary>
    public bool CanDash()
    {
        return remainingDashes > 0 && !isDashing && dashCooldownTimer <= 0;
    }
}

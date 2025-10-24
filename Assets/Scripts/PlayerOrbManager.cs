using UnityEngine;
using UnityEngine.InputSystem;
using TarodevController;
using System;

public class PlayerOrbManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    [SerializeField] private ScriptableStats playerStats;
    [SerializeField] private PlayerInputHandler inputHandler;
    
    [Header("Character Sprites")]
    [SerializeField] private Sprite redSprite;    // Default red character
    [SerializeField] private Sprite greenSprite; // Green character for dash ability
    
    [Header("Current Orb State")]
    [SerializeField] private OrbStats currentOrbStats;
    [SerializeField] private int currentStackCount = 0;
    [SerializeField] private OrbAbility currentAbility = OrbAbility.Jump;
    
    [Header("Jump Counter")]
    [SerializeField] private int remainingAirJumps = 0;
    
    [Header("Dash Counter")]
    [SerializeField] private int remainingDashes = 0;
    
    [Header("Dash Settings")]
    [SerializeField] private float dashCooldownTimer = 0f;
    [SerializeField] private bool isDashing = false;
    [SerializeField] private Vector2 dashDirection;
    [SerializeField] private float dashTimer = 0f;
    
    [Header("Debug")]
    [SerializeField] public bool debugMode = true;
    
    // Events for UI updates
    public event Action<int, int> OnOrbStacksChanged; // redStacks, greenStacks
    
    private Rigidbody2D playerRigidbody;
    
    private void Awake()
    {
        // Get components
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
            
        playerRigidbody = GetComponent<Rigidbody2D>();
        
        // Store the initial red sprite if not assigned
        if (redSprite == null && playerSpriteRenderer != null)
        {
            redSprite = playerSpriteRenderer.sprite;
        }
    }
    
    private void Update()
    {
        // Always handle dash input and movement, regardless of current ability
        HandleDashInput();
        UpdateDashCooldown();
        UpdateDashMovement();
    }
    
    private void HandleDashInput()
    {
        if (inputHandler == null) 
        {
            if (debugMode)
            {
                Debug.Log("InputHandler is null!");
            }
            return;
        }
        
        bool dashInput = inputHandler.GetDashPressed();
        if (debugMode && dashInput)
        {
            Debug.Log($"Dash input detected! Current ability: {currentAbility}, Remaining dashes: {remainingDashes}, Is dashing: {isDashing}");
        }
        
        if (currentAbility != OrbAbility.Dash) 
        {
            if (debugMode && dashInput)
            {
                Debug.Log($"Dash input detected but current ability is: {currentAbility}, need Dash ability");
            }
            return;
        }
        if (isDashing) return;
        if (remainingDashes <= 0) 
        {
            if (debugMode && dashInput)
            {
                Debug.Log($"Dash input detected but no remaining dashes: {remainingDashes}");
            }
            return;
        }
        
        Vector2 inputDirection = inputHandler.GetMoveInput();
        
        if (dashInput)
        {
            Vector2 dashDirection = Vector2.zero;
            
            if (inputDirection.magnitude > 0.1f)
            {
                // Use current input direction
                dashDirection = inputDirection.normalized;
                if (debugMode)
                {
                    Debug.Log($"Dash input detected! Using input direction: {dashDirection}");
                }
            }
            else
            {
                // No input direction, use character facing direction from sprite
                if (playerSpriteRenderer != null)
                {
                    // Check if sprite is flipped (facing left)
                    bool facingLeft = playerSpriteRenderer.flipX;
                    dashDirection = facingLeft ? Vector2.left : Vector2.right;
                }
                else
                {
                    // Fallback: check transform scale
                    if (transform.localScale.x < 0)
                    {
                        dashDirection = Vector2.left;
                    }
                    else
                    {
                        dashDirection = Vector2.right;
                    }
                }
                
                if (debugMode)
                {
                    Debug.Log($"Dash input detected! Using character facing direction: {dashDirection}");
                }
            }
            
            StartDash(dashDirection);
        }
    }
    
    private void StartDash(Vector2 direction)
    {
        if (playerStats == null) 
        {
            Debug.LogError("PlayerStats is null! Cannot start dash.");
            return;
        }
        if (playerRigidbody == null) 
        {
            Debug.LogError("PlayerRigidbody is null! Cannot start dash.");
            return;
        }
        
        isDashing = true;
        dashDirection = direction;
        dashTimer = playerStats.DashDistance / playerStats.DashSpeed;
        dashCooldownTimer = playerStats.DashCooldown;
        
        if (debugMode)
        {
            Debug.Log($"Dash timer calculation: {playerStats.DashDistance} / {playerStats.DashSpeed} = {dashTimer}");
        }
        
        // Consume one dash
        remainingDashes--;
        
        // Update UI after consuming dash
        TriggerUIUpdate();
        
        // Play dash SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDashSFX();
        }
        
        
        if (debugMode)
        {
            Debug.Log($"Started dash in direction: {direction} for {dashTimer} seconds, remaining dashes: {remainingDashes}");
            Debug.Log($"Dash parameters - Distance: {playerStats.DashDistance}, Speed: {playerStats.DashSpeed}");
            Debug.Log($"Player position before dash: {transform.position}");
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
            // During dash: suspend gravity, maintain horizontal movement
            float dashSpeed = playerStats.DashSpeed;
            Vector2 dashVelocity = dashDirection * dashSpeed;
            
            // Apply dash velocity with NO vertical movement (gravity suspended)
            playerRigidbody.linearVelocity = dashVelocity;
            
            if (debugMode)
            {
                Debug.Log($"Dashing! Speed: {dashSpeed}, Direction: {dashDirection}, Velocity: {playerRigidbody.linearVelocity}");
            }
        }
    }
    
    private void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
        
        // Don't reset anything - let PlayerController handle it naturally
        if (debugMode)
        {
            Debug.Log("Dash ended - returning control to PlayerController");
        }
    }
    
    private void ChangePlayerSprite(OrbAbility ability)
    {
        if (playerSpriteRenderer == null) return;
        
        switch (ability)
        {
            case OrbAbility.Jump:
                // Red sprite for jump ability
                if (redSprite != null)
                {
                    playerSpriteRenderer.sprite = redSprite;
                    if (debugMode)
                    {
                        Debug.Log("Changed to red sprite (Jump ability)");
                    }
                }
                break;
                
            case OrbAbility.Dash:
                // Green sprite for dash ability
                if (greenSprite != null)
                {
                    playerSpriteRenderer.sprite = greenSprite;
                    if (debugMode)
                    {
                        Debug.Log("Changed to green sprite (Dash ability)");
                    }
                }
                break;
        }
    }
    
    private void TriggerUIUpdate()
    {
        // Calculate current stacks based on remaining abilities
        int redStacks = (currentAbility == OrbAbility.Jump) ? remainingAirJumps : 0;
        int greenStacks = (currentAbility == OrbAbility.Dash) ? remainingDashes : 0;
        
        // Trigger UI update event
        OnOrbStacksChanged?.Invoke(redStacks, greenStacks);
        
        if (debugMode)
        {
            Debug.Log($"UI Update: Red stacks: {redStacks}, Green stacks: {greenStacks}");
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
            
            // Change sprite to match the new ability
            ChangePlayerSprite(orbStats.ability);
            
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
                
                // Change sprite to match the ability
                ChangePlayerSprite(orbStats.ability);
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
            // Each red orb gives 1 air jump, add to existing count
            remainingAirJumps += 1;
            if (debugMode)
            {
                Debug.Log($"Red orb collected! Stacks: {currentStackCount}, Air jumps: {remainingAirJumps}");
            }
        }
        
        // Update dash counter for green orbs
        if (orbStats.ability == OrbAbility.Dash)
        {
            // Each green orb gives 1 dash, add to existing count
            remainingDashes += 1;
            if (debugMode)
            {
                Debug.Log($"Green orb collected! Stacks: {currentStackCount}, Dashes: {remainingDashes}, Ability: {currentAbility}");
            }
        }
        
        // Update UI after collecting orb
        TriggerUIUpdate();
        
        // Play orb collect SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOrbCollectSFX();
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
                Debug.Log($"Used air jump! Remaining: {remainingAirJumps}");
            }
            
            // Update UI after consuming air jump
            TriggerUIUpdate();
        }
        else
        {
            if (debugMode)
            {
                Debug.Log("Tried to use air jump but none remaining!");
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
        
        // Update UI after resetting air jumps
        TriggerUIUpdate();
    }
    
    public int GetRemainingAirJumps()
    {
        return remainingAirJumps;
    }
    
    public void ResetDashCount()
    {
        remainingDashes = 0;
        if (debugMode)
        {
            Debug.Log("Dash count reset to 0");
        }
        
        // Update UI after resetting dash count
        TriggerUIUpdate();
    }
    
    public bool IsDashing()
    {
        return isDashing;
    }
    
    public int GetRemainingDashes()
    {
        return remainingDashes;
    }
    
}

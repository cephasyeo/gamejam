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
    [SerializeField] private Sprite whiteSprite;  // Default white character
    [SerializeField] private Sprite redSprite;    // Red character for jump ability
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
    
    [Header("Yellow Orb Shooting")]
    [SerializeField] private bool canShootYellowOrbs = false;
    [SerializeField] private GameObject yellowOrbPrefab;
    [SerializeField] private float yellowOrbFireRate = 0.5f; // Time between shots
    [SerializeField] private float yellowOrbSpeed = 15f;
    [SerializeField] private float yellowOrbLifetime = 5f; // How long the orb exists before destroying
    [SerializeField] private float lastShotTime = 0f;
    
    [Header("Debug")]
    [SerializeField] public bool debugMode = false; // Disabled for better performance
    
    // Events for UI updates
    public event Action<int, int> OnOrbStacksChanged; // redStacks, greenStacks
    
    private Rigidbody2D playerRigidbody;
    
    // Track if player has ever collected an orb to preserve color
    private bool hasEverCollectedOrb = false;
    
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
        // Handle dash input in Update for responsive input
        HandleDashInput();
        UpdateDashCooldown();
    }
    
    private void FixedUpdate()
    {
        // Handle dash movement in FixedUpdate for consistent physics timing
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
            
            // Only allow horizontal dashes (left/right)
            if (Mathf.Abs(inputDirection.x) > 0.1f)
            {
                // Use horizontal input direction only (ignore vertical)
                dashDirection = inputDirection.x > 0 ? Vector2.right : Vector2.left;
                if (debugMode)
                {
                    Debug.Log($"Dash input detected! Using horizontal input direction: {dashDirection}");
                }
            }
            else
            {
                // No horizontal input, use character facing direction from sprite
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
            dashCooldownTimer -= Time.deltaTime; // Use deltaTime since this is in Update()
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
                    Debug.Log("Dash ended early due to wall collision");
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
                Debug.Log($"Dash ended - synced velocity: {newVelocity}");
            }
        }
        else
        {
            if (debugMode)
            {
                Debug.Log("Dash ended - returning control to PlayerController");
            }
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
    
    private void SetDefaultSprite()
    {
        if (playerSpriteRenderer == null) return;
        
        // Set to white/default sprite when no orbs are collected
        if (whiteSprite != null)
        {
            playerSpriteRenderer.sprite = whiteSprite;
            if (debugMode)
            {
                Debug.Log("Changed to white sprite (default)");
            }
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
        
        // Handle white orb (reset orb) - always resets player to default
        if (orbStats.ability == OrbAbility.Reset)
        {
            ResetPlayerToDefault();
            
            if (debugMode)
            {
                Debug.Log("White orb collected - player reset to default state");
            }
            return;
        }
        
        // Mark that player has collected an orb
        hasEverCollectedOrb = true;
        
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
    
    public void ClearAllOrbs()
    {
        // Reset all orb counters
        remainingAirJumps = 0;
        remainingDashes = 0;
        
        // Reset orb stats and ability
        currentOrbStats = null;
        currentAbility = OrbAbility.Jump;
        currentStackCount = 0;
        
        // Only change back to default white sprite if player has never collected an orb
        if (!hasEverCollectedOrb)
        {
            SetDefaultSprite();
        }
        
        if (debugMode)
        {
            Debug.Log($"All orbs cleared - player landed on ground. Has ever collected orb: {hasEverCollectedOrb}");
        }
        
        // Update UI to reflect cleared orbs
        TriggerUIUpdate();
    }
    
    public void ResetPlayerToDefault()
    {
        // Reset all orb counters
        remainingAirJumps = 0;
        remainingDashes = 0;
        
        // Reset orb stats and ability
        currentOrbStats = null;
        currentAbility = OrbAbility.Jump;
        currentStackCount = 0;
        
        // Reset the flag so player goes back to white sprite
        hasEverCollectedOrb = false;
        
        // Change back to default white sprite
        SetDefaultSprite();
        
        if (debugMode)
        {
            Debug.Log("Player reset to default state (respawned)");
        }
        
        // Update UI to reflect cleared orbs
        TriggerUIUpdate();
    }
    
    /// <summary>
    /// Enables the yellow orb shooting ability.
    /// </summary>
    public void EnableYellowOrbShooting()
    {
        canShootYellowOrbs = true;
        
        if (debugMode)
        {
            Debug.Log("Yellow orb shooting ability enabled!");
        }
    }
    
    /// <summary>
    /// Disables the yellow orb shooting ability.
    /// </summary>
    public void DisableYellowOrbShooting()
    {
        canShootYellowOrbs = false;
        
        if (debugMode)
        {
            Debug.Log("Yellow orb shooting ability disabled!");
        }
    }
    
    /// <summary>
    /// Checks if the player can shoot yellow orbs.
    /// </summary>
    public bool CanShootYellowOrbs()
    {
        return canShootYellowOrbs;
    }
    
    /// <summary>
    /// Attempts to shoot a yellow orb towards the target position.
    /// </summary>
    public bool TryShootYellowOrb(Vector2 targetPosition)
    {
        if (!canShootYellowOrbs || yellowOrbPrefab == null)
        {
            return false;
        }
        
        // Check fire rate
        if (Time.time - lastShotTime < yellowOrbFireRate)
        {
            return false;
        }
        
        // Calculate direction from player to target
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Create yellow orb
        GameObject yellowOrb = Instantiate(yellowOrbPrefab, transform.position, Quaternion.identity);
        
        // Set up the yellow orb
        YellowOrb yellowOrbScript = yellowOrb.GetComponent<YellowOrb>();
        if (yellowOrbScript != null)
        {
            yellowOrbScript.Initialize(direction, yellowOrbSpeed, yellowOrbLifetime);
        }
        
        // Update last shot time
        lastShotTime = Time.time;
        
        if (debugMode)
        {
            Debug.Log($"Shot yellow orb towards {targetPosition}");
        }
        
        return true;
    }
    
    /// <summary>
    /// Sets the yellow orb fire rate.
    /// </summary>
    public void SetYellowOrbFireRate(float newFireRate)
    {
        yellowOrbFireRate = newFireRate;
        
        if (debugMode)
        {
            Debug.Log($"Yellow orb fire rate set to: {newFireRate}");
        }
    }
    
    /// <summary>
    /// Gets the current yellow orb fire rate.
    /// </summary>
    public float GetYellowOrbFireRate()
    {
        return yellowOrbFireRate;
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

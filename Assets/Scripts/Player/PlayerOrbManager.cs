using UnityEngine;
using TarodevController;
using System;

/// <summary>
/// Main coordinator for player orb-related functionality.
/// Delegates to specialized components for better modularity.
/// </summary>
public class PlayerOrbManager : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private DashSystem dashSystem;
    [SerializeField] private YellowOrbShooter yellowOrbShooter;
    [SerializeField] private PlayerSpriteManager spriteManager;
    [SerializeField] private OrbCollector orbCollector;
    
    [Header("Debug")]
    [SerializeField] public bool debugMode = false;
    
    // Events for UI updates (delegated from components)
    public event Action<int, int> OnOrbStacksChanged; // redStacks, greenStacks
    
    private void Awake()
    {
        // Get components if not assigned
        if (dashSystem == null)
            dashSystem = GetComponent<DashSystem>();
        if (yellowOrbShooter == null)
            yellowOrbShooter = GetComponent<YellowOrbShooter>();
        if (spriteManager == null)
            spriteManager = GetComponent<PlayerSpriteManager>();
        if (orbCollector == null)
            orbCollector = GetComponent<OrbCollector>();
        
        // Subscribe to component events
        SubscribeToComponentEvents();
    }
    
    /// <summary>
    /// Subscribes to component events for coordination.
    /// </summary>
    private void SubscribeToComponentEvents()
    {
        // Subscribe to orb collector events
        if (orbCollector != null)
        {
            orbCollector.OnOrbStacksChanged += OnOrbStacksChanged;
            orbCollector.OnOrbCollected += OnOrbCollected;
            orbCollector.OnPlayerReset += OnPlayerReset;
        }
        
        // Subscribe to dash system events
        if (dashSystem != null)
        {
            dashSystem.OnDashCountChanged += OnDashCountChanged;
        }
        
        // Subscribe to sprite manager events
        if (spriteManager != null)
        {
            spriteManager.OnSpriteChanged += OnSpriteChanged;
        }
    }
    
    /// <summary>
    /// Handles orb collection events from OrbCollector.
    /// </summary>
    private void OnOrbCollected(OrbStats orbStats)
    {
        // Update sprite when orb is collected
        if (spriteManager != null)
        {
            spriteManager.ChangePlayerSprite(orbStats.ability);
        }
        
        // Update dash count when green orb is collected
        if (orbStats.ability == OrbAbility.Dash && dashSystem != null)
        {
            dashSystem.AddDashes(1);
        }
    }
    
    /// <summary>
    /// Handles player reset events from OrbCollector.
    /// </summary>
    private void OnPlayerReset()
    {
        // Reset sprite to default when player is reset
        if (spriteManager != null)
        {
            spriteManager.SetDefaultSprite();
        }
        
        // Reset dash count when player is reset
        if (dashSystem != null)
        {
            dashSystem.ResetDashCount();
        }
    }
    
    /// <summary>
    /// Handles dash count changes from DashSystem.
    /// </summary>
    private void OnDashCountChanged(int remainingDashes)
    {
        // Trigger UI update with current dash count
        TriggerUIUpdate();
    }
    
    /// <summary>
    /// Handles sprite changes from PlayerSpriteManager.
    /// </summary>
    private void OnSpriteChanged(OrbAbility ability)
    {
        if (debugMode)
        {
            Debug.Log($"PlayerOrbManager: Sprite changed to {ability}");
        }
    }
    
    /// <summary>
    /// Triggers UI update with current stack counts.
    /// </summary>
    private void TriggerUIUpdate()
    {
        if (orbCollector == null) return;
        
        // Get current stacks from orb collector
        int redStacks = orbCollector.RemainingAirJumps;
        int greenStacks = dashSystem != null ? dashSystem.RemainingDashes : 0;
        
        // Trigger UI update event
        OnOrbStacksChanged?.Invoke(redStacks, greenStacks);
        
        if (debugMode)
        {
            Debug.Log($"PlayerOrbManager: UI Update - Red stacks: {redStacks}, Green stacks: {greenStacks}");
        }
    }
    
    // ===== PUBLIC API METHODS (Delegation to Components) =====
    
    /// <summary>
    /// Collects an orb and updates player state.
    /// </summary>
    public void CollectOrb(OrbStats orbStats)
    {
        if (orbCollector != null)
        {
            orbCollector.CollectOrb(orbStats);
        }
    }
    
    /// <summary>
    /// Gets the current stack count.
    /// </summary>
    public int GetCurrentStackCount()
    {
        return orbCollector != null ? orbCollector.CurrentStackCount : 0;
    }
    
    /// <summary>
    /// Gets the current ability.
    /// </summary>
    public OrbAbility GetCurrentAbility()
    {
        return orbCollector != null ? orbCollector.CurrentAbility : OrbAbility.Jump;
    }
    
    /// <summary>
    /// Gets the current orb stats.
    /// </summary>
    public OrbStats GetCurrentOrbStats()
    {
        return orbCollector != null ? orbCollector.CurrentOrbStats : null;
    }
    
    /// <summary>
    /// Checks if the player can jump.
    /// </summary>
    public bool CanJump()
    {
        return true; // Player can always jump, orbs just enhance the power
    }
    
    /// <summary>
    /// Gets the jump power multiplier.
    /// </summary>
    public float GetJumpPowerMultiplier()
    {
        return orbCollector != null ? orbCollector.GetJumpPowerMultiplier() : 1f;
    }
    
    /// <summary>
    /// Checks if the player can air jump.
    /// </summary>
    public bool CanAirJump()
    {
        return orbCollector != null ? orbCollector.CanAirJump() : false;
    }
    
    /// <summary>
    /// Consumes an air jump.
    /// </summary>
    public void ConsumeAirJump()
    {
        if (orbCollector != null)
        {
            orbCollector.ConsumeAirJump();
        }
    }
    
    /// <summary>
    /// Resets air jumps.
    /// </summary>
    public void ResetAirJumps()
    {
        if (orbCollector != null)
        {
            orbCollector.ResetAirJumps();
        }
    }
    
    /// <summary>
    /// Gets remaining air jumps.
    /// </summary>
    public int GetRemainingAirJumps()
    {
        return orbCollector != null ? orbCollector.RemainingAirJumps : 0;
    }
    
    /// <summary>
    /// Clears all orbs.
    /// </summary>
    public void ClearAllOrbs()
    {
        if (orbCollector != null)
        {
            orbCollector.ClearAllOrbs();
        }
    }
    
    /// <summary>
    /// Resets player to default state.
    /// </summary>
    public void ResetPlayerToDefault()
    {
        if (orbCollector != null)
        {
            orbCollector.ResetPlayerToDefault();
        }
    }
    
    /// <summary>
    /// Checks if the player is currently dashing.
    /// </summary>
    public bool IsDashing()
    {
        return dashSystem != null ? dashSystem.IsDashing : false;
    }
    
    /// <summary>
    /// Gets remaining dashes.
    /// </summary>
    public int GetRemainingDashes()
    {
        return dashSystem != null ? dashSystem.RemainingDashes : 0;
    }
    
    /// <summary>
    /// Enables yellow orb shooting.
    /// </summary>
    public void EnableYellowOrbShooting()
    {
        if (yellowOrbShooter != null)
        {
            yellowOrbShooter.EnableYellowOrbShooting();
        }
    }
    
    /// <summary>
    /// Disables yellow orb shooting.
    /// </summary>
    public void DisableYellowOrbShooting()
    {
        if (yellowOrbShooter != null)
        {
            yellowOrbShooter.DisableYellowOrbShooting();
        }
    }
    
    /// <summary>
    /// Checks if the player can shoot yellow orbs.
    /// </summary>
    public bool CanShootYellowOrbs()
    {
        return yellowOrbShooter != null ? yellowOrbShooter.CanShootYellowOrbs() : false;
    }
    
    /// <summary>
    /// Attempts to shoot a yellow orb.
    /// </summary>
    public bool TryShootYellowOrb(Vector2 targetPosition)
    {
        return yellowOrbShooter != null ? yellowOrbShooter.TryShootYellowOrb(targetPosition) : false;
    }
    
    /// <summary>
    /// Sets the yellow orb fire rate.
    /// </summary>
    public void SetYellowOrbFireRate(float newFireRate)
    {
        if (yellowOrbShooter != null)
        {
            yellowOrbShooter.SetYellowOrbFireRate(newFireRate);
        }
    }
    
    /// <summary>
    /// Gets the current yellow orb fire rate.
    /// </summary>
    public float GetYellowOrbFireRate()
    {
        return yellowOrbShooter != null ? yellowOrbShooter.GetYellowOrbFireRate() : 0.5f;
    }
}

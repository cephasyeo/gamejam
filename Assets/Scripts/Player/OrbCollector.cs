using UnityEngine;
using System;

/// <summary>
/// Handles orb collection logic and stack management.
/// Extracted from PlayerOrbManager for better modularity.
/// </summary>
public class OrbCollector : MonoBehaviour
{
    [Header("Current Orb State")]
    [SerializeField] private OrbStats currentOrbStats;
    [SerializeField] private int currentStackCount = 0;
    [SerializeField] private OrbAbility currentAbility = OrbAbility.Jump;
    
    [Header("Jump Counter")]
    [SerializeField] private int remainingAirJumps = 0;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Track if player has ever collected an orb to preserve color
    private bool hasEverCollectedOrb = false;
    
    // Events
    public event Action<int, int> OnOrbStacksChanged; // redStacks, greenStacks
    public event Action<OrbStats> OnOrbCollected;
    public event Action OnPlayerReset;
    
    // Properties
    public int CurrentStackCount => currentStackCount;
    public OrbAbility CurrentAbility => currentAbility;
    public OrbStats CurrentOrbStats => currentOrbStats;
    public int RemainingAirJumps => remainingAirJumps;
    public bool HasEverCollectedOrb => hasEverCollectedOrb;
    
    /// <summary>
    /// Collects an orb and updates player state accordingly.
    /// </summary>
    public void CollectOrb(OrbStats orbStats)
    {
        if (orbStats == null) return;
        
        // Handle white orb (reset orb) - always resets player to default
        if (orbStats.ability == OrbAbility.Reset)
        {
            ResetPlayerToDefault();
            
            if (debugMode)
            {
                Debug.Log("OrbCollector: White orb collected - player reset to default state");
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
            
            if (debugMode)
            {
                Debug.Log($"OrbCollector: Switched to {orbStats.orbName} (new color), stacks reset to 1");
            }
        }
        else
        {
            // Same color orb - increase stacks
            if (currentOrbStats == null)
            {
                currentOrbStats = orbStats;
                currentAbility = orbStats.ability;
            }
            
            currentStackCount = Mathf.Min(currentStackCount + 1, orbStats.maxStacks);
            
            if (debugMode)
            {
                Debug.Log($"OrbCollector: Collected {orbStats.orbName}, stacks: {currentStackCount}");
            }
        }
        
        // Update air jump counter for red orbs
        if (orbStats.ability == OrbAbility.Jump)
        {
            // Each red orb gives 1 air jump, add to existing count
            remainingAirJumps += 1;
            if (debugMode)
            {
                Debug.Log($"OrbCollector: Red orb collected! Stacks: {currentStackCount}, Air jumps: {remainingAirJumps}");
            }
        }
        
        // Trigger events
        OnOrbCollected?.Invoke(orbStats);
        TriggerUIUpdate();
        
        // Play orb collect SFX
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayOrbCollectSFX();
        }
    }
    
    /// <summary>
    /// Consumes an air jump and updates counters.
    /// </summary>
    public void ConsumeAirJump()
    {
        if (remainingAirJumps > 0)
        {
            remainingAirJumps--;
            if (debugMode)
            {
                Debug.Log($"OrbCollector: Used air jump! Remaining: {remainingAirJumps}");
            }
            
            // Update UI after consuming air jump
            TriggerUIUpdate();
        }
        else
        {
            if (debugMode)
            {
                Debug.Log("OrbCollector: Tried to use air jump but none remaining!");
            }
        }
    }
    
    /// <summary>
    /// Resets air jumps to zero.
    /// </summary>
    public void ResetAirJumps()
    {
        remainingAirJumps = 0;
        if (debugMode)
        {
            Debug.Log("OrbCollector: Air jumps reset (landed on ground)");
        }
        
        // Update UI after resetting air jumps
        TriggerUIUpdate();
    }
    
    /// <summary>
    /// Clears all orbs but preserves color if player has ever collected an orb.
    /// </summary>
    public void ClearAllOrbs()
    {
        // Reset all orb counters
        remainingAirJumps = 0;
        
        // Reset orb stats and ability
        currentOrbStats = null;
        currentAbility = OrbAbility.Jump;
        currentStackCount = 0;
        
        if (debugMode)
        {
            Debug.Log($"OrbCollector: All orbs cleared - player landed on ground. Has ever collected orb: {hasEverCollectedOrb}");
        }
        
        // Update UI to reflect cleared orbs
        TriggerUIUpdate();
    }
    
    /// <summary>
    /// Resets player to default state (used on respawn).
    /// </summary>
    public void ResetPlayerToDefault()
    {
        // Reset all orb counters
        remainingAirJumps = 0;
        
        // Reset orb stats and ability
        currentOrbStats = null;
        currentAbility = OrbAbility.Jump;
        currentStackCount = 0;
        
        // Reset the flag so player goes back to white sprite
        hasEverCollectedOrb = false;
        
        if (debugMode)
        {
            Debug.Log("OrbCollector: Player reset to default state (respawned)");
        }
        
        // Trigger events
        OnPlayerReset?.Invoke();
        TriggerUIUpdate();
    }
    
    /// <summary>
    /// Checks if the player can perform an air jump.
    /// </summary>
    public bool CanAirJump()
    {
        return remainingAirJumps > 0;
    }
    
    /// <summary>
    /// Gets the jump power multiplier based on current orb stats.
    /// </summary>
    public float GetJumpPowerMultiplier()
    {
        if (currentAbility != OrbAbility.Jump || currentOrbStats == null) return 1f;
        return Mathf.Pow(currentOrbStats.jumpPowerMultiplier, currentStackCount);
    }
    
    /// <summary>
    /// Triggers UI update with current stack counts.
    /// </summary>
    private void TriggerUIUpdate()
    {
        // Calculate current stacks based on remaining abilities
        int redStacks = (currentAbility == OrbAbility.Jump) ? remainingAirJumps : 0;
        int greenStacks = 0; // Will be updated by DashSystem
        
        // Trigger UI update event
        OnOrbStacksChanged?.Invoke(redStacks, greenStacks);
        
        if (debugMode)
        {
            Debug.Log($"OrbCollector: UI Update - Red stacks: {redStacks}, Green stacks: {greenStacks}");
        }
    }
}

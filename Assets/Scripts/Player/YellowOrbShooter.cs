using UnityEngine;

/// <summary>
/// Handles yellow orb shooting functionality for the player.
/// Extracted from PlayerOrbManager for better modularity.
/// </summary>
public class YellowOrbShooter : MonoBehaviour
{
    [Header("Yellow Orb Settings")]
    [SerializeField] private bool canShootYellowOrbs = false;
    [SerializeField] private GameObject yellowOrbPrefab;
    [SerializeField] private float yellowOrbFireRate = 0.5f; // Time between shots
    [SerializeField] private float yellowOrbSpeed = 15f;
    [SerializeField] private float yellowOrbLifetime = 5f; // How long the orb exists before destroying
    [SerializeField] private float lastShotTime = 0f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip yellowOrbShootSound;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Events
    public System.Action OnYellowOrbShot;
    
    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    /// <summary>
    /// Enables the yellow orb shooting ability.
    /// </summary>
    public void EnableYellowOrbShooting()
    {
        canShootYellowOrbs = true;
        
        if (debugMode)
        {
            Debug.Log("YellowOrbShooter: Yellow orb shooting ability enabled!");
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
            Debug.Log("YellowOrbShooter: Yellow orb shooting ability disabled!");
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
        
        // Play shooting sound effect
        PlayShootSound();
        
        // Trigger event
        OnYellowOrbShot?.Invoke();
        
        if (debugMode)
        {
            Debug.Log($"YellowOrbShooter: Shot yellow orb towards {targetPosition}");
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
            Debug.Log($"YellowOrbShooter: Yellow orb fire rate set to: {newFireRate}");
        }
    }
    
    /// <summary>
    /// Gets the current yellow orb fire rate.
    /// </summary>
    public float GetYellowOrbFireRate()
    {
        return yellowOrbFireRate;
    }
    
    /// <summary>
    /// Plays the yellow orb shooting sound effect.
    /// </summary>
    private void PlayShootSound()
    {
        if (audioSource != null && yellowOrbShootSound != null)
        {
            audioSource.PlayOneShot(yellowOrbShootSound);
        }
        else if (debugMode)
        {
            Debug.LogWarning("YellowOrbShooter: AudioSource or yellowOrbShootSound not assigned!");
        }
    }
}

using UnityEngine;
using System.Collections;
using TarodevController;

public class FinalSectionManager : MonoBehaviour
{
    [Header("Final Section Settings")]
    [SerializeField] private float sectionDuration = 30f;
    
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private PlayerController player;
    [SerializeField] private PlayerOrbManager playerOrbManager;
    [SerializeField] private RespawnManager respawnManager;
    [SerializeField] private FinalSectionTrigger finalSectionTrigger;
    
    [Header("Purple Orb Shooters")]
    [SerializeField] private PlayerTargetingOrbShooter[] shooterComponents;
    [SerializeField] private GameObject purpleOrbPrefab;
    
    [Header("Effects")]
    [SerializeField] private AudioClip sectionStartSound;
    [SerializeField] private AudioClip sectionEndSound;
    [SerializeField] private ParticleSystem sectionStartEffect;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool isFinalSectionActive = false;
    private float sectionTimer = 0f;
    private Vector3 frozenCameraPosition;
    private Quaternion frozenCameraRotation;
    private float originalFireRate;
    
    private void Awake()
    {
        // Find components if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
        if (playerOrbManager == null)
            playerOrbManager = FindFirstObjectByType<PlayerOrbManager>();
        if (respawnManager == null)
            respawnManager = FindFirstObjectByType<RespawnManager>();
        if (finalSectionTrigger == null)
            finalSectionTrigger = FindFirstObjectByType<FinalSectionTrigger>();
    }
    
    private void Start()
    {
        // Store original fire rate
        if (playerOrbManager != null)
        {
            originalFireRate = playerOrbManager.GetYellowOrbFireRate();
        }
    }
    
    private void Update()
    {
        if (isFinalSectionActive)
        {
            UpdateFinalSection();
        }
    }
    
    private void LateUpdate()
    {
        // Keep camera frozen during final section
        if (isFinalSectionActive && mainCamera != null)
        {
            mainCamera.transform.position = frozenCameraPosition;
            mainCamera.transform.rotation = frozenCameraRotation;
        }
    }
    
    /// <summary>
    /// Starts the final section when triggered.
    /// </summary>
    public void StartFinalSection()
    {
        if (isFinalSectionActive) return;
        
        isFinalSectionActive = true;
        sectionTimer = sectionDuration;
        
        // Freeze camera at current position
        FreezeCamera();
        
        // Double player shooting speed
        if (playerOrbManager != null)
        {
            playerOrbManager.SetYellowOrbFireRate(originalFireRate * 0.5f);
        }
        
        // Start purple orb shooting
        StartPurpleOrbShooting();
        
        // Play effects
        PlaySectionStartEffects();
        
        if (debugMode)
        {
            Debug.Log($"Final section started! Duration: {sectionDuration} seconds");
        }
    }
    
    /// <summary>
    /// Updates the final section logic.
    /// </summary>
    private void UpdateFinalSection()
    {
        // Update timer
        sectionTimer -= Time.deltaTime;
        
        // Check if section is complete
        if (sectionTimer <= 0f)
        {
            EndFinalSection();
        }
        
        // Constrain player to camera boundaries
        ConstrainPlayerToCamera();
    }
    
    /// <summary>
    /// Freezes the camera at its current position.
    /// </summary>
    private void FreezeCamera()
    {
        if (mainCamera != null)
        {
            frozenCameraPosition = mainCamera.transform.position;
            frozenCameraRotation = mainCamera.transform.rotation;
            
            if (debugMode)
            {
                Debug.Log($"Camera frozen at position: {frozenCameraPosition}");
            }
        }
    }
    
    /// <summary>
    /// Constrains the player to stay within camera boundaries.
    /// </summary>
    private void ConstrainPlayerToCamera()
    {
        if (player == null) return;
        
        // Calculate camera boundaries
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        float leftBound = frozenCameraPosition.x - cameraWidth / 2f;
        float rightBound = frozenCameraPosition.x + cameraWidth / 2f;
        float bottomBound = frozenCameraPosition.y - cameraHeight / 2f;
        float topBound = frozenCameraPosition.y + cameraHeight / 2f;
        
        // Get player position
        Vector3 playerPos = player.transform.position;
        
        // Constrain player position
        playerPos.x = Mathf.Clamp(playerPos.x, leftBound, rightBound);
        playerPos.y = Mathf.Clamp(playerPos.y, bottomBound, topBound);
        
        // Apply constrained position
        player.transform.position = playerPos;
    }
    
    /// <summary>
    /// Starts purple orb shooting from all shooters.
    /// </summary>
    private void StartPurpleOrbShooting()
    {
        if (shooterComponents == null || shooterComponents.Length == 0)
        {
            Debug.LogError("FinalSectionManager: No shooter components assigned!");
            return;
        }
        
        // Position shooters on camera boundaries
        PositionShootersOnCameraBoundaries();
        
        // Configure and start each shooter
        foreach (PlayerTargetingOrbShooter shooter in shooterComponents)
        {
            if (shooter != null)
            {
                ConfigureShooter(shooter);
                shooter.StartShooting();
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Purple orb shooting started with {shooterComponents.Length} shooters");
        }
    }
    
    /// <summary>
    /// Positions shooters on camera boundaries.
    /// </summary>
    private void PositionShootersOnCameraBoundaries()
    {
        if (mainCamera == null) return;
        
        // Calculate camera boundaries
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        float offset = 0.5f;
        
        // Position shooters: 2 North, 2 West
        Vector3 northLeftPos = new Vector3(frozenCameraPosition.x - cameraWidth/4, frozenCameraPosition.y + cameraHeight/2 + offset, frozenCameraPosition.z);
        Vector3 northRightPos = new Vector3(frozenCameraPosition.x + cameraWidth/4, frozenCameraPosition.y + cameraHeight/2 + offset, frozenCameraPosition.z);
        Vector3 westTopPos = new Vector3(frozenCameraPosition.x - cameraWidth/2 - offset, frozenCameraPosition.y + cameraHeight/4, frozenCameraPosition.z);
        Vector3 westBottomPos = new Vector3(frozenCameraPosition.x - cameraWidth/2 - offset, frozenCameraPosition.y - cameraHeight/4, frozenCameraPosition.z);
        
        // Set shooter positions and rotations
        if (shooterComponents.Length >= 1) SetShooterPosition(shooterComponents[0], northLeftPos, 180f);
        if (shooterComponents.Length >= 2) SetShooterPosition(shooterComponents[1], northRightPos, 180f);
        if (shooterComponents.Length >= 3) SetShooterPosition(shooterComponents[2], westTopPos, 90f);
        if (shooterComponents.Length >= 4) SetShooterPosition(shooterComponents[3], westBottomPos, 90f);
        
        if (debugMode)
        {
            Debug.Log($"Positioned {shooterComponents.Length} shooters on camera boundaries");
        }
    }
    
    /// <summary>
    /// Sets a shooter's position and rotation.
    /// </summary>
    private void SetShooterPosition(PlayerTargetingOrbShooter shooter, Vector3 position, float rotationZ)
    {
        if (shooter == null) return;
        
        shooter.transform.position = position;
        shooter.transform.rotation = Quaternion.Euler(0, 0, rotationZ);
    }
    
    /// <summary>
    /// Configures a shooter with purple orb settings.
    /// </summary>
    private void ConfigureShooter(PlayerTargetingOrbShooter shooter)
    {
        if (shooter == null) return;
        
        // Set purple orb prefab
        shooter.SetPurpleOrbPrefab(purpleOrbPrefab);
        
        // Configure shooting parameters
        shooter.SetShootInterval(1.5f); // Shoot every 1.5 seconds
        shooter.SetOrbSpeed(8f); // Moderate speed
        
        // Ensure player is always in range
        shooter.targetingRange = 50f;
        
        // Set player transform for real-time targeting
        if (player != null)
        {
            shooter.playerTransform = player.transform;
            shooter.ForceUpdatePlayerTracking();
        }
    }
    
    /// <summary>
    /// Plays effects when the final section starts.
    /// </summary>
    private void PlaySectionStartEffects()
    {
        // Play particle effect
        if (sectionStartEffect != null)
        {
            sectionStartEffect.Play();
        }
        
        // Play sound effect
        PlaySound(sectionStartSound);
        
        if (debugMode)
        {
            Debug.Log("Final section start effects played");
        }
    }
    
    /// <summary>
    /// Plays effects when the final section ends.
    /// </summary>
    private void PlaySectionEndEffects()
    {
        // Play sound effect
        PlaySound(sectionEndSound);
        
        if (debugMode)
        {
            Debug.Log("Final section end effects played");
        }
    }
    
    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    private void PlaySound(AudioClip soundClip)
    {
        if (soundClip == null) return;
        
        // Play through AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(soundClip);
        }
    }
    
    /// <summary>
    /// Ends the final section.
    /// </summary>
    private void EndFinalSection()
    {
        isFinalSectionActive = false;
        
        // Stop all shooter components
        foreach (PlayerTargetingOrbShooter shooter in shooterComponents)
        {
            if (shooter != null)
            {
                shooter.StopShooting();
            }
        }
        
        // Restore original fire rate
        if (playerOrbManager != null)
        {
            playerOrbManager.SetYellowOrbFireRate(originalFireRate);
        }
        
        // Play effects
        PlaySectionEndEffects();
        
        if (debugMode)
        {
            Debug.Log("Final section completed!");
        }
    }
    
    /// <summary>
    /// Handles player death during final section.
    /// </summary>
    public void HandlePlayerDeath()
    {
        if (!isFinalSectionActive) return;
        
        if (debugMode)
        {
            Debug.Log("Player died during final section - resetting");
        }
        
        // Reset final section
        ResetFinalSection();
        
        // Respawn player
        if (respawnManager != null)
        {
            respawnManager.ForceRespawn();
        }
    }
    
    /// <summary>
    /// Resets the final section completely.
    /// </summary>
    private void ResetFinalSection()
    {
        isFinalSectionActive = false;
        
        // Stop all shooter components
        foreach (PlayerTargetingOrbShooter shooter in shooterComponents)
        {
            if (shooter != null)
            {
                shooter.StopShooting();
            }
        }
        
        // Restore original fire rate
        if (playerOrbManager != null)
        {
            playerOrbManager.SetYellowOrbFireRate(originalFireRate);
        }
        
        // Reactivate the trigger
        if (finalSectionTrigger != null)
        {
            finalSectionTrigger.ReactivateTrigger();
        }
        
        if (debugMode)
        {
            Debug.Log("Final section completely reset - can be triggered again");
        }
    }
    
    /// <summary>
    /// Makes the player sprite blink.
    /// </summary>
    private IEnumerator BlinkPlayerSprite()
    {
        SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
        if (playerSprite == null) yield break;
        
        float blinkDuration = 1f;
        float blinkInterval = 0.1f;
        float elapsed = 0f;
        
        while (elapsed < blinkDuration)
        {
            playerSprite.enabled = !playerSprite.enabled;
            yield return new WaitForSecondsRealtime(blinkInterval);
            elapsed += blinkInterval;
        }
        
        // Ensure sprite is visible at the end
        playerSprite.enabled = true;
    }
    
    /// <summary>
    /// Checks if the final section is currently active.
    /// </summary>
    public bool IsFinalSectionActive()
    {
        return isFinalSectionActive;
    }
    
    /// <summary>
    /// Gets the remaining time in the final section.
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, sectionTimer);
    }
    
    // Draw gizmos for debugging
    private void OnDrawGizmos()
    {
        // Draw camera boundaries
        if (mainCamera != null)
        {
            float cameraHeight = 2f * mainCamera.orthographicSize;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            Vector3 cameraPos = isFinalSectionActive ? frozenCameraPosition : mainCamera.transform.position;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(cameraPos, new Vector3(cameraWidth, cameraHeight, 0));
            
            // Draw shooter positions
            if (isFinalSectionActive && shooterComponents != null)
            {
                Gizmos.color = Color.purple;
                float offset = 0.5f;
                Vector3 northLeftPos = new Vector3(cameraPos.x - cameraWidth/4, cameraPos.y + cameraHeight/2 + offset, cameraPos.z);
                Vector3 northRightPos = new Vector3(cameraPos.x + cameraWidth/4, cameraPos.y + cameraHeight/2 + offset, cameraPos.z);
                Vector3 westTopPos = new Vector3(cameraPos.x - cameraWidth/2 - offset, cameraPos.y + cameraHeight/4, cameraPos.z);
                Vector3 westBottomPos = new Vector3(cameraPos.x - cameraWidth/2 - offset, cameraPos.y - cameraHeight/4, cameraPos.z);
                
                Gizmos.DrawWireSphere(northLeftPos, 0.5f);
                Gizmos.DrawWireSphere(northRightPos, 0.5f);
                Gizmos.DrawWireSphere(westTopPos, 0.5f);
                Gizmos.DrawWireSphere(westBottomPos, 0.5f);
            }
        }
    }
}
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
        if (player == null || mainCamera == null) return;
        
        // Calculate camera boundaries
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        Vector3 cameraPos = frozenCameraPosition;
        Vector2 playerPos = player.transform.position;
        
        // Calculate camera boundary limits
        float leftBound = cameraPos.x - cameraWidth/2;
        float rightBound = cameraPos.x + cameraWidth/2;
        float bottomBound = cameraPos.y - cameraHeight/2;
        float topBound = cameraPos.y + cameraHeight/2;
        
        // Constrain player to camera boundaries
        Vector2 constrainedPosition = playerPos;
        bool wasConstrained = false;
        
        if (playerPos.x < leftBound)
        {
            constrainedPosition.x = leftBound;
            wasConstrained = true;
        }
        else if (playerPos.x > rightBound)
        {
            constrainedPosition.x = rightBound;
            wasConstrained = true;
        }
        
        if (playerPos.y < bottomBound)
        {
            constrainedPosition.y = bottomBound;
            wasConstrained = true;
        }
        else if (playerPos.y > topBound)
        {
            constrainedPosition.y = topBound;
            wasConstrained = true;
        }
        
        if (wasConstrained)
        {
            player.transform.position = constrainedPosition;
            
            if (debugMode)
            {
                Debug.Log($"Player constrained to camera boundary at {constrainedPosition}");
            }
        }
    }
    
    /// <summary>
    /// Starts shooting purple orbs from all sides.
    /// </summary>
    private void StartPurpleOrbShooting()
    {
        // Position shooters on camera boundaries
        PositionShootersOnCameraBoundaries();
        
        // Activate all shooter components
        foreach (PlayerTargetingOrbShooter shooter in shooterComponents)
        {
            if (shooter != null)
            {
                // Configure shooter for purple orbs
                shooter.SetPurpleOrbPrefab(purpleOrbPrefab);
                shooter.SetShootInterval(1f);
                shooter.SetOrbSpeed(8f);
                
                // Set large targeting range to ensure player is always in range
                shooter.targetingRange = 50f;
                
                // Ensure shooter can see the player and update targeting
                if (player != null)
                {
                    shooter.playerTransform = player.transform;
                    // Force update player tracking
                    shooter.ForceUpdatePlayerTracking();
                }
                
                shooter.StartShooting();
                
                if (debugMode)
                {
                    Debug.Log($"Activated shooter: {shooter.name} targeting player at {player.transform.position}");
                }
            }
        }
    }
    
    /// <summary>
    /// Positions shooters exactly on camera boundaries.
    /// </summary>
    private void PositionShootersOnCameraBoundaries()
    {
        if (mainCamera == null || shooterComponents == null) return;
        
        // Calculate camera boundaries
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        Vector3 cameraPos = frozenCameraPosition;
        
        // Calculate boundary positions with small offset
        float offset = 0.5f;
        Vector3 northLeftPos = new Vector3(cameraPos.x - cameraWidth/4, cameraPos.y + cameraHeight/2 + offset, cameraPos.z);
        Vector3 northRightPos = new Vector3(cameraPos.x + cameraWidth/4, cameraPos.y + cameraHeight/2 + offset, cameraPos.z);
        Vector3 westTopPos = new Vector3(cameraPos.x - cameraWidth/2 - offset, cameraPos.y + cameraHeight/4, cameraPos.z);
        Vector3 westBottomPos = new Vector3(cameraPos.x - cameraWidth/2 - offset, cameraPos.y - cameraHeight/4, cameraPos.z);
        
        // Position shooters (2 North, 2 West)
        if (shooterComponents.Length >= 4)
        {
            shooterComponents[0].transform.position = northLeftPos;   // North Left
            shooterComponents[1].transform.position = northRightPos; // North Right
            shooterComponents[2].transform.position = westTopPos;    // West Top
            shooterComponents[3].transform.position = westBottomPos; // West Bottom
            
            // Set shooter rotations to face inward
            shooterComponents[0].transform.rotation = Quaternion.Euler(0, 0, 180); // North Left faces south
            shooterComponents[1].transform.rotation = Quaternion.Euler(0, 0, 180); // North Right faces south
            shooterComponents[2].transform.rotation = Quaternion.Euler(0, 0, 90);  // West Top faces east
            shooterComponents[3].transform.rotation = Quaternion.Euler(0, 0, 90);  // West Bottom faces east
            
            if (debugMode)
            {
                Debug.Log($"Positioned shooters: 2 North, 2 West");
            }
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
        
        // Resume camera following
        ResumeCameraFollowing();
        
        // Play end effects
        PlaySectionEndEffects();
        
        if (debugMode)
        {
            Debug.Log("Final section completed!");
        }
    }
    
    /// <summary>
    /// Resumes camera following the player.
    /// </summary>
    private void ResumeCameraFollowing()
    {
        // This would integrate with your existing camera follow system
        // For now, we just stop forcing the camera position
        if (debugMode)
        {
            Debug.Log("Camera following resumed");
        }
    }
    
    /// <summary>
    /// Plays effects when section starts.
    /// </summary>
    private void PlaySectionStartEffects()
    {
        // Play particle effect
        if (sectionStartEffect != null)
        {
            sectionStartEffect.Play();
        }
        
        // Play sound
        if (AudioManager.Instance != null && sectionStartSound != null)
        {
            AudioManager.Instance.PlaySFX(sectionStartSound);
        }
    }
    
    /// <summary>
    /// Plays effects when section ends.
    /// </summary>
    private void PlaySectionEndEffects()
    {
        // Play sound
        if (AudioManager.Instance != null && sectionEndSound != null)
        {
            AudioManager.Instance.PlaySFX(sectionEndSound);
        }
    }
    
    /// <summary>
    /// Called when player gets hit by purple orb.
    /// </summary>
    public void OnPlayerHitByPurpleOrb()
    {
        if (!isFinalSectionActive) return;
        
        // Freeze game for 1 second
        StartCoroutine(HandlePlayerDeath());
    }
    
    /// <summary>
    /// Handles player death sequence.
    /// </summary>
    private IEnumerator HandlePlayerDeath()
    {
        // Freeze time
        Time.timeScale = 0f;
        
        // Make player blink
        if (player != null)
        {
            StartCoroutine(BlinkPlayerSprite());
        }
        
        // Wait 1 second (real time)
        yield return new WaitForSecondsRealtime(1f);
        
        // Restore time
        Time.timeScale = 1f;
        
        // Reset final section completely
        ResetFinalSection();
        
        // Respawn player
        if (respawnManager != null)
        {
            respawnManager.ForceRespawn();
        }
        
        if (debugMode)
        {
            Debug.Log("Player respawned and final section reset");
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
        
        // Resume camera following
        ResumeCameraFollowing();
        
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
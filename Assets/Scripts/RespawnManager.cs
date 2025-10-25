using UnityEngine;
using TarodevController;

public class RespawnManager : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 1f;
    [SerializeField] private Vector2 respawnOffset = new Vector2(0f, 1f);
    
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float cameraMargin = 5f; // Extra distance beyond camera bounds
    
    [Header("Player Settings")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Rigidbody2D playerRigidbody;
    
    [Header("Respawn Effects")]
    [SerializeField] private ParticleSystem respawnEffect;
    [SerializeField] private AudioClip respawnSound;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private RespawnPoint currentRespawnPoint;
    private Vector3 initialSpawnPosition;
    private bool isRespawning = false;
    
    private void Awake()
    {
        // Get components if not assigned
        if (mainCamera == null)
            mainCamera = Camera.main;
        if (playerTransform == null)
            playerTransform = FindFirstObjectByType<PlayerController>()?.transform;
        if (playerRigidbody == null && playerTransform != null)
            playerRigidbody = playerTransform.GetComponent<Rigidbody2D>();
            
        // Store initial spawn position
        if (playerTransform != null)
        {
            initialSpawnPosition = playerTransform.position;
        }
    }
    
    private void Update()
    {
        // Check if player is out of camera bounds
        if (!isRespawning && IsPlayerOutOfBounds())
        {
            StartRespawn();
        }
    }
    
    private bool IsPlayerOutOfBounds()
    {
        if (mainCamera == null || playerTransform == null) return false;
        
        // Get camera bounds with margin
        Vector3 cameraPos = mainCamera.transform.position;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        Vector3 playerPos = playerTransform.position;
        
        // Check if player is outside camera bounds (with margin)
        bool outOfBounds = 
            playerPos.x < cameraPos.x - cameraWidth/2 - cameraMargin ||
            playerPos.x > cameraPos.x + cameraWidth/2 + cameraMargin ||
            playerPos.y < cameraPos.y - cameraHeight/2 - cameraMargin ||
            playerPos.y > cameraPos.y + cameraHeight/2 + cameraMargin;
            
        if (debugMode && outOfBounds)
        {
            Debug.Log($"Player out of bounds! Position: {playerPos}, Camera: {cameraPos}");
        }
        
        return outOfBounds;
    }
    
    private void StartRespawn()
    {
        if (isRespawning) return;
        
        isRespawning = true;
        
        if (debugMode)
        {
            Debug.Log("Starting respawn process...");
        }
        
        // Play respawn effects
        PlayRespawnEffects();
        
        // Respawn after delay
        Invoke(nameof(ExecuteRespawn), respawnDelay);
    }
    
    private void ExecuteRespawn()
    {
        if (playerTransform == null) return;
        
        // Determine respawn position
        Vector3 respawnPosition;
        if (currentRespawnPoint != null)
        {
            respawnPosition = currentRespawnPoint.GetRespawnPosition() + (Vector3)respawnOffset;
            if (debugMode)
            {
                Debug.Log($"Respawning at checkpoint: {currentRespawnPoint.name} at position: {respawnPosition}");
            }
        }
        else
        {
            respawnPosition = initialSpawnPosition + (Vector3)respawnOffset;
            if (debugMode)
            {
                Debug.Log($"Respawning at initial position: {respawnPosition} (no checkpoint set)");
            }
        }
        
        // Move player to respawn position
        playerTransform.position = respawnPosition;
        
        // Reset player velocity
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }
        
        // Reset player state (if PlayerController has reset methods)
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.ResetPlayerState();
        }
        
        // Reset orb abilities (if PlayerOrbManager exists)
        PlayerOrbManager orbManager = playerTransform.GetComponent<PlayerOrbManager>();
        if (orbManager != null)
        {
            orbManager.ResetPlayerToDefault();
        }
        
        isRespawning = false;
        
        if (debugMode)
        {
            Debug.Log($"Respawn completed! Player now at: {playerTransform.position}");
        }
    }
    
    private void PlayRespawnEffects()
    {
        // Play particle effect
        if (respawnEffect != null)
        {
            respawnEffect.Play();
        }
        
        // Play sound
        if (respawnSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(respawnSound);
            }
        }
    }
    
    public void SetCurrentRespawnPoint(RespawnPoint respawnPoint)
    {
        // Deactivate previous respawn point
        if (currentRespawnPoint != null)
        {
            currentRespawnPoint.SetActive(false);
        }
        
        // Set new respawn point
        currentRespawnPoint = respawnPoint;
        
        if (debugMode)
        {
            Debug.Log($"Current respawn point set to: {respawnPoint.name}");
        }
    }
    
    public RespawnPoint GetCurrentRespawnPoint()
    {
        return currentRespawnPoint;
    }
    
    public void ForceRespawn()
    {
        if (!isRespawning)
        {
            if (debugMode)
            {
                Debug.Log($"ForceRespawn called. Current respawn point: {(currentRespawnPoint != null ? currentRespawnPoint.name : "None")}");
            }
            StartRespawn();
        }
        else if (debugMode)
        {
            Debug.Log("ForceRespawn called but already respawning");
        }
    }
    
    // Draw camera bounds in scene view
    private void OnDrawGizmos()
    {
        if (mainCamera == null) return;
        
        Vector3 cameraPos = mainCamera.transform.position;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;
        
        // Draw camera bounds
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(cameraPos, new Vector3(cameraWidth, cameraHeight, 0));
        
        // Draw respawn bounds (with margin)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(cameraPos, new Vector3(cameraWidth + cameraMargin * 2, cameraHeight + cameraMargin * 2, 0));
    }
}

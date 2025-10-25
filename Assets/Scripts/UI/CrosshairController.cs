using UnityEngine;
using UnityEngine.InputSystem;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float crosshairDistance = 2f; // Distance from player
    [SerializeField] private bool followMouse = true;
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer crosshairRenderer;
    
    [Header("Shooting Settings")]
    [SerializeField] private AudioClip shootSound;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private PlayerOrbManager playerOrbManager;
    private AudioSource audioSource;
    private bool isActive = false;
    private Vector2 mouseWorldPosition;
    private Mouse mouse;
    
    private void Awake()
    {
        // Get components
        if (crosshairRenderer == null)
            crosshairRenderer = GetComponent<SpriteRenderer>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Find camera if not assigned
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        // Find player orb manager
        playerOrbManager = FindFirstObjectByType<PlayerOrbManager>();
        if (playerOrbManager == null)
        {
            Debug.LogError("CrosshairController: PlayerOrbManager not found in scene!");
        }
        
        // Initially hide crosshair
        SetActive(false);
        
        // Get mouse input device
        mouse = Mouse.current;
    }
    
    private void Update()
    {
        if (!isActive) return;
        
        // Update crosshair position
        UpdateCrosshairPosition();
        
        // Handle shooting input
        HandleShootingInput();
    }
    
    /// <summary>
    /// Updates the crosshair position based on mouse or fixed position.
    /// </summary>
    private void UpdateCrosshairPosition()
    {
        if (followMouse && targetCamera != null)
        {
            Vector2 mouseScreenPosition;
            
            // Try new Input System first
            if (mouse != null)
            {
                mouseScreenPosition = mouse.position.ReadValue();
            }
            else
            {
                // Fallback to old Input System if new one fails
                mouseScreenPosition = UnityEngine.Input.mousePosition;
            }
            
            Vector3 mouseScreenPosition3D = new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, targetCamera.transform.position.z);
            mouseWorldPosition = targetCamera.ScreenToWorldPoint(mouseScreenPosition3D);
            
            // Position crosshair at mouse world position
            transform.position = mouseWorldPosition;
        }
        else
        {
            // Fixed position relative to player
            if (playerOrbManager != null)
            {
                Vector2 playerPosition = playerOrbManager.transform.position;
                Vector2 direction = (mouseWorldPosition - playerPosition).normalized;
                transform.position = playerPosition + (direction * crosshairDistance);
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Crosshair position: {transform.position}, Mouse world: {mouseWorldPosition}");
        }
    }
    
    /// <summary>
    /// Handles shooting input.
    /// </summary>
    private void HandleShootingInput()
    {
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            TryShootYellowOrb();
        }
    }
    
    /// <summary>
    /// Attempts to shoot a yellow orb towards the crosshair position.
    /// </summary>
    private void TryShootYellowOrb()
    {
        if (playerOrbManager != null && playerOrbManager.CanShootYellowOrbs())
        {
            bool shot = playerOrbManager.TryShootYellowOrb(transform.position);
            
            if (shot)
            {
                // Play shoot sound
                PlayShootSound();
                
                if (debugMode)
                {
                    Debug.Log($"Shot yellow orb towards crosshair at {transform.position}");
                }
            }
            else if (debugMode)
            {
                Debug.Log("Failed to shoot yellow orb (fire rate limit or no ability)");
            }
        }
    }
    
    /// <summary>
    /// Plays the shoot sound effect.
    /// </summary>
    private void PlayShootSound()
    {
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
        
        // Play SFX through AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(shootSound);
        }
    }
    
    /// <summary>
    /// Activates the crosshair.
    /// </summary>
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (crosshairRenderer != null)
        {
            crosshairRenderer.enabled = active;
        }
        
        if (debugMode)
        {
            Debug.Log($"Crosshair {(active ? "activated" : "deactivated")}");
        }
    }
    
    /// <summary>
    /// Checks if the crosshair is active.
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }
    
    /// <summary>
    /// Gets the current target position (where the crosshair is pointing).
    /// </summary>
    public Vector2 GetTargetPosition()
    {
        return transform.position;
    }
    
    /// <summary>
    /// Sets whether the crosshair follows the mouse.
    /// </summary>
    public void SetFollowMouse(bool follow)
    {
        followMouse = follow;
    }
    
    /// <summary>
    /// Sets the crosshair distance from player (when not following mouse).
    /// </summary>
    public void SetCrosshairDistance(float distance)
    {
        crosshairDistance = distance;
    }
    
    // Draw gizmos for debugging
    private void OnDrawGizmos()
    {
        if (!isActive) return;
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        // Draw line to player if available
        if (playerOrbManager != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, playerOrbManager.transform.position);
        }
    }
}
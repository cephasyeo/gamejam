using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color inactiveColor = Color.gray;
    
    [Header("Visual Settings")]
    [SerializeField] private SpriteRenderer visualIndicator;
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private AudioClip activationSound;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private AudioSource audioSource;
    private RespawnManager respawnManager;
    
    private void Awake()
    {
        // Get components
        if (visualIndicator == null)
            visualIndicator = GetComponent<SpriteRenderer>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Find respawn manager
        respawnManager = FindFirstObjectByType<RespawnManager>();
        if (respawnManager == null)
        {
            Debug.LogError("RespawnPoint: RespawnManager not found in scene!");
        }
        
        // Set up visual appearance
        UpdateVisualAppearance();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered the respawn point
        if (other.CompareTag("Player") && isActive)
        {
            ActivateRespawnPoint();
        }
    }
    
    private void ActivateRespawnPoint()
    {
        if (respawnManager != null)
        {
            // Set this as the current respawn point
            respawnManager.SetCurrentRespawnPoint(this);
            
            if (debugMode)
            {
                Debug.Log($"Respawn point activated at: {transform.position}");
            }
            
            // Play activation effects
            PlayActivationEffects();
        }
    }
    
    private void PlayActivationEffects()
    {
        // Play particle effect
        if (activationEffect != null)
        {
            activationEffect.Play();
        }
        
        // Play sound
        if (audioSource != null && activationSound != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
        
        // Play SFX through AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(activationSound);
        }
    }
    
    private void UpdateVisualAppearance()
    {
        if (visualIndicator != null)
        {
            visualIndicator.color = isActive ? activeColor : inactiveColor;
        }
    }
    
    public Vector3 GetRespawnPosition()
    {
        return transform.position;
    }
    
    public void SetActive(bool active)
    {
        isActive = active;
        UpdateVisualAppearance();
    }
    
    public bool IsActive()
    {
        return isActive;
    }
    
    private void OnValidate()
    {
        UpdateVisualAppearance();
    }
    
    // Draw gizmos in scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? activeColor : inactiveColor;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // Draw respawn direction indicator
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Vector2.up * 2f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}




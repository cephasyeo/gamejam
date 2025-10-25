using UnityEngine;

public class YellowOrbAbilityTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private bool isActive = true;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private AudioClip activationSound;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private AudioSource audioSource;
    private PlayerOrbManager playerOrbManager;
    private CrosshairController crosshairController;
    
    private void Awake()
    {
        // Get components if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Find player orb manager
        playerOrbManager = FindFirstObjectByType<PlayerOrbManager>();
        if (playerOrbManager == null)
        {
            Debug.LogError("YellowOrbAbilityTrigger: PlayerOrbManager not found in scene!");
        }
        
        // Find crosshair controller
        crosshairController = FindFirstObjectByType<CrosshairController>();
        if (crosshairController == null)
        {
            Debug.LogError("YellowOrbAbilityTrigger: CrosshairController not found in scene!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player entered the trigger
        if (other.CompareTag("Player") && isActive)
        {
            ActivateYellowOrbAbility();
        }
    }
    
    private void ActivateYellowOrbAbility()
    {
        if (playerOrbManager != null)
        {
            // Enable yellow orb shooting ability
            playerOrbManager.EnableYellowOrbShooting();
            
            if (debugMode)
            {
                Debug.Log($"Yellow orb shooting ability activated at: {transform.position}");
            }
            
            // Play activation effects
            PlayActivationEffects();
            
            // Show crosshair
            if (crosshairController != null)
            {
                crosshairController.SetActive(true);
            }
            
            // Hide this trigger permanently
            gameObject.SetActive(false);
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
    
    
    // Draw gizmos in scene view
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // Draw activation indicator
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Vector2.up * 2f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
    }
}

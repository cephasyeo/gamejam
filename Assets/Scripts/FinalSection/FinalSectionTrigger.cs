using UnityEngine;

public class FinalSectionTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private bool oneTimeUse = true;
    [SerializeField] private bool debugMode = true;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem activationEffect;
    [SerializeField] private AudioClip activationSound;
    
    private AudioSource audioSource;
    private FinalSectionManager finalSectionManager;
    private bool hasBeenTriggered = false;
    
    private void Awake()
    {
        // Get components if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Find final section manager
        finalSectionManager = FindFirstObjectByType<FinalSectionManager>();
        if (finalSectionManager == null)
        {
            Debug.LogError("FinalSectionTrigger: FinalSectionManager not found in scene!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateFinalSection();
        }
    }
    
    private void ActivateFinalSection()
    {
        if (finalSectionManager != null)
        {
            // Start the final section
            finalSectionManager.StartFinalSection();
            
            if (debugMode)
            {
                Debug.Log($"Final section activated at: {transform.position}");
            }
            
            // Play activation effects
            PlayActivationEffects();
            
            // Mark as triggered
            hasBeenTriggered = true;
            
            // Hide trigger temporarily (will be reactivated when final section resets)
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1f);
        
        // Draw activation indicator
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, Vector2.up * 2f);
    }
    
    /// <summary>
    /// Reactivates the trigger for reuse.
    /// </summary>
    public void ReactivateTrigger()
    {
        hasBeenTriggered = false;
        gameObject.SetActive(true);
        
        if (debugMode)
        {
            Debug.Log("Final section trigger reactivated");
        }
    }
}

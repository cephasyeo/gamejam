using UnityEngine;

public class PurpleOrb : MonoBehaviour
{
    [Header("Purple Orb Settings")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private Vector2 moveDirection = Vector2.right;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem hitEffect;
    [SerializeField] private AudioClip hitSound;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private FinalSectionManager finalSectionManager;
    
    private void Awake()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Find final section manager
        finalSectionManager = FindFirstObjectByType<FinalSectionManager>();
    }
    
    private void Start()
    {
        // Set up visual appearance
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.magenta; // Purple color
        }
        
        // Destroy after 10 seconds (safety measure)
        Destroy(gameObject, 10f);
    }
    
    private void Update()
    {
        // Move in the set direction
        transform.position += (Vector3)(moveDirection.normalized * moveSpeed * Time.deltaTime);
        
        // Rotate while moving
        transform.Rotate(0, 0, 90f * Time.deltaTime);
        
        // Destroy if it goes below Y = -10f
        if (transform.position.y < -10f)
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Initializes the purple orb with movement parameters.
    /// </summary>
    public void Initialize(Vector2 direction, float speed)
    {
        moveDirection = direction;
        moveSpeed = speed;
        
        // Rotate to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        if (debugMode)
        {
            Debug.Log($"Purple orb initialized: direction={direction}, speed={speed}");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it hit the player
        if (other.CompareTag("Player"))
        {
            HitPlayer(other);
            return;
        }
        
        // Check if it hit a yellow orb (nullified)
        YellowOrb yellowOrb = other.GetComponent<YellowOrb>();
        if (yellowOrb != null)
        {
            NullifyPurpleOrb(yellowOrb);
            return;
        }
        
        // Check if it hit another orb (red, green, white)
        Orb otherOrb = other.GetComponent<Orb>();
        if (otherOrb != null)
        {
            // Purple orb destroys other orbs
            DestroyOrb(otherOrb);
            return;
        }
    }
    
    /// <summary>
    /// Handles hitting the player.
    /// </summary>
    private void HitPlayer(Collider2D playerCollider)
    {
        if (debugMode)
        {
            Debug.Log($"Purple orb hit player: {playerCollider.name}");
        }
        
        // Play hit effects
        PlayHitEffects();
        
        // Notify final section manager
        if (finalSectionManager != null)
        {
            finalSectionManager.OnPlayerHitByPurpleOrb();
        }
        
        // Destroy this purple orb
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Handles being nullified by yellow orb.
    /// </summary>
    private void NullifyPurpleOrb(YellowOrb yellowOrb)
    {
        if (debugMode)
        {
            Debug.Log("Purple orb nullified by yellow orb");
        }
        
        // Play hit effects
        PlayHitEffects();
        
        // Destroy the yellow orb
        Destroy(yellowOrb.gameObject);
        
        // Destroy this purple orb
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Destroys another orb.
    /// </summary>
    private void DestroyOrb(Orb orb)
    {
        if (debugMode)
        {
            Debug.Log($"Purple orb destroyed another orb: {orb.name}");
        }
        
        // Play hit effects
        PlayHitEffects();
        
        // Destroy the other orb
        Destroy(orb.gameObject);
        
        // Destroy this purple orb
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Plays hit effects (particles and sound).
    /// </summary>
    private void PlayHitEffects()
    {
        // Play particle effect
        if (hitEffect != null)
        {
            hitEffect.Play();
        }
        
        // Play sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // Play SFX through AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(hitSound);
        }
    }
    
    // Draw gizmos for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw movement direction
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, moveDirection.normalized * 2f);
    }
}

using UnityEngine;
using TarodevController;

public class Orb : MonoBehaviour
{
    [Header("Orb Settings")]
    [SerializeField] private OrbStats orbStats;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D orbCollider;
    [SerializeField] private ParticleSystem collectEffect;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip collectSound;
    
    [Header("Visual Effects")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float rotationSpeed = 90f;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    
    private void Awake()
    {
        // Get components if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (orbCollider == null)
            orbCollider = GetComponent<Collider2D>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        startPosition = transform.position;
        
        // Set up orb appearance
        SetupOrbAppearance();
    }
    
    private void Update()
    {
        if (!isCollected)
        {
            // Bobbing animation
            float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            
            // Rotation animation
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        }
    }
    
    private void SetupOrbAppearance()
    {
        if (orbStats != null && spriteRenderer != null)
        {
            spriteRenderer.color = orbStats.orbColor;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;
        
        // Check if player collected the orb
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            CollectOrb(player);
        }
    }
    
    private void CollectOrb(PlayerController player)
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Get player's orb manager
        PlayerOrbManager orbManager = player.GetComponent<PlayerOrbManager>();
        if (orbManager != null)
        {
            orbManager.CollectOrb(orbStats);
        }
        
        // Play collection effects
        PlayCollectionEffects();
        
        // Destroy the orb
        Destroy(gameObject, 0.5f); // Small delay to let effects play
    }
    
    private void PlayCollectionEffects()
    {
        // Play particle effect
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
        
        // Play sound
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // Hide the orb visually
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }
        
        // Disable collider
        if (orbCollider != null)
        {
            orbCollider.enabled = false;
        }
    }
    
    public void SetOrbStats(OrbStats stats)
    {
        orbStats = stats;
        SetupOrbAppearance();
    }
    
    public OrbStats GetOrbStats()
    {
        return orbStats;
    }
    
    private void OnValidate()
    {
        SetupOrbAppearance();
    }
}

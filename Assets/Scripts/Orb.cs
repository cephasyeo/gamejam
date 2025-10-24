using UnityEngine;
using System.Collections;
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
    
    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 3f;
    [SerializeField] private bool respawnEnabled = true;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    private bool isRespawning = false;
    
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
        if (!isCollected && !isRespawning)
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
        if (isCollected || isRespawning) return;
        
        isCollected = true;
        
        // Get player's orb manager
        PlayerOrbManager orbManager = player.GetComponent<PlayerOrbManager>();
        if (orbManager != null)
        {
            orbManager.CollectOrb(orbStats);
        }
        
        // Play collection effects
        PlayCollectionEffects();
        
        // Start respawn process if enabled
        if (respawnEnabled)
        {
            StartCoroutine(RespawnOrb());
        }
        else
        {
            // Destroy the orb if respawn is disabled
            Destroy(gameObject, 0.5f);
        }
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
    
    private System.Collections.IEnumerator RespawnOrb()
    {
        // Wait for respawn delay
        yield return new WaitForSeconds(respawnDelay);
        
        // Reset orb state
        isCollected = false;
        isRespawning = false;
        
        // Reset position to original spawn position
        transform.position = startPosition;
        
        // Re-enable visual components
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        
        if (orbCollider != null)
        {
            orbCollider.enabled = true;
        }
        
        Debug.Log($"Orb respawned at position: {startPosition}");
    }
    
    private void OnValidate()
    {
        SetupOrbAppearance();
    }
}

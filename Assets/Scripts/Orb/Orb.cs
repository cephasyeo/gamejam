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
    
    [Header("Shooter Settings")]
    [SerializeField] public bool isShooterOrb = false; // Set true for moving orbs
    [SerializeField] public Vector2 moveDirection = Vector2.down;
    [SerializeField] public float moveSpeed = 10f;
    private Vector3 startPosition;
    private bool isCollected = false;
    private bool isRespawning = false;
    
    /// <summary>
    /// Public property to check if orb is collected
    /// </summary>
    public bool IsCollected => isCollected;
    
    /// <summary>
    /// Resets the orb to its initial state
    /// </summary>
    public void ResetOrb()
    {
        isCollected = false;
        isRespawning = false;
        gameObject.SetActive(true);
        
        if (debugMode)
        {
            Debug.Log($"Orb {gameObject.name} reset to initial state");
        }
    }
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
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
        
        // Debug collider info
        if (debugMode)
        {
            if (orbCollider != null)
            {
                Debug.Log($"Orb collider: IsTrigger={orbCollider.isTrigger}, Layer={gameObject.layer}, Tag={gameObject.tag}, Radius={(orbCollider as CircleCollider2D)?.radius}");
            }
            else
            {
                Debug.LogError($"Orb {gameObject.name} has no Collider2D component!");
            }
        }
        
        // Set up orb appearance
        SetupOrbAppearance();
    }
    
    private void Update()
    {
        if (isCollected || isRespawning) return;

        if (isShooterOrb)
        {
            // Move in the set direction
            transform.position += (Vector3)(moveDirection.normalized * moveSpeed * Time.deltaTime);

            // Optional: rotate while moving
            transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
            
            // Destroy orb if it goes below Y = -10f
            if (transform.position.y < -10f)
            {
                Destroy(gameObject);
            }
        }
        else
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

        if (debugMode)
        {
            Debug.Log($"Orb OnTriggerEnter2D called with: {other.name}");
        }

        // Check if it's a yellow orb (nullify orb)
        YellowOrb yellowOrb = other.GetComponent<YellowOrb>();
        if (yellowOrb != null)
        {
            if (debugMode)
            {
                Debug.Log($"Orb detected YellowOrb component on: {other.name}");
            }
            
            // Check if this orb should be nullified by yellow orb
            // Yellow orbs only nullify white orbs (Reset ability), not red or green
            if (orbStats != null && orbStats.ability == OrbAbility.Reset)
            {
                // Yellow orb nullifies this white orb
                NullifyOrb();
            }
            else if (debugMode)
            {
                Debug.Log($"Orb ignoring yellow orb (only white orbs are nullified by yellow orbs)");
            }
            return;
        }

        // Check if player collected the orb
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            if (debugMode)
            {
                Debug.Log($"Orb detected Player component on: {other.name}");
            }
            CollectOrb(player);
        }
        
        if (debugMode)
        {
            Debug.Log($"Orb collision with {other.name} - no matching components found");
        }
    }
    
    private void OnBecameInvisible()
    {
        if (isShooterOrb)
        {
            Destroy(gameObject);
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
    
    /// <summary>
    /// Called when this orb is nullified by a yellow orb.
    /// </summary>
    private void NullifyOrb()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Play nullify effects
        PlayCollectionEffects();
        
        // Destroy this orb
        Destroy(gameObject);
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

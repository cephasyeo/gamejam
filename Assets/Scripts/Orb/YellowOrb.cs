using UnityEngine;

public class YellowOrb : MonoBehaviour
{
    [Header("Yellow Orb Settings")]
    [SerializeField] private float moveSpeed = 15f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private Vector2 moveDirection = Vector2.right;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem nullifyEffect;
    [SerializeField] private AudioClip nullifySound;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private float spawnTime;
    
    private void Awake()
    {
        // Get components
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        
        // Set spawn time
        spawnTime = Time.time;
    }
    
    private void Start()
    {
        // Set up visual appearance
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }
        
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
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
    /// Initializes the yellow orb with movement parameters.
    /// </summary>
    public void Initialize(Vector2 direction, float speed, float lifeTime)
    {
        moveDirection = direction;
        moveSpeed = speed;
        lifetime = lifeTime;
        
        // Rotate to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        if (debugMode)
        {
            Debug.Log($"Yellow orb initialized: direction={direction}, speed={speed}, lifetime={lifeTime}");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if it hit another orb
        Orb otherOrb = other.GetComponent<Orb>();
        if (otherOrb != null)
        {
            NullifyOrb(otherOrb);
            return;
        }
        
        // Check if it hit a yellow orb shooter
        PlayerTargetingOrbShooter shooter = other.GetComponent<PlayerTargetingOrbShooter>();
        if (shooter != null)
        {
            NullifyShooter(shooter);
            return;
        }
        
        // Check if it hit a regular orb shooter
        OrbShooter regularShooter = other.GetComponent<OrbShooter>();
        if (regularShooter != null)
        {
            NullifyRegularShooter(regularShooter);
            return;
        }
    }
    
    /// <summary>
    /// Nullifies (destroys) another orb.
    /// </summary>
    private void NullifyOrb(Orb orb)
    {
        if (debugMode)
        {
            Debug.Log($"Yellow orb nullified another orb: {orb.name}");
        }
        
        // Play nullify effects
        PlayNullifyEffects();
        
        // Destroy the other orb
        Destroy(orb.gameObject);
        
        // Destroy this yellow orb
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Temporarily disables a targeting orb shooter.
    /// </summary>
    private void NullifyShooter(PlayerTargetingOrbShooter shooter)
    {
        if (debugMode)
        {
            Debug.Log($"Yellow orb nullified targeting shooter: {shooter.name}");
        }
        
        // Play nullify effects
        PlayNullifyEffects();
        
        // Stop the shooter temporarily
        shooter.StopShooting();
        
        // Restart shooting after a delay
        StartCoroutine(RestartShooterAfterDelay(shooter, 2f));
        
        // Destroy this yellow orb
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Temporarily disables a regular orb shooter.
    /// </summary>
    private void NullifyRegularShooter(OrbShooter shooter)
    {
        if (debugMode)
        {
            Debug.Log($"Yellow orb nullified regular shooter: {shooter.name}");
        }
        
        // Play nullify effects
        PlayNullifyEffects();
        
        // Stop the shooter temporarily
        shooter.StopShooting();
        
        // Restart shooting after a delay
        StartCoroutine(RestartRegularShooterAfterDelay(shooter, 2f));
        
        // Destroy this yellow orb
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Plays nullify effects (particles and sound).
    /// </summary>
    private void PlayNullifyEffects()
    {
        // Play particle effect
        if (nullifyEffect != null)
        {
            nullifyEffect.Play();
        }
        
        // Play sound
        if (audioSource != null && nullifySound != null)
        {
            audioSource.PlayOneShot(nullifySound);
        }
        
        // Play SFX through AudioManager if available
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(nullifySound);
        }
    }
    
    /// <summary>
    /// Restarts a targeting shooter after a delay.
    /// </summary>
    private System.Collections.IEnumerator RestartShooterAfterDelay(PlayerTargetingOrbShooter shooter, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (shooter != null)
        {
            shooter.StartShooting();
            
            if (debugMode)
            {
                Debug.Log($"Restarted targeting shooter: {shooter.name}");
            }
        }
    }
    
    /// <summary>
    /// Restarts a regular shooter after a delay.
    /// </summary>
    private System.Collections.IEnumerator RestartRegularShooterAfterDelay(OrbShooter shooter, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (shooter != null)
        {
            shooter.StartShooting();
            
            if (debugMode)
            {
                Debug.Log($"Restarted regular shooter: {shooter.name}");
            }
        }
    }
    
    // Draw gizmos for debugging
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw movement direction
        Gizmos.color = Color.white;
        Gizmos.DrawRay(transform.position, moveDirection.normalized * 2f);
    }
}

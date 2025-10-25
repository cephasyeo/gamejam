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
        
        // Debug collider info
        if (debugMode)
        {
            Collider2D col = GetComponent<Collider2D>();
            if (col != null)
            {
                Debug.Log($"YellowOrb collider: IsTrigger={col.isTrigger}, Layer={gameObject.layer}, Tag={gameObject.tag}");
            }
            else
            {
                Debug.LogError("YellowOrb has no Collider2D component!");
            }
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
        
        // Debug: Log position every second
        if (debugMode && Time.time - spawnTime > 0 && Mathf.FloorToInt(Time.time - spawnTime) != Mathf.FloorToInt((Time.time - Time.deltaTime) - spawnTime))
        {
            Debug.Log($"YellowOrb position: {transform.position}, Direction: {moveDirection}, Speed: {moveSpeed}");
        }
        
        // Destroy if it goes below Y = -10f
        if (transform.position.y < -10f)
        {
            if (debugMode)
            {
                Debug.Log("YellowOrb destroyed - went below Y = -10f");
            }
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
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (debugMode)
        {
            Debug.Log($"YellowOrb OnCollisionEnter2D with: {collision.gameObject.name}");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (debugMode)
        {
            Debug.Log($"YellowOrb OnTriggerEnter2D called with: {other.name} (Layer: {other.gameObject.layer})");
        }
        
        // Ignore player collisions
        if (other.CompareTag("Player"))
        {
            if (debugMode)
            {
                Debug.Log($"YellowOrb ignoring collision with Player");
            }
            return;
        }
        
        // Check if it hit another orb (red, green, white)
        Orb otherOrb = other.GetComponent<Orb>();
        if (otherOrb != null)
        {
            if (debugMode)
            {
                Debug.Log($"YellowOrb detected Orb component on: {other.name}");
            }
            NullifyOrb(otherOrb);
            return;
        }
        
        // Check if it hit another yellow orb
        YellowOrb otherYellowOrb = other.GetComponent<YellowOrb>();
        if (otherYellowOrb != null)
        {
            if (debugMode)
            {
                Debug.Log($"YellowOrb detected YellowOrb component on: {other.name}");
            }
            NullifyYellowOrb(otherYellowOrb);
            return;
        }
        
        if (debugMode)
        {
            Debug.Log($"YellowOrb collision with {other.name} - no matching components found");
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (debugMode)
        {
            Debug.Log($"YellowOrb OnTriggerStay2D with: {other.name}");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (debugMode)
        {
            Debug.Log($"YellowOrb OnTriggerExit2D with: {other.name}");
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
    /// Nullifies (destroys) another yellow orb.
    /// </summary>
    private void NullifyYellowOrb(YellowOrb otherYellowOrb)
    {
        if (debugMode)
        {
            Debug.Log($"Yellow orb nullified another yellow orb: {otherYellowOrb.name}");
        }
        
        // Play nullify effects
        PlayNullifyEffects();
        
        // Destroy the other yellow orb
        Destroy(otherYellowOrb.gameObject);
        
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

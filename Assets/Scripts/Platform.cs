using UnityEngine;

public enum PlatformColor
{
    White,
    Red,
    Purple,
    Yellow,
    Green
}

public class Platform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private PlatformColor platformColor = PlatformColor.White;
    [SerializeField] private SpriteRenderer[] blockRenderers; // Multiple blocks for visual
    [SerializeField] private Color[] colorValues = new Color[5];
    
    [Header("Collision")]
    [SerializeField] private Collider2D platformCollider; // Single collider for entire platform
    [SerializeField] private Collider2D topCollider; // For landing on top
    [SerializeField] private Collider2D sideCollider; // For side collision when falling
    [SerializeField] private float passThroughDuration = 0.5f; // How long collider stays disabled
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool isPassThroughActive = false;
    
    private void Awake()
    {
        // Get components if not assigned
        if (platformCollider == null)
            platformCollider = GetComponent<Collider2D>();
            
        // Initialize color values
        InitializeColors();
        
        // Set initial color
        SetPlatformColor(platformColor);
    }
    
    private void InitializeColors()
    {
        ColorUtility.TryParseHtmlString("#FFFFFF", out colorValues[0]); // White
        ColorUtility.TryParseHtmlString("#FC5C65", out colorValues[1]); // Red
        ColorUtility.TryParseHtmlString("#9179FF", out colorValues[2]); // Purple
        ColorUtility.TryParseHtmlString("#FFB600", out colorValues[3]); // Yellow
        ColorUtility.TryParseHtmlString("#37D98C", out colorValues[4]); // Green
    }
    
    public void SetPlatformColor(PlatformColor newColor)
    {
        platformColor = newColor;
        
        // Get all block renderers if not already assigned
        if (blockRenderers == null || blockRenderers.Length == 0)
        {
            blockRenderers = GetComponentsInChildren<SpriteRenderer>();
        }
        
        // Apply color to all block renderers
        if (blockRenderers != null)
        {
            foreach (SpriteRenderer renderer in blockRenderers)
            {
                if (renderer != null)
                {
                    renderer.color = colorValues[(int)newColor];
                }
            }
        }
        
        if (debugMode)
        {
            Debug.Log($"Platform color changed to: {newColor}");
        }
    }
    
    public PlatformColor GetPlatformColor()
    {
        return platformColor;
    }
    
    public bool CanPlayerPassThrough(PlatformColor playerColor)
    {
        // Player can pass through if colors match OR if platform is white
        return playerColor == platformColor || platformColor == PlatformColor.White;
    }
    
    public bool CanPlayerLandOnTop(PlatformColor playerColor)
    {
        // Player can land on top if colors don't match OR if platform is white
        return playerColor != platformColor || platformColor == PlatformColor.White;
    }
    
    public void OnPlayerJumpFromBelow(PlatformColor playerColor)
    {
        if (CanPlayerPassThrough(playerColor) && !isPassThroughActive)
        {
            // Disable top collider to allow pass-through from below
            if (topCollider != null)
            {
                topCollider.enabled = false;
                isPassThroughActive = true;
                
                // Re-enable after duration
                Invoke(nameof(ReenableTopCollider), passThroughDuration);
                
                if (debugMode)
                {
                    Debug.Log($"Player passed through platform from below (colors matched or white platform: {playerColor})");
                }
            }
        }
        else if (!CanPlayerPassThrough(playerColor))
        {
            // Colors don't match and not white - ensure colliders are enabled for normal collision
            if (topCollider != null)
                topCollider.enabled = true;
            if (sideCollider != null)
                sideCollider.enabled = true;
                
            if (debugMode)
            {
                Debug.Log($"Player collides with platform (colors don't match: {playerColor} vs {platformColor})");
            }
        }
    }
    
    public void OnPlayerLanding(PlatformColor playerColor)
    {
        // When player lands on platform, ensure top collider is enabled for landing
        if (CanPlayerLandOnTop(playerColor))
        {
            if (topCollider != null)
                topCollider.enabled = true;
                
            if (debugMode)
            {
                Debug.Log($"Player landed on platform (can land: {playerColor})");
            }
        }
    }
    
    public void OnPlayerSideCollision(PlatformColor playerColor, bool isFalling)
    {
        if (CanPlayerPassThrough(playerColor) && isFalling)
        {
            // Allow pass-through when falling and colors match
            if (sideCollider != null)
            {
                sideCollider.enabled = false;
                Invoke(nameof(ReenableSideCollider), passThroughDuration);
                
                if (debugMode)
                {
                    Debug.Log($"Player passed through platform side while falling (colors matched: {playerColor})");
                }
            }
        }
    }
    
    private void ReenableTopCollider()
    {
        if (topCollider != null)
        {
            topCollider.enabled = true;
            isPassThroughActive = false;
            
            if (debugMode)
            {
                Debug.Log("Platform top collider re-enabled");
            }
        }
    }
    
    private void ReenableSideCollider()
    {
        if (sideCollider != null)
        {
            sideCollider.enabled = true;
            
            if (debugMode)
            {
                Debug.Log("Platform side collider re-enabled");
            }
        }
    }
    
    // Method to randomly change platform color
    public void RandomizeColor()
    {
        PlatformColor randomColor = (PlatformColor)Random.Range(0, System.Enum.GetValues(typeof(PlatformColor)).Length);
        SetPlatformColor(randomColor);
    }
    
    // Method to cycle through colors
    public void CycleColor()
    {
        int currentIndex = (int)platformColor;
        int nextIndex = (currentIndex + 1) % System.Enum.GetValues(typeof(PlatformColor)).Length;
        SetPlatformColor((PlatformColor)nextIndex);
    }
    
    private void OnValidate()
    {
        // This is called when values change in the inspector
        // Initialize colors first
        InitializeColors();
        
        // Apply the color immediately
        SetPlatformColor(platformColor);
    }
    
    private void OnDrawGizmos()
    {
        if (debugMode)
        {
            // Draw color indicator
            Gizmos.color = colorValues[(int)platformColor];
            Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);
            
            // Draw top collider bounds
            if (topCollider != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(topCollider.bounds.center, topCollider.bounds.size);
            }
            
            // Draw side collider bounds
            if (sideCollider != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(sideCollider.bounds.center, sideCollider.bounds.size);
            }
        }
    }
}

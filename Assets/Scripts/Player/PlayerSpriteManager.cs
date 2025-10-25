using UnityEngine;

/// <summary>
/// Handles player sprite management and visual appearance changes.
/// Extracted from PlayerOrbManager for better modularity.
/// </summary>
public class PlayerSpriteManager : MonoBehaviour
{
    [Header("Character Sprites")]
    [SerializeField] private Sprite whiteSprite;  // Default white character
    [SerializeField] private Sprite redSprite;    // Red character for jump ability
    [SerializeField] private Sprite greenSprite; // Green character for dash ability
    
    [Header("References")]
    [SerializeField] private SpriteRenderer playerSpriteRenderer;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    
    // Events
    public System.Action<OrbAbility> OnSpriteChanged;
    
    private void Awake()
    {
        if (playerSpriteRenderer == null)
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
            
        // Store the initial red sprite if not assigned
        if (redSprite == null && playerSpriteRenderer != null)
        {
            redSprite = playerSpriteRenderer.sprite;
        }
    }
    
    /// <summary>
    /// Changes the player sprite based on the orb ability.
    /// </summary>
    public void ChangePlayerSprite(OrbAbility ability)
    {
        if (playerSpriteRenderer == null) return;
        
        switch (ability)
        {
            case OrbAbility.Jump:
                // Red sprite for jump ability
                if (redSprite != null)
                {
                    playerSpriteRenderer.sprite = redSprite;
                    if (debugMode)
                    {
                        Debug.Log("PlayerSpriteManager: Changed to red sprite (Jump ability)");
                    }
                }
                break;
                
            case OrbAbility.Dash:
                // Green sprite for dash ability
                if (greenSprite != null)
                {
                    playerSpriteRenderer.sprite = greenSprite;
                    if (debugMode)
                    {
                        Debug.Log("PlayerSpriteManager: Changed to green sprite (Dash ability)");
                    }
                }
                break;
        }
        
        OnSpriteChanged?.Invoke(ability);
    }
    
    /// <summary>
    /// Sets the player to the default white sprite.
    /// </summary>
    public void SetDefaultSprite()
    {
        if (playerSpriteRenderer == null) return;
        
        // Set to white/default sprite when no orbs are collected
        if (whiteSprite != null)
        {
            playerSpriteRenderer.sprite = whiteSprite;
            if (debugMode)
            {
                Debug.Log("PlayerSpriteManager: Changed to white sprite (default)");
            }
        }
    }
    
    /// <summary>
    /// Gets the current sprite being displayed.
    /// </summary>
    public Sprite GetCurrentSprite()
    {
        return playerSpriteRenderer != null ? playerSpriteRenderer.sprite : null;
    }
    
    /// <summary>
    /// Checks if the player is currently displaying the default white sprite.
    /// </summary>
    public bool IsDefaultSprite()
    {
        return playerSpriteRenderer != null && playerSpriteRenderer.sprite == whiteSprite;
    }
}

using UnityEngine;
using UnityEngine.UI;

public class OrbUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform orbContainer;
    [SerializeField] private GameObject redOrbPrefab;
    [SerializeField] private GameObject greenOrbPrefab;
    
    [Header("UI Settings")]
    [SerializeField] private float orbSpacing = 10f;
    [SerializeField] private int maxOrbsPerRow = 5;
    
    private PlayerOrbManager playerOrbManager;
    private int currentRedOrbs = 0;
    private int currentGreenOrbs = 0;
    
    private void Awake()
    {
        // Get PlayerOrbManager reference
        playerOrbManager = FindFirstObjectByType<PlayerOrbManager>();
        if (playerOrbManager == null)
        {
            Debug.LogError("OrbUIManager: PlayerOrbManager not found!");
            return;
        }
        
        // Subscribe to orb events
        playerOrbManager.OnOrbStacksChanged += UpdateOrbDisplay;
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (playerOrbManager != null)
        {
            playerOrbManager.OnOrbStacksChanged -= UpdateOrbDisplay;
        }
    }
    
    private void UpdateOrbDisplay(int redStacks, int greenStacks)
    {
        // Update red orbs
        UpdateOrbType(OrbAbility.Jump, redStacks);
        
        // Update green orbs
        UpdateOrbType(OrbAbility.Dash, greenStacks);
        
        // Update container layout
        UpdateContainerLayout();
    }
    
    private void UpdateOrbType(OrbAbility ability, int newCount)
    {
        int currentCount = (ability == OrbAbility.Jump) ? currentRedOrbs : currentGreenOrbs;
        GameObject orbPrefab = (ability == OrbAbility.Jump) ? redOrbPrefab : greenOrbPrefab;
        
        if (newCount > currentCount)
        {
            // Add new orbs
            for (int i = currentCount; i < newCount; i++)
            {
                CreateOrbUI(orbPrefab, ability);
            }
        }
        else if (newCount < currentCount)
        {
            // Remove excess orbs
            RemoveOrbs(ability, currentCount - newCount);
        }
        
        // Update current count
        if (ability == OrbAbility.Jump)
            currentRedOrbs = newCount;
        else
            currentGreenOrbs = newCount;
    }
    
    private void CreateOrbUI(GameObject orbPrefab, OrbAbility ability)
    {
        if (orbPrefab == null || orbContainer == null) return;
        
        GameObject orbUI = Instantiate(orbPrefab, orbContainer);
        orbUI.name = $"{ability}Orb_{currentRedOrbs + currentGreenOrbs}";
        
        // Add to appropriate position
        orbUI.transform.SetAsLastSibling();
    }
    
    private void RemoveOrbs(OrbAbility ability, int count)
    {
        if (orbContainer == null) return;
        
        string orbPrefix = (ability == OrbAbility.Jump) ? "JumpOrb_" : "DashOrb_";
        
        // Find and remove the last orbs of this type
        for (int i = 0; i < count; i++)
        {
            Transform orbToRemove = null;
            
            // Find the last orb of this type
            for (int j = orbContainer.childCount - 1; j >= 0; j--)
            {
                Transform child = orbContainer.GetChild(j);
                if (child.name.StartsWith(orbPrefix))
                {
                    orbToRemove = child;
                    break;
                }
            }
            
            if (orbToRemove != null)
            {
                DestroyImmediate(orbToRemove.gameObject);
            }
        }
    }
    
    private void UpdateContainerLayout()
    {
        if (orbContainer == null) return;
        
        // Layout orbs from left side with wrapping, inside the container
        for (int i = 0; i < orbContainer.childCount; i++)
        {
            Transform child = orbContainer.GetChild(i);
            RectTransform rectTransform = child.GetComponent<RectTransform>();
            
            if (rectTransform != null)
            {
                // Calculate position based on index - start from left
                int row = i / maxOrbsPerRow;
                int col = i % maxOrbsPerRow;
                
                // Position from left edge of container with padding
                float x = col * orbSpacing + 30f; // 10px padding from left edge
                float y = -row * orbSpacing - 30f; // 10px padding from top edge
                
                // Set anchor to top-left for consistent positioning
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.anchoredPosition = new Vector2(x, y);
            }
        }
    }
}

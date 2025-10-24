using UnityEngine;
using UnityEngine.UI;

public class OrbUI : MonoBehaviour
{
    [Header("Orb Settings")]
    [SerializeField] private OrbAbility orbAbility;
    [SerializeField] private Color orbColor = Color.white;
    
    [Header("UI Components")]
    [SerializeField] private Image orbImage;
    [SerializeField] private Image backgroundImage;
    
    private void Awake()
    {
        // Get components if not assigned
        if (orbImage == null)
            orbImage = GetComponent<Image>();
        
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        // Set up the orb appearance
        SetupOrbAppearance();
    }
    
    private void SetupOrbAppearance()
    {
        if (orbImage != null)
        {
            orbImage.color = orbColor;
        }
        
        // Set orb size
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(40, 40); // 40x40 pixel orbs
        }
    }
    
    public void SetOrbAbility(OrbAbility ability)
    {
        orbAbility = ability;
        
        // Set color based on ability
        switch (ability)
        {
            case OrbAbility.Jump:
                orbColor = new Color(0.988f, 0.361f, 0.396f, 1f); // Red color
                break;
            case OrbAbility.Dash:
                orbColor = new Color(0.216f, 0.851f, 0.549f, 1f); // Green color
                break;
        }
        
        SetupOrbAppearance();
    }
    
    public void SetOrbColor(Color color)
    {
        orbColor = color;
        SetupOrbAppearance();
    }
}

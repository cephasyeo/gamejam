using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Helper script to easily connect UI buttons to MenuManager functions
/// Attach this to buttons in your main menu for quick setup
/// </summary>
public class MainMenuButtonHelper : MonoBehaviour
{
    [Header("Button Type")]
    [SerializeField] private ButtonType buttonType;
    
    private enum ButtonType
    {
        Play,
        Options,
        Quit
    }
    
    private void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            switch (buttonType)
            {
                case ButtonType.Play:
                    button.onClick.AddListener(() => FindFirstObjectByType<MenuManager>()?.PlayGame());
                    break;
                case ButtonType.Options:
                    button.onClick.AddListener(() => FindFirstObjectByType<MenuManager>()?.ShowOptions());
                    break;
                case ButtonType.Quit:
                    button.onClick.AddListener(() => FindFirstObjectByType<MenuManager>()?.QuitGame());
                    break;
            }
        }
    }
}

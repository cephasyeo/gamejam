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
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
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
                    button.onClick.AddListener(() => {
                        if (debugMode) Debug.Log("MainMenuButtonHelper: Play button clicked!");
                        FindFirstObjectByType<MenuManager>()?.PlayGame();
                    });
                    break;
                case ButtonType.Options:
                    button.onClick.AddListener(() => {
                        if (debugMode) Debug.Log("MainMenuButtonHelper: Options button clicked!");
                        FindFirstObjectByType<MenuManager>()?.ShowOptions();
                    });
                    break;
                case ButtonType.Quit:
                    button.onClick.AddListener(() => {
                        if (debugMode) Debug.Log("MainMenuButtonHelper: Quit button clicked!");
                        FindFirstObjectByType<MenuManager>()?.QuitGame();
                    });
                    break;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitToMainMenuButton;
    
    [Header("Menu References")]
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private OptionsMenu optionsMenuScript;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool isPauseMenuActive = false;
    
    private void Awake()
    {
        // Subscribe to GameManager events as early as possible
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePause += ShowPauseMenu;
            GameManager.Instance.OnGameResume += HidePauseMenu;
        }
    }

    private void Start()
    {
        SetupEventListeners();

        // Ensure the pause menu starts hidden even if the object is initially active in the scene
        gameObject.SetActive(true); // make sure Start runs when testing in-editor
        HidePauseMenu();

        if (debugMode)
        {
            Debug.Log("PauseMenu initialized");
        }
    }
    
    private void SetupEventListeners()
    {
        // Continue Button
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
        
        // Options Button
        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OnOptionsButtonClicked);
        }
        
        // Quit to Main Menu Button
        if (quitToMainMenuButton != null)
        {
            quitToMainMenuButton.onClick.AddListener(OnQuitToMainMenuButtonClicked);
        }
        
        // GameManager subscriptions handled in Awake
    }
    
    #region Button Events
    
    private void OnContinueButtonClicked()
    {
        ResumeGame();
        
        if (debugMode)
        {
            Debug.Log("Continue button clicked");
        }
    }
    
    private void OnOptionsButtonClicked()
    {
        ShowOptionsMenu();
        
        if (debugMode)
        {
            Debug.Log("Options button clicked");
        }
    }
    
    private void OnQuitToMainMenuButtonClicked()
    {
        QuitToMainMenu();
        
        if (debugMode)
        {
            Debug.Log("Quit to main menu button clicked");
        }
    }
    
    #endregion
    
    #region Menu Navigation
    
    private void ResumeGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
        
        HidePauseMenu();
    }
    
    private void ShowOptionsMenu()
    {
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(true);
            
            // Configure options menu to know it's from pause menu
            if (optionsMenuScript != null)
            {
                optionsMenuScript.SetFromMainMenu(false);
            }
        }
        
        // Hide pause menu while options are open
        HidePauseMenu();
    }
    
    private void QuitToMainMenu()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
        
        HidePauseMenu();
    }
    
    #endregion
    
    #region Pause Menu Visibility
    
    public void ShowPauseMenu()
    {
        gameObject.SetActive(true);
        isPauseMenuActive = true;

        // Hide options menu if it's open
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(false);
        }

        if (debugMode)
        {
            Debug.Log("Pause menu shown");
        }
    }
    
    public void HidePauseMenu()
    {
        gameObject.SetActive(false);
        isPauseMenuActive = false;

        if (debugMode)
        {
            Debug.Log("Pause menu hidden");
        }
    }
    
    public void TogglePauseMenu()
    {
        if (isPauseMenuActive)
        {
            HidePauseMenu();
        }
        else
        {
            ShowPauseMenu();
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public bool IsPauseMenuActive()
    {
        return isPauseMenuActive;
    }
    
    public void OnOptionsMenuClosed()
    {
        // Called when options menu is closed to show pause menu again
        ShowPauseMenu();
        
        if (debugMode)
        {
            Debug.Log("Options menu closed, showing pause menu");
        }
    }
    
    #endregion
    
    #region Input Handling
    
    // Removed Escape key handling - only P key pauses the game now
    
    #endregion
    
    #region Cleanup
    
    private void OnDestroy()
    {
        // Remove event listeners to prevent memory leaks
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }
        
        if (optionsButton != null)
        {
            optionsButton.onClick.RemoveListener(OnOptionsButtonClicked);
        }
        
        if (quitToMainMenuButton != null)
        {
            quitToMainMenuButton.onClick.RemoveListener(OnQuitToMainMenuButtonClicked);
        }
        
        // Unsubscribe from GameManager events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGamePause -= ShowPauseMenu;
            GameManager.Instance.OnGameResume -= HidePauseMenu;
        }
    }
    
    #endregion
}

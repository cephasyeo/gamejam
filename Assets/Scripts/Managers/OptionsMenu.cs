using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Button backButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private PauseMenu pauseMenu; // assign in Level1; optional in Main Menu
    [SerializeField] private MenuManager menuManager; // assign in Main Menu scene
    
    [Header("Menu Settings")]
    [SerializeField] private bool isFromMainMenu = true; // Set this in inspector based on context
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private void Awake()
    {
        // Start disabled - will be activated by parent menus when needed
        gameObject.SetActive(false);
    }
    
    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
        
        if (debugMode)
        {
            Debug.Log($"OptionsMenu initialized. isFromMainMenu: {isFromMainMenu}, menuManager: {menuManager}, pauseMenu: {pauseMenu}");
        }
    }
    
    private void OnEnable()
    {
        // Refresh UI when enabled
        RefreshSliderValues();
    }
    
    private void RefreshSliderValues()
    {
        // Update sliders with current volume values without triggering events
        if (AudioManager.Instance != null)
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MasterVolume);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);
            }
        }
    }
    
    private void InitializeUI()
    {
        // Initialize sliders with current volume values without triggering events
        if (AudioManager.Instance != null)
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MasterVolume);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);
            }
        }
    }
    
    private void SetupEventListeners()
    {
        // Master Volume Slider
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        
        // Music Volume Slider
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }
        
        // SFX Volume Slider
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
        
        // Back Button
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }
        
        // Reset Button
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
    }
    
    #region Volume Slider Events
    
    public void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }
        
        if (debugMode)
        {
            Debug.Log($"Master volume changed to: {value:F2}");
        }
    }
    
    public void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        
        if (debugMode)
        {
            Debug.Log($"Music volume changed to: {value:F2}");
        }
    }
    
    public void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
        
        if (debugMode)
        {
            Debug.Log($"SFX volume changed to: {value:F2}");
        }
    }
    
    #endregion
    
    #region Button Events
    
    private void OnBackButtonClicked()
    {
        if (debugMode)
        {
            Debug.Log($"Back button clicked. isFromMainMenu: {isFromMainMenu}, menuManager: {menuManager}, pauseMenu: {pauseMenu}");
        }
        
        if (isFromMainMenu)
        {
            // Main menu context: close options and show main menu
            if (menuManager != null)
            {
                menuManager.HideOptions();
            }
            else
            {
                // Fallback: just close options menu
                CloseOptionsMenu();
            }
        }
        else
        {
            // Pause menu context: hide options, pause menu should already be visible
            gameObject.SetActive(false);
            
            if (debugMode)
            {
                Debug.Log("Closing options menu in pause context");
            }
        }
    }
    
    public void OnResetButtonClicked()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ResetToDefaults();
            
            // Update sliders to reflect reset values without triggering events
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MasterVolume);
            }
            
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            }
            
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);
            }
        }
        
        if (debugMode)
        {
            Debug.Log("Reset button clicked - audio settings reset to defaults");
        }
    }
    
    #endregion
    
    #region Menu Navigation
    
    public void ReturnToMainMenu()
    {
        // If GameManager exists, use it. Otherwise, load directly
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene(0); // Load main menu directly
        }
        
        if (debugMode)
        {
            Debug.Log("Returning to main menu");
        }
    }
    
    public void CloseOptionsMenu()
    {
        // Deactivate this options menu
        gameObject.SetActive(false);
        
        if (debugMode)
        {
            Debug.Log("Options menu closed");
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public void SetFromMainMenu(bool fromMainMenu)
    {
        isFromMainMenu = fromMainMenu;
        
        if (debugMode)
        {
            Debug.Log($"Options menu context set to: {(fromMainMenu ? "Main Menu" : "Pause Menu")}");
        }
    }
    
    public void ShowOptionsMenu()
    {
        gameObject.SetActive(true);
        
        // Refresh UI with current values (but don't restart music)
        RefreshSliderValues();
        
        if (debugMode)
        {
            Debug.Log("Options menu shown");
        }
    }
    
    public void HideOptionsMenu()
    {
        gameObject.SetActive(false);
        
        if (debugMode)
        {
            Debug.Log("Options menu hidden");
        }
    }
    
    #endregion
    
    #region Cleanup
    
    private void OnDestroy()
    {
        // Remove event listeners to prevent memory leaks
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }
        
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }
        
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
        }
        
        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackButtonClicked);
        }
        
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(OnResetButtonClicked);
        }
    }
    
    #endregion
}

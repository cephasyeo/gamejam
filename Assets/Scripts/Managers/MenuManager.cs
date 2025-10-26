using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private int gameSceneIndex = 1; // Level1 scene index
    
    [Header("Main Menu Root")]
    [SerializeField] private GameObject mainMenuRoot; // Assign your main menu panel/container here
    
    [Header("Menu References")]
    [SerializeField] private GameObject optionsMenu;
    [SerializeField] private OptionsMenu optionsMenuScript;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    private bool isTransitioning = false;
    
    private void Awake()
    {
        // Reset transition flag when menu loads
        isTransitioning = false;
    }
    
    public void PlayGame()
    {
        // Prevent multiple calls
        if (isTransitioning)
        {
            if (debugMode)
            {
                Debug.LogWarning("PlayGame called but already transitioning - ignoring");
            }
            return;
        }
        
        isTransitioning = true;
        
        // Load Level1 directly from MainMenu (GameManager will be created there)
        SceneManager.LoadScene(gameSceneIndex);
        
        if (debugMode)
        {
            Debug.Log("Starting game - loading Level1");
        }
    }
    
    public void ShowOptions()
    {
        if (debugMode)
        {
            Debug.Log("ShowOptions called");
        }
        
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(true);
            
            // Configure options menu to know it's from main menu
            if (optionsMenuScript != null)
            {
                optionsMenuScript.SetFromMainMenu(true);
                if (debugMode)
                {
                    Debug.Log("Set options menu context to Main Menu");
                }
            }
        }
        
        // Hide main menu while options are open
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(false);
        }
        
        if (debugMode)
        {
            Debug.Log("Options menu opened from main menu");
        }
    }
    
    public void HideOptions()
    {
        if (debugMode)
        {
            Debug.Log("HideOptions called");
        }
        
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(false);
        }
        
        // Show main menu again when options are closed
        if (mainMenuRoot != null)
        {
            mainMenuRoot.SetActive(true);
        }
        
        if (debugMode)
        {
            Debug.Log("Options menu closed and main menu shown");
        }
    }
    
    public void LoadScene(int sceneIndex)
    {
        // If GameManager exists, use it. Otherwise, load directly
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(sceneIndex);
        }
        else
        {
            SceneManager.LoadScene(sceneIndex);
        }
    }
    
    public void LoadScene(string sceneName)
    {
        // If GameManager exists, use it. Otherwise, load directly
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    public void QuitGame()
    {
        if (debugMode)
        {
            Debug.Log("QuitGame called in MenuManager");
        }
        
        // If GameManager exists, use it. Otherwise, quit directly
        if (GameManager.Instance != null)
        {
            if (debugMode)
            {
                Debug.Log("GameManager found, delegating quit to GameManager");
            }
            GameManager.Instance.QuitGame();
        }
        else
        {
            if (debugMode)
            {
                Debug.Log("No GameManager found, quitting application directly");
            }
            
            #if UNITY_EDITOR
                if (debugMode)
                {
                    Debug.Log("Editor detected - stopping play mode");
                }
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                if (debugMode)
                {
                    Debug.Log("Build detected - calling Application.Quit()");
                }
                Application.Quit();
            #endif
        }
    }
    
    public void RestartGame()
    {
        // Only works if GameManager exists (i.e., in game scenes)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartCurrentScene();
        }
        
        if (debugMode)
        {
            Debug.Log("Restart game requested");
        }
    }
    
    public void ReturnToMainMenu()
    {
        // Only works if GameManager exists (i.e., in game scenes)
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
            Debug.Log("Return to main menu requested");
        }
    }
}
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
    
    public void PlayGame()
    {
        // Load Level1 directly from MainMenu (GameManager will be created there)
        SceneManager.LoadScene(gameSceneIndex);
        
        if (debugMode)
        {
            Debug.Log("Starting game - loading Level1");
        }
    }
    
    public void ShowOptions()
    {
        if (optionsMenu != null)
        {
            optionsMenu.SetActive(true);
            
            // Configure options menu to know it's from main menu
            if (optionsMenuScript != null)
            {
                optionsMenuScript.SetFromMainMenu(true);
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
            Debug.Log("Options menu closed");
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
        // If GameManager exists, use it. Otherwise, quit directly
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        if (debugMode)
        {
            Debug.Log("Quit game requested");
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
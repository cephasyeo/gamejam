using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryCanvasController : MonoBehaviour
{
    [Header("Victory Canvas Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private bool debugMode = true;
    
    private void Start()
    {
        // Ensure canvas starts hidden
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Called by Play Again button - reloads current scene
    /// </summary>
    public void PlayAgain()
    {
        if (debugMode)
        {
            Debug.Log("Play Again button clicked - reloading current scene");
        }
        
        // Resume time scale
        Time.timeScale = 1f;
        
        // Reload current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    
    /// <summary>
    /// Called by Main Menu button - loads main menu scene
    /// </summary>
    public void GoToMainMenu()
    {
        if (debugMode)
        {
            Debug.Log("Main Menu button clicked - loading main menu scene");
        }
        
        // Resume time scale
        Time.timeScale = 1f;
        
        // Load main menu scene
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    /// <summary>
    /// Shows the victory canvas and pauses the game
    /// </summary>
    public void ShowVictory()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f;
        
        if (debugMode)
        {
            Debug.Log("Victory canvas shown and game paused");
        }
    }
    
    /// <summary>
    /// Hides the victory canvas and resumes the game
    /// </summary>
    public void HideVictory()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        
        if (debugMode)
        {
            Debug.Log("Victory canvas hidden and game resumed");
        }
    }
}

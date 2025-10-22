using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [SerializeField] private int mainMenuSceneIndex = 0;
    [SerializeField] private float sceneTransitionDelay = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    // Events
    public System.Action OnGameStart;
    public System.Action OnGamePause;
    public System.Action OnGameResume;
    public System.Action OnGameOver;
    public System.Action OnSceneLoad;
    
    // Game state
    public bool IsGamePaused { get; private set; }
    public bool IsGameStarted { get; private set; }
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeGame()
    {
        IsGamePaused = false;
        IsGameStarted = false;
        
        if (debugMode)
        {
            Debug.Log("GameManager initialized");
        }
    }
    
    #region Scene Management
    
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadScene(nextSceneIndex);
        }
        else
        {
            Debug.LogWarning("No next scene available. Staying in current scene.");
            // Don't auto-load main menu - let the game continue
        }
    }
    
    public void LoadScene(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(LoadSceneCoroutine(sceneIndex));
        }
        else
        {
            Debug.LogError($"Invalid scene index: {sceneIndex}");
        }
    }
    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }
    
    public void LoadMainMenu()
    {
        LoadScene(mainMenuSceneIndex);
    }
    
    public void RestartCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        LoadScene(currentSceneIndex);
    }
    
    private IEnumerator LoadSceneCoroutine(int sceneIndex)
    {
        OnSceneLoad?.Invoke();
        
        if (debugMode)
        {
            Debug.Log($"Loading scene index: {sceneIndex}");
        }
        
        yield return new WaitForSeconds(sceneTransitionDelay);
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Reset game state when loading new scene
        IsGamePaused = false;
        IsGameStarted = false;
        
        if (debugMode)
        {
            Debug.Log($"Scene {sceneIndex} loaded successfully");
        }
    }
    
    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        OnSceneLoad?.Invoke();
        
        if (debugMode)
        {
            Debug.Log($"Loading scene: {sceneName}");
        }
        
        yield return new WaitForSeconds(sceneTransitionDelay);
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // Reset game state when loading new scene
        IsGamePaused = false;
        IsGameStarted = false;
        
        if (debugMode)
        {
            Debug.Log($"Scene {sceneName} loaded successfully");
        }
    }
    
    #endregion
    
    #region Game State Management
    
    public void StartGame()
    {
        IsGameStarted = true;
        OnGameStart?.Invoke();
        
        if (debugMode)
        {
            Debug.Log("Game started");
        }
    }
    
    public void PauseGame()
    {
        if (!IsGamePaused)
        {
            IsGamePaused = true;
            Time.timeScale = 0f;
            OnGamePause?.Invoke();
            
            if (debugMode)
            {
                Debug.Log("Game paused");
            }
        }
    }
    
    public void ResumeGame()
    {
        if (IsGamePaused)
        {
            IsGamePaused = false;
            Time.timeScale = 1f;
            OnGameResume?.Invoke();
            
            if (debugMode)
            {
                Debug.Log("Game resumed");
            }
        }
    }
    
    public void TogglePause()
    {
        if (IsGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }
    
    public void GameOver()
    {
        OnGameOver?.Invoke();
        
        if (debugMode)
        {
            Debug.Log("Game Over");
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    public void QuitGame()
    {
        if (debugMode)
        {
            Debug.Log("Quitting game");
        }
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    public int GetCurrentSceneIndex()
    {
        return SceneManager.GetActiveScene().buildIndex;
    }
    
    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }
    
    public bool IsMainMenuScene()
    {
        return GetCurrentSceneIndex() == mainMenuSceneIndex;
    }
    
    #endregion
    
    #region Input Handling
    
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        
        // Handle pause input (P key) - only in game scenes
        if (keyboard.pKey.wasPressedThisFrame && !IsMainMenuScene())
        {
            TogglePause();
        }
        
        // Handle restart input (R key)
        if (keyboard.rKey.wasPressedThisFrame && !IsMainMenuScene())
        {
            RestartCurrentScene();
        }
    }
    
    #endregion
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelTrigger : MonoBehaviour
{
    [Header("Next Level Settings")]
    [SerializeField] private bool debugMode = true;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player touched the trigger
        if (other.CompareTag("Player"))
        {
            LoadNextLevel();
        }
    }
    
    private void LoadNextLevel()
    {
        // Get current scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        
        // Calculate next scene index
        int nextSceneIndex = currentSceneIndex + 1;
        
        // Check if next scene exists in build settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            if (debugMode)
            {
                Debug.Log($"Loading next level: Scene index {nextSceneIndex}");
            }
            
            // Load next scene
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            if (debugMode)
            {
                Debug.LogWarning("NextLevelTrigger: No next scene found in build settings!");
            }
            
            // Fallback: Load scene 0 (main menu) or show message
            Debug.Log("This is the last level! Loading main menu...");
            SceneManager.LoadScene(0); // Load first scene (usually main menu)
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}

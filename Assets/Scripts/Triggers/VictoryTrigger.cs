using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryTrigger : MonoBehaviour
{
    [Header("Victory Settings")]
    [SerializeField] private GameObject victoryCanvas;
    [SerializeField] private bool debugMode = true;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if player touched the trigger
        if (other.CompareTag("Player"))
        {
            TriggerVictory();
        }
    }
    
    private void TriggerVictory()
    {
        if (victoryCanvas != null)
        {
            // Show victory canvas
            victoryCanvas.SetActive(true);
            
            // Pause the game
            Time.timeScale = 0f;
            
            if (debugMode)
            {
                Debug.Log("Victory triggered! Game paused.");
            }
        }
        else
        {
            Debug.LogError("VictoryTrigger: Victory canvas is not assigned!");
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }
}

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 5f;
    
    [Header("Boundaries")]
    [SerializeField] private bool useBoundaries = true;
    [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
    [SerializeField] private Vector2 maxBounds = new Vector2(10f, 5f);
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = true;
    
    private Camera cam;
    private Vector3 targetPosition;
    private Vector3 velocity = Vector3.zero;
    
    // Simple camera following
    
    private void Awake()
    {
        cam = GetComponent<Camera>();
        
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        
        // Initialize camera position
        if (player != null)
        {
            transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
        }
    }
    
    private void Start()
    {
        // Set initial camera position
        if (player != null)
        {
            transform.position = new Vector3(player.position.x, player.position.y, transform.position.z);
        }
    }
    
    private void FixedUpdate()
    {
        if (player == null) return;
        
        CalculateTargetPosition();
        ApplyBoundaries();
        MoveCamera();
    }
    
    private void CalculateTargetPosition()
    {
        // Simply center the player on screen
        targetPosition = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z
        );
    }
    
    private void ApplyBoundaries()
    {
        if (!useBoundaries) return;
        
        // Get camera bounds
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;
        
        // Clamp position to boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, 
            minBounds.x + camWidth, 
            maxBounds.x - camWidth);
            
        targetPosition.y = Mathf.Clamp(targetPosition.y, 
            minBounds.y + camHeight, 
            maxBounds.y - camHeight);
    }
    
    private void MoveCamera()
    {
        // Use FixedUpdate timing for smoother movement
        float deltaTime = Time.fixedDeltaTime;
        
        // Smooth camera movement with better damping
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / followSpeed, Mathf.Infinity, deltaTime);
    }
    
    // Public methods for external control
    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
    
    public void SetBoundaries(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
    }
    
    // Debug visualization
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;
        
        // Draw boundaries
        if (useBoundaries)
        {
            Gizmos.color = Color.red;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
        
        // Draw camera view
        if (cam != null)
        {
            Gizmos.color = Color.yellow;
            float height = cam.orthographicSize;
            float width = height * cam.aspect;
            Gizmos.DrawWireCube(transform.position, new Vector3(width * 2, height * 2, 0));
        }
        
        // Draw target position
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(targetPosition, 0.5f);
    }
}

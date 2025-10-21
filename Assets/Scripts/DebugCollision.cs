using UnityEngine;

public class DebugCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Player hit: {other.name}");
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Player collided with: {collision.gameObject.name}");
    }
    
    private void Update()
    {
        // Debug ground detection
        var hit = Physics2D.Raycast(transform.position, Vector2.down, 2f);
        if (hit)
        {
            Debug.DrawRay(transform.position, Vector2.down * hit.distance, Color.green);
        }
        else
        {
            Debug.DrawRay(transform.position, Vector2.down * 2f, Color.red);
        }
    }
}

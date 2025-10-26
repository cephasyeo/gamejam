using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerTargetingOrbShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shootInterval = 1f;
    public float orbSpeed = 10f;
    public bool startShootingOnAwake = true;
    public bool loopShooting = true;

    [Header("Targeting Settings")]
    [Tooltip("If not assigned in inspector, the script will try to find the player by tag 'Player'")]
    public Transform playerTransform;
    public float targetingRange = 10f;
    [Header("Stop Shooting Settings")]
    public bool useStopThreshold = false;
    public float stopShootingXThreshold = 9999f;

    [Header("Orb Sequence")]
    public List<OrbType> orbSequence = new List<OrbType> { OrbType.White };
    private int currentSequenceIndex = 0;

    [Header("Orb Prefabs")]
    public GameObject whiteOrbPrefab;
    public GameObject purpleOrbPrefab;

    [Header("Debug")]
    public bool debugMode = true; // Enable by default to help debug

    private bool isShooting = false;
    private Coroutine shootingCoroutine;
    private Vector2 lastKnownPlayerPosition;
    private bool playerInRange = false;

    public enum OrbType { Red, Green, White, Purple }

    private void Start()
    {
        // Try to find player by tag if not assigned
        if (playerTransform == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) playerTransform = go.transform;
        }

        if (startShootingOnAwake)
            StartShooting();
    }

    private void Update()
    {
        UpdatePlayerTracking();
    }

    private void UpdatePlayerTracking()
    {
        if (playerTransform == null)
        {
            playerInRange = false;
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        playerInRange = dist <= targetingRange;

        if (playerInRange)
        {
            lastKnownPlayerPosition = playerTransform.position;
            if (debugMode) Debug.DrawLine(transform.position, lastKnownPlayerPosition, Color.red);
        }
        
        if (debugMode)
        {
            Debug.Log($"PlayerTargetingOrbShooter: Player distance: {dist:F2}, In range: {playerInRange}, Targeting range: {targetingRange}");
        }
    }

    public void StartShooting()
    {
        if (isShooting) return;
        if (orbSequence == null || orbSequence.Count == 0) 
        {
            if (debugMode) Debug.LogWarning("PlayerTargetingOrbShooter: orbSequence is null or empty!");
            return;
        }
        if (purpleOrbPrefab == null && orbSequence.Contains(OrbType.Purple)) 
        {
            if (debugMode) Debug.LogWarning("PlayerTargetingOrbShooter: purpleOrbPrefab is null!");
            return;
        }
        if (whiteOrbPrefab == null && orbSequence.Contains(OrbType.White)) 
        {
            if (debugMode) Debug.LogWarning("PlayerTargetingOrbShooter: whiteOrbPrefab is null!");
            return;
        }

        isShooting = true;
        shootingCoroutine = StartCoroutine(ShootingLoop());

        if (debugMode) Debug.Log($"PlayerTargetingOrbShooter: started shooting. Sequence: {string.Join(", ", orbSequence)}");
    }
    
    /// <summary>
    /// Public method to force update player tracking
    /// </summary>
    public void ForceUpdatePlayerTracking()
    {
        UpdatePlayerTracking();
    }

    public void StopShooting()
    {
        if (!isShooting) return;
        isShooting = false;

        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }

        if (debugMode) Debug.Log("PlayerTargetingOrbShooter: stopped shooting.");
    }

    public void SetShootInterval(float newInterval)
    {
        shootInterval = Mathf.Max(0.01f, newInterval);

        if (isShooting)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = StartCoroutine(ShootingLoop());
        }
    }

    public void SetPurpleOrbPrefab(GameObject purplePrefab)
    {
        purpleOrbPrefab = purplePrefab;
        
        // Change orb sequence to only shoot purple orbs
        orbSequence.Clear();
        orbSequence.Add(OrbType.Purple);
        
        if (debugMode)
        {
            Debug.Log($"PlayerTargetingOrbShooter: Set purple orb prefab and updated sequence");
        }
    }
    
    public void SetOrbSpeed(float newSpeed)
    {
        orbSpeed = newSpeed;
    }

    private IEnumerator ShootingLoop()
    {
        if (debugMode) Debug.Log($"PlayerTargetingOrbShooter: Starting shooting loop. Player in range: {playerInRange}");
        
        while (isShooting)
        {
            if (useStopThreshold && playerTransform != null)
            {
                if (debugMode) Debug.Log($"PlayerTargetingOrbShooter: Checking threshold. Player x: {playerTransform.position.x}, Threshold: {stopShootingXThreshold}");
                
                if (playerTransform.position.x > stopShootingXThreshold)
                {
                    if (debugMode) Debug.Log($"PlayerTargetingOrbShooter: Player x ({playerTransform.position.x}) passed threshold ({stopShootingXThreshold}), stopping shooting.");
                    StopShooting();
                    yield break;
                }
            }
            
            if (playerInRange)
            {
                ShootOrbAtLastKnownPosition();
                currentSequenceIndex = (currentSequenceIndex + 1) % orbSequence.Count;
                
                if (debugMode)
                {
                    Debug.Log($"PlayerTargetingOrbShooter: Shot orb! Player in range: {playerInRange}, Distance: {Vector2.Distance(transform.position, playerTransform.position):F2}");
                }
            }
            else if (debugMode)
            {
                Debug.Log($"PlayerTargetingOrbShooter: Not shooting - player not in range. Distance: {Vector2.Distance(transform.position, playerTransform.position):F2}, Range: {targetingRange}");
            }

            yield return new WaitForSeconds(shootInterval);

            if (!loopShooting && currentSequenceIndex == 0)
            {
                StopShooting();
                yield break;
            }
        }
    }

    private void ShootOrbAtLastKnownPosition()
    {
        OrbType orbType = orbSequence[currentSequenceIndex];
        GameObject prefab = GetOrbPrefab(orbType);
        if (prefab == null)
        {
            if (debugMode) Debug.LogWarning($"PlayerTargetingOrbShooter: prefab for {orbType} is null");
            return;
        }

        // Instantiate and ensure visible Z plane
        Vector3 spawnPos = transform.position;
        spawnPos.z = 0f; // force Z to 0 (camera looks at Z=0)
        GameObject orb = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Determine direction toward last known player position
        Vector2 direction = (lastKnownPlayerPosition - (Vector2)spawnPos);
        if (direction.sqrMagnitude < 0.0001f)
        {
            // fallback direction (down) if positions coincide
            direction = Vector2.down;
        }
        direction.Normalize();

        // Fix rotation so sprite remains visible (only Z rotation)
        orb.transform.rotation = Quaternion.identity;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        orb.transform.Rotate(0f, 0f, angle);

        // Check if it's a PurpleOrb
        PurpleOrb purpleOrbScript = orb.GetComponent<PurpleOrb>();
        if (purpleOrbScript != null)
        {
            // Initialize the purple orb with movement
            purpleOrbScript.Initialize(direction, orbSpeed);
        }
        else
        {
            // Try Orb component for other orb types
            Orb orbScript = orb.GetComponent<Orb>();
            if (orbScript != null)
            {
                orbScript.SetOrbStats(orbScript.GetOrbStats()); // preserve appearance if needed
                orbScript.isShooterOrb = true;
                orbScript.moveDirection = direction;
                orbScript.moveSpeed = orbSpeed;
            }
            else
            {
                if (debugMode) Debug.LogWarning("PlayerTargetingOrbShooter: spawned orb prefab has no Orb or PurpleOrb component");
                // As a fallback, move the transform manually via a temporary component
                orb.AddComponent<SimpleMover>().Init(direction, orbSpeed);
            }
        }

        // Ensure sprite renders above background (optional)
        SpriteRenderer sr = orb.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 10;
        }

        if (debugMode)
        {
            string orbTypeName = orb.GetComponent<PurpleOrb>() != null ? "purple" : "orb";
            Debug.Log($"PlayerTargetingOrbShooter: fired {orbTypeName} orb toward {lastKnownPlayerPosition} (dir {direction})");
        }
    }

    private GameObject GetOrbPrefab(OrbType type)
    {
        switch (type)
        {
            case OrbType.White: return whiteOrbPrefab;
            case OrbType.Purple: return purpleOrbPrefab;
            default: return null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = playerInRange ? Color.white : Color.gray;
        Gizmos.DrawWireSphere(transform.position, targetingRange);
    }

    // tiny helper mover in case orb prefab lacks Orb — kept private and minimal
    private class SimpleMover : MonoBehaviour
    {
        Vector2 dir;
        float spd;
        public void Init(Vector2 d, float s) { dir = d; spd = s; }
        private void Update() { transform.position += (Vector3)(dir * spd * Time.deltaTime); }
        private void OnBecameInvisible() { Destroy(gameObject); }
    }
}

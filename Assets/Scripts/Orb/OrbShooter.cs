using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OrbShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float shootInterval = 1f;
    public float orbSpeed = 10f;
    public bool startShootingOnAwake = true;
    public bool loopShooting = true;

    [Header("Shooting Direction")]
    public Vector2 shootDirection = Vector2.down;
    public bool normalizeDirection = true;

    [Header("Orb Sequence")]
    public List<OrbType> orbSequence = new List<OrbType> { OrbType.Red, OrbType.Green };
    private int currentSequenceIndex = 0;

    [Header("Orb Prefabs")]
    public GameObject redOrbPrefab;
    public GameObject greenOrbPrefab;
    public GameObject whiteOrbPrefab;

    private bool isShooting = false;
    private Coroutine shootingCoroutine;

    public enum OrbType { Red, Green, White }

    private void Start()
    {
        if (normalizeDirection)
            shootDirection = shootDirection.normalized;

        if (startShootingOnAwake)
            StartShooting();
    }

    /// <summary>
    /// Starts shooting orbs.
    /// </summary>
    public void StartShooting()
    {
        if (isShooting) return;
        if (orbSequence.Count == 0) return;
        if (redOrbPrefab == null || greenOrbPrefab == null || whiteOrbPrefab == null) return;

        isShooting = true;
        shootingCoroutine = StartCoroutine(ShootingLoop());
    }

    /// <summary>
    /// Stops shooting orbs.
    /// </summary>
    public void StopShooting()
    {
        if (!isShooting) return;
        isShooting = false;

        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    /// <summary>
    /// Updates shoot interval at runtime and applies it immediately.
    /// </summary>
    public void SetShootInterval(float newInterval)
    {
        shootInterval = newInterval;
        // restart coroutine if shooting to immediately apply change
        if (isShooting)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = StartCoroutine(ShootingLoop());
        }
    }

    /// <summary>
    /// Updates orb speed at runtime.
    /// </summary>
    public void SetOrbSpeed(float newSpeed)
    {
        orbSpeed = newSpeed;
    }

    private IEnumerator ShootingLoop()
    {
        while (isShooting)
        {
            ShootOrb();
            currentSequenceIndex = (currentSequenceIndex + 1) % orbSequence.Count;

            // Dynamic wait so updated shootInterval is applied immediately
            float timer = 0f;
            while (timer < shootInterval)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (!loopShooting && currentSequenceIndex == 0)
            {
                StopShooting();
                break;
            }
        }
    }

    private void ShootOrb()
    {
        OrbType orbType = orbSequence[currentSequenceIndex];
        GameObject prefab = GetOrbPrefab(orbType);
        if (prefab == null) return;

        GameObject orb = Instantiate(prefab, transform.position, Quaternion.identity);

        Orb orbScript = orb.GetComponent<Orb>();
        if (orbScript != null)
        {
            orbScript.SetOrbStats(orbScript.GetOrbStats()); // keep appearance
            orbScript.isShooterOrb = true;                  // enable movement
            orbScript.moveDirection = shootDirection.normalized;
            orbScript.moveSpeed = orbSpeed;
        }

        // Rotate to face shooting direction
        float angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg;
        orb.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    private GameObject GetOrbPrefab(OrbType type)
    {
        switch (type)
        {
            case OrbType.Red: return redOrbPrefab;
            case OrbType.Green: return greenOrbPrefab;
            case OrbType.White: return whiteOrbPrefab;
            default: return null;
        }
    }
}